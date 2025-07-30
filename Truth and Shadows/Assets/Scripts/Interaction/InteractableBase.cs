using System;
using System.Collections;
using System.Linq;
using Cinemachine;
using TruthAndShadows.InputSystem;
using TruthAndShadows.Interaction;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// Base class for objects that can be interacted with. Provides common functionality
    /// and optional camera support.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField]
        protected bool requireContinuousHold = false;

        [SerializeField]
        protected float interactionDistance = 5f;

        [SerializeField]
        protected float pickupDistance = 5f;

        [SerializeField]
        protected bool useColliderBounds = true;

        [Header("Pickup Settings")]
        [SerializeField]
        protected bool canBePickedUp = true;

        [SerializeField]
        protected float pickupRaiseAmount = 0.2f;

        [SerializeField]
        protected float pickupSmoothness = 10f;

        [SerializeField]
        protected float pickupMovementSmoothing = 15f;

        [Header("Pickup Movement")]
        [SerializeField]
        protected float pickupMoveSpeed = 2.5f;

        [SerializeField]
        protected float minPlayerDistance = 0f;

        [Header("Player Facing Settings")]
        [Tooltip("How fast the player rotates to face the picked up object (degrees per second)")]
        [SerializeField]
        protected float playerRotationSpeed = 360f; // Degrees per second, can be set in Inspector

        //For Object interactable Sound
        //Rashai was here
        public AudioSource source;
        public AudioClip pickUpClip;

        [Header("Camera Settings")]
        [SerializeField]
        protected Component interactionCamera;

        [Header("Outline Settings")]
        [SerializeField]
        protected bool enableOutline = true;

        [SerializeField]
        protected Color outlineColor = new Color(0.2f, 0.6f, 1f, 1f); // Bright blue

        [SerializeField]
        protected float outlineWidth = 10f; // Double the previous size

        protected Transform playerTransform;
        protected Vector3 originalPosition;
        protected Quaternion originalRotation;
        protected Transform originalParent;
        protected Rigidbody rigidBody;
        protected Vector3 relativePosition;
        protected bool hasCalculatedRelativePosition = false;
        protected Vector3 lastPlayerPosition;

        // QuickOutline system variables
        protected Outline[] outlineComponents;
        protected Coroutine outlineFadeCoroutine;
        protected bool outlineShouldBeVisible = false;
        protected float outlineFadeDuration = 1f;

        [Header("Outline Particle Effect")]
        [SerializeField]
        protected ParticleSystem outlineParticlePrefab;
        protected ParticleSystem outlineParticlesInstance;
        public virtual bool RequiresContinuousInteraction => requireContinuousHold;
        public virtual Component InteractionCamera => interactionCamera;
        public virtual bool CanBePickedUp => canBePickedUp;
        public virtual bool IsPickedUp { get; protected set; }
        protected virtual bool PickupIsKinematic => true;

        // --- Laser Visual Aid fields ---
        [Header("Pickup Laser Visual Aid")]
        [SerializeField]
        protected LaserBetweenPoints laserPrefab;

        // Remove per-instance laserInstance and laserEndPointDummy
        //protected Hovl_LaserBetweenPoints laserInstance;
        //protected Transform laserEndPointDummy; // Used as a movable end point
        protected Collider interactableCollider;
        public Collider InteractableCollider => interactableCollider; // Public getter
        protected Animator playerAnimator;
        protected Camera mainCamera;
        protected CharacterAnimation playerCharacterAnimation;

        // --- Static reference to the currently held interactable ---
        public static InteractableBase CurrentlyHeldInteractable { get; private set; }

        private static GameObject freelookMainCharacter;

        /// <summary>
        /// Event triggered when interaction begins
        /// </summary>
        public event Action<GameObject> OnInteractionStarted;

        /// <summary>
        /// Event triggered when interaction ends
        /// </summary>
        public event Action<GameObject> OnInteractionEnded;

        /// <summary>
        /// Determines if this interactable can currently be interacted with based on custom conditions
        /// </summary>
        /// <param name="player">The player attempting to interact</param>
        /// <returns>True if interaction conditions are met, false otherwise</returns>
        public virtual bool CanInteract(Vector3 playerPosition)
        {
            float centerDistance = Vector3.Distance(transform.position, playerPosition);

            if (objectRenderer != null)
            {
                Vector3 closestPoint = objectRenderer.bounds.ClosestPoint(playerPosition);
                float boundsDistance = Vector3.Distance(playerPosition, closestPoint);

                return Mathf.Min(centerDistance, boundsDistance) <= interactionDistance;
            }
            else if (interactableCollider != null)
            {
                // Account for collider center offset
                Vector3 actualColliderCenter =
                    transform.position + transform.TransformDirection(colliderCenterOffset);

                // Get closest point on collider from player
                Vector3 closestPoint = interactableCollider.ClosestPoint(playerPosition);
                float boundsDistance = Vector3.Distance(playerPosition, closestPoint);

                return Mathf.Min(centerDistance, boundsDistance) <= interactionDistance;
            }

            return centerDistance <= interactionDistance;
        }

        /// <summary>
        /// Raises the OnInteractionStarted event
        /// </summary>
        /// <param name="interactor">The GameObject that initiated the interaction</param>
        protected virtual void RaiseInteractionStartedEvent(GameObject interactor)
        {
            OnInteractionStarted?.Invoke(interactor);
        }

        /// <summary>
        /// Raises the OnInteractionEnded event
        /// </summary>
        /// <param name="interactor">The GameObject that initiated the interaction</param>
        protected virtual void RaiseInteractionEndedEvent(GameObject interactor)
        {
            OnInteractionEnded?.Invoke(interactor);
        }

        protected virtual void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
            interactableCollider = GetComponent<Collider>();

            // Calculate the collider center offset
            UpdateColliderCenterOffset();

            // Add Outline to all renderers in children
            var renderers = GetComponentsInChildren<Renderer>();
            outlineComponents = new Outline[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                var outline = renderers[i].gameObject.GetComponent<Outline>();
                if (outline == null)
                    outline = renderers[i].gameObject.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineColor = outlineColor;
                outline.OutlineWidth = outlineWidth;
                outline.enabled = false;
                outlineComponents[i] = outline;
            }

            // Auto-assign OutlineAuraParticles prefab if not set
            if (outlineParticlePrefab == null)
            {
                outlineParticlePrefab = Resources.Load<ParticleSystem>("OutlineAuraParticles");
            }
            if (outlineParticlePrefab != null)
            {
                outlineParticlesInstance = Instantiate(outlineParticlePrefab, transform);
                // Dynamically set radius based on object bounds
                Bounds bounds = default;
                var rend = GetComponent<Renderer>();
                if (rend != null)
                    bounds = rend.bounds;
                else
                {
                    var childRend = GetComponentInChildren<Renderer>();
                    if (childRend != null)
                        bounds = childRend.bounds;
                }
                float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                var shape = outlineParticlesInstance.shape;
                shape.radius = maxExtent * 1.1f;
                // Center the particle system on the object's bounds
                Vector3 centerWorld = bounds.center;
                Vector3 centerLocal = transform.InverseTransformPoint(centerWorld);
                outlineParticlesInstance.transform.localPosition = centerLocal;
                var main = outlineParticlesInstance.main;
                main.startColor = outlineColor;
                outlineParticlesInstance.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }

            // Find the main camera
            freelookMainCharacter = GameObject.Find("FreeLookMainCharacter");

            // Initialize the renderer for bounds-based checks
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer == null)
            {
                objectRenderer = GetComponentInChildren<Renderer>();
            }

            UpdateColliderCenterOffset(); // Calculate the offset at start
        }

        public abstract void StartInteraction();

        public virtual void ContinueInteraction() { }

        public virtual void EndInteraction() { }

        public virtual void StartPickup(Transform playerTransform)
        {
            if (!canBePickedUp || IsPickedUp)
                return;

            // Check permissions from the centralized provider
            bool canPickup;

            // Get permission from InputContextProvider if available
            if (InputContextProvider.Instance != null)
            {
                canPickup = InputContextProvider.Instance.CanPickup;

                if (!canPickup)
                {
                    Debug.LogWarning(
                        "Interactable pickup attempted but permission denied by InputContextProvider"
                    );
                    return; // Don't proceed with pickup if not allowed
                }
            }

            this.playerTransform = playerTransform;
            IsPickedUp = true;
            CurrentlyHeldInteractable = this; // Set static reference
            hasCalculatedRelativePosition = false;
            lastPlayerPosition = playerTransform.position;

            // Recalculate collider center offset to ensure accuracy
            UpdateColliderCenterOffset();

            // --- Disable player movement when picked up ---
            var playerMovement =
                playerTransform.GetComponent<TruthAndShadows.Player.PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.canMove = false;
            }
            // --- Optionally, update InputContextProvider permissions ---
            var contextProvider = TruthAndShadows.InputSystem.InputContextProvider.Instance;
            if (contextProvider != null)
            {
                contextProvider.ForcePermission("move", false);
            }

            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalParent = transform.parent;

            // --- Get CharacterAnimation and set pushing ---
            playerCharacterAnimation = playerTransform.GetComponent<CharacterAnimation>();
            if (playerCharacterAnimation != null)
                playerCharacterAnimation.SetPushing(true);

            if (source != null && pickUpClip != null)
            {
                source.PlayOneShot(pickUpClip);
            }

            if (rigidBody != null)
            {
                rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
                rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidBody.constraints =
                    RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

                // Reset velocities for a clean pickup state
                rigidBody.velocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;
            }

            // Apply pickup raise if specified
            if (pickupRaiseAmount > 0)
            {
                Vector3 raisedPosition = transform.position + Vector3.up * pickupRaiseAmount;
                transform.position = raisedPosition;
            }

            Debug.Log($"Picked up: {gameObject.name}");
        }

        public virtual void EndPickup()
        {
            if (!IsPickedUp)
                return;

            IsPickedUp = false;
            if (CurrentlyHeldInteractable == this)
            {
                CurrentlyHeldInteractable = null; // Clear static reference
            }
            hasCalculatedRelativePosition = false;

            // --- Re-enable player movement when dropped ---
            if (playerTransform != null)
            {
                var movement =
                    playerTransform.GetComponent<TruthAndShadows.Player.PlayerMovement>();
                if (movement != null)
                {
                    movement.canMove = true;
                }
            }
            // --- Optionally, update InputContextProvider permissions ---
            var contextProvider = TruthAndShadows.InputSystem.InputContextProvider.Instance;
            if (contextProvider != null)
            {
                contextProvider.ForcePermission("move", true);
            }

            transform.SetParent(originalParent);

            if (playerCharacterAnimation != null)
                playerCharacterAnimation.SetPushing(false);
            playerCharacterAnimation = null;

            if (source != null)
            {
                source.Stop();
            }

            if (rigidBody != null)
            {
                rigidBody.isKinematic = false;
                rigidBody.useGravity = true;
                rigidBody.interpolation = RigidbodyInterpolation.None;
                rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
                rigidBody.velocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;
            }

            playerTransform = null;

            Debug.Log($"Dropped: {gameObject.name}");
        }

        protected virtual void FixedUpdate()
        {
            // Update pickup position in FixedUpdate for better physics
            if (IsPickedUp && playerTransform != null)
            {
                // Drop object if too far - Use XZ-plane distance
                float distanceToPlayer;

                // Zero out Y values for XZ-plane distance calculation
                Vector3 playerPositionXZ = playerTransform.position;
                Vector3 objectPositionXZ = transform.position;
                playerPositionXZ.y = 0;
                objectPositionXZ.y = 0;

                if (interactableCollider != null)
                {
                    // Project player position to same Y level as collider center for closest point calculation
                    Vector3 sameYLevelPosition = playerTransform.position;

                    // Account for collider center offset relative to the object position
                    Vector3 actualColliderCenter =
                        transform.position + transform.TransformDirection(colliderCenterOffset);
                    sameYLevelPosition.y = actualColliderCenter.y;

                    // Get closest point on collider
                    Vector3 closestPoint = interactableCollider.ClosestPoint(sameYLevelPosition);

                    // Calculate XZ distance
                    Vector3 closestPointXZ = closestPoint;
                    closestPointXZ.y = 0;
                    distanceToPlayer = Vector3.Distance(playerPositionXZ, closestPointXZ);
                }
                else
                {
                    // Fallback to center-to-center XZ distance
                    distanceToPlayer = Vector3.Distance(playerPositionXZ, objectPositionXZ);
                }
                if (distanceToPlayer > pickupDistance)
                {
                    EndPickup();
                    return; // Stop further processing
                }
                UpdatePickupPosition();

                if (playerTransform != null)
                {
                    Vector3 lookAtPosition = transform.position;
                    lookAtPosition.y = playerTransform.position.y; // Keep y level
                    Quaternion targetRotation = Quaternion.LookRotation(
                        lookAtPosition - playerTransform.position
                    );
                    playerTransform.rotation = Quaternion.RotateTowards(
                        playerTransform.rotation,
                        targetRotation,
                        playerRotationSpeed * Time.deltaTime
                    );
                }
            }
        }

        protected virtual void UpdatePickupPosition()
        {
            // Use camera-relative movement for telekinesis
            Camera cam = Camera.main;

            // Get input directly from InputManager (not blocked by player movement permission)
            var inputManager = TruthAndShadows.InputSystem.InputManager.Instance;
            Vector2 moveInput =
                inputManager != null ? inputManager.InteractableMoveInput : Vector2.zero;

            // Camera forward/right, flattened
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0;
            camForward.Normalize();
            Vector3 camRight = cam.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            // Calculate movement direction
            Vector3 moveDir = camRight * moveInput.x + camForward * moveInput.y;

            Vector3 updateDistance = moveDir * pickupMoveSpeed * pickupMoveSpeed * Time.deltaTime;
            Vector3 targetPos = transform.position;
            targetPos.x += updateDistance.x;
            targetPos.z += updateDistance.z;
            // Y position is explicitly preserved - no vertical movement during pickup

            // prevent movement if too close to interaction max distance
            // or if too close to the player
            if (playerTransform != null)
            {
                // Calculate distance from the player to the object in XZ-plane only
                float distanceToPlayer;

                // Zero out Y values for XZ-plane distance calculation
                Vector3 playerPositionXZ = playerTransform.position;
                Vector3 targetPosXZ = targetPos;
                playerPositionXZ.y = 0;
                targetPosXZ.y = 0;

                if (interactableCollider != null)
                {
                    // Project player position to same Y level as collider center for closest point calculation
                    Vector3 sameYLevelPosition = playerTransform.position;

                    // Account for collider center offset relative to the object position
                    Vector3 actualColliderCenter =
                        transform.position + transform.TransformDirection(colliderCenterOffset);
                    sameYLevelPosition.y = actualColliderCenter.y;

                    // Get closest point on collider
                    Vector3 closestPoint = interactableCollider.ClosestPoint(sameYLevelPosition);

                    // Calculate XZ distance
                    Vector3 closestPointXZ = closestPoint;
                    closestPointXZ.y = 0;
                    distanceToPlayer = Vector3.Distance(playerPositionXZ, closestPointXZ);
                }
                else
                {
                    // Fallback to center-to-center XZ distance
                    distanceToPlayer = Vector3.Distance(playerPositionXZ, targetPosXZ);
                }

                // Use pickupDistance to determine maximum distance for moving the object
                if (distanceToPlayer >= pickupDistance)
                {
                    // If too far from player, don't move
                    return;
                }

                // Prevent moving closer if colliding with player - enhanced check
                Collider playerCollider = playerTransform.GetComponent<Collider>();
                if (playerCollider != null && interactableCollider != null)
                {
                    // First check if we would penetrate the player with the new position
                    bool wouldPenetrate = Physics.ComputePenetration(
                        interactableCollider,
                        targetPos,
                        transform.rotation,
                        playerCollider,
                        playerTransform.position,
                        playerTransform.rotation,
                        out Vector3 direction,
                        out float distance
                    );

                    if (wouldPenetrate)
                    {
                        // If significant penetration, adjust position to prevent it
                        if (distance > 0.01f) // More sensitive threshold
                        {
                            // Move away from the player by the penetration distance plus a small buffer
                            targetPos = transform.position + (direction * (distance + 0.05f));

                            // Double-check our adjustment with another penetration test
                            if (
                                Physics.ComputePenetration(
                                    interactableCollider,
                                    targetPos,
                                    transform.rotation,
                                    playerCollider,
                                    playerTransform.position,
                                    playerTransform.rotation,
                                    out Vector3 _, // We don't need these values for the second check
                                    out float _
                                )
                            )
                            {
                                // If still penetrating after adjustment, don't move at all
                                targetPos = transform.position;
                            }
                        }
                    }

                    // Calculate the post-movement distance to player (center to center)
                    float distToPlayerCenter = Vector3.Distance(
                        targetPos,
                        playerTransform.position
                    );

                    if (distToPlayerCenter < minPlayerDistance)
                    {
                        // Simple adjustment to maintain minimum distance
                        Vector3 dirToPlayer = (targetPos - playerTransform.position).normalized;
                        targetPos = playerTransform.position + dirToPlayer * minPlayerDistance;
                    }
                }

                // Calculate the smoothed position for movement
                Vector3 lerpedPosition = Vector3.Lerp(
                    transform.position,
                    targetPos,
                    Time.deltaTime * pickupSmoothness
                );

                // Handle collision checking with the environment
                CheckAndAdjustForCollisions(ref lerpedPosition);

                // Apply the final position
                transform.position = lerpedPosition;

                // Optionally, update rigidbody position if kinematic
                if (rigidBody.isKinematic)
                {
                    rigidBody.position = transform.position;
                }
            }
        }

        // Enhanced method to handle collision checks with improved accuracy
        protected virtual void CheckAndAdjustForCollisions(ref Vector3 targetPosition)
        {
            // Check for collisions with environment (not player) that might block movement
            if (interactableCollider == null || rigidBody == null)
                return;

            // Do a small check in the direction we're trying to move
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            float moveDistance = Vector3.Distance(transform.position, targetPosition);

            // Only check if we're actually trying to move
            if (moveDistance <= 0.01f)
                return;

            // Use a layermask to exclude the player layer if needed
            int layerMask = Physics.DefaultRaycastLayers;
            if (playerTransform != null)
            {
                // Exclude the player's layer from our collision check
                layerMask &= ~(1 << playerTransform.gameObject.layer);
            }

            // Cast the collider in the movement direction to see if we'd hit anything
            // Calculate actual collider center accounting for offset
            Vector3 actualColliderCenter =
                transform.position + transform.TransformDirection(colliderCenterOffset);

            bool hitSomething = Physics.BoxCast(
                actualColliderCenter,
                interactableCollider.bounds.extents * 0.9f, // Slightly smaller to avoid edge cases
                moveDirection,
                out RaycastHit hitInfo,
                transform.rotation,
                moveDistance,
                layerMask,
                QueryTriggerInteraction.Ignore
            );

            if (!hitSomething)
                return;

            // If we hit something, make sure it's not the player (extra safety check)
            if (
                playerTransform != null
                && hitInfo.collider.gameObject == playerTransform.gameObject
            )
                return;

            // Stop slightly before the collision point to prevent pushing through walls
            float adjustedDistance = hitInfo.distance * 0.85f; // Give more clearance to prevent clipping

            // Don't move if we're too close to the obstacle
            if (adjustedDistance < 0.01f)
            {
                targetPosition = transform.position;
                return;
            }

            // Calculate the adjusted position
            Vector3 adjustedPosition = transform.position + (moveDirection * adjustedDistance);

            targetPosition = adjustedPosition;
        }

        protected virtual void Update()
        {
            // --- Outline auto toggle using 'Player' tag with fade ---
            if (enableOutline && outlineComponents != null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                bool shouldShowOutline = false;

                if (playerObj != null && playerObj.activeInHierarchy)
                {
                    // Calculate XZ-plane distance (ignoring Y)
                    Vector3 playerPosXZ = playerObj.transform.position;
                    Vector3 objPosXZ = transform.position;
                    playerPosXZ.y = 0;
                    objPosXZ.y = 0;
                    float distXZ = Vector3.Distance(objPosXZ, playerPosXZ);

                    shouldShowOutline = (distXZ <= interactionDistance);
                }

                if (shouldShowOutline != outlineShouldBeVisible)
                {
                    outlineShouldBeVisible = shouldShowOutline;
                    if (outlineFadeCoroutine != null)
                        StopCoroutine(outlineFadeCoroutine);
                    outlineFadeCoroutine = StartCoroutine(FadeOutline(shouldShowOutline));
                }
            }
        }

        private void SetParticleEffectActive(bool active)
        {
            if (outlineParticlesInstance == null)
                return;
            if (active && !outlineParticlesInstance.isPlaying)
            {
                outlineParticlesInstance.Play();
            }
            else if (!active && outlineParticlesInstance.isPlaying)
            {
                outlineParticlesInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private System.Collections.IEnumerator FadeOutline(bool fadeIn)
        {
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;
            float elapsed = 0f;

            // Enable all outlines
            outlineComponents
                .Where(outline => outline != null)
                .ToList()
                .ForEach(outline => outline.enabled = true);

            SetParticleEffectActive(true);

            while (elapsed < outlineFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / outlineFadeDuration);
                float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

                // Update all outline colors
                outlineComponents
                    .Where(outline => outline != null)
                    .ToList()
                    .ForEach(outline =>
                    {
                        Color c = outline.OutlineColor;
                        c.a = alpha;
                        outline.OutlineColor = c;
                    });

                yield return null;
            }

            // Final update to all outlines
            outlineComponents
                .Where(outline => outline != null)
                .ToList()
                .ForEach(outline =>
                {
                    Color c = outline.OutlineColor;
                    c.a = endAlpha;
                    outline.OutlineColor = c;
                    outline.enabled = fadeIn;
                });

            SetParticleEffectActive(fadeIn);
        }

        /// <summary>
        /// Check if player can pickup this object using renderer bounds or collider bounds for accuracy.
        /// </summary>
        public virtual bool CanPickup(Vector3 playerPosition)
        {
            if (!canBePickedUp)
            {
                return false;
            }

            float centerDistance = Vector3.Distance(transform.position, playerPosition);

            if (objectRenderer != null)
            {
                Vector3 closestPoint = objectRenderer.bounds.ClosestPoint(playerPosition);
                float boundsDistance = Vector3.Distance(playerPosition, closestPoint);

                return Mathf.Min(centerDistance, boundsDistance) <= pickupDistance;
            }
            else if (interactableCollider != null)
            {
                // Account for collider center offset
                Vector3 actualColliderCenter =
                    transform.position + transform.TransformDirection(colliderCenterOffset);

                // Get closest point on collider from player
                Vector3 closestPoint = interactableCollider.ClosestPoint(playerPosition);
                float boundsDistance = Vector3.Distance(playerPosition, closestPoint);

                return Mathf.Min(centerDistance, boundsDistance) <= pickupDistance;
            }

            return centerDistance <= pickupDistance;
        }

        // Stores the offset between the collider center and transform position
        protected Vector3 colliderCenterOffset;

        // Method to calculate and store the offset between collider center and object position
        protected void UpdateColliderCenterOffset()
        {
            if (interactableCollider != null)
            {
                // Calculate the world-space offset between collider center and transform position
                Vector3 worldCenterOffset = interactableCollider.bounds.center - transform.position;
                // Convert to local space so it stays relative to the object's orientation
                colliderCenterOffset = transform.InverseTransformDirection(worldCenterOffset);

                Debug.Log($"Collider center offset for {gameObject.name}: {colliderCenterOffset}");
            }
            else
            {
                colliderCenterOffset = Vector3.zero;
            }
        }

        protected Renderer objectRenderer; // Renderer for bounds-based checks

        // Draw debug visuals in the Scene view
        protected virtual void OnDrawGizmosSelected()
        {
            if (interactableCollider != null && Application.isPlaying)
            {
                // Draw the collider center
                Gizmos.color = Color.yellow;
                Vector3 actualColliderCenter =
                    transform.position + transform.TransformDirection(colliderCenterOffset);
                Gizmos.DrawSphere(actualColliderCenter, 0.1f);

                // Draw a line from transform to collider center
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, actualColliderCenter);

                // Show pickup radius
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawSphere(transform.position, pickupDistance);
            }
        }

        /// <summary>
        /// Checks if a CameraPanController component exists on this game object and activates it if found.
        /// Returns true if the controller was found and activated, false otherwise.
        /// </summary>
        /// <param name="customDuration">Optional custom duration for the camera pan in seconds. If -1, uses the default duration.</param>
        /// <returns>True if the camera pan was activated, false if no controller was found.</returns>
        public bool TryActivateCameraPan(float customDuration = -1)
        {
            // Try to get the CameraPanController component
            var cameraPanController = GetComponent<Interaction.CameraPanController>();

            // Check if we found a controller
            if (cameraPanController != null)
            {
                // If a custom duration was provided, use the CameraPan method
                if (customDuration > 0)
                {
                    cameraPanController.CameraPan(customDuration);
                }
                // Otherwise, just activate with default duration
                else
                {
                    cameraPanController.Activate();
                }

                return true; // Successfully activated
            }

            // No controller found
            return false;
        }
    }
}
