using System;
using System.Collections;
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
        protected float interactionDistance = 3f;

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
        protected float pickupMoveSpeed = 2.5f; // New: max move speed for held objects

        [SerializeField]
        protected float minPlayerBlockDistance = 1.0f; // New: minimum allowed distance from player

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
        public virtual bool CanInteract(MonoBehaviour player)
        {
            // Default implementation allows interaction
            return true;
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
        }

        public abstract void StartInteraction();

        public virtual void ContinueInteraction() { }

        public virtual void EndInteraction() { }

        public virtual void StartPickup(Transform playerTransform)
        {
            if (!canBePickedUp || IsPickedUp)
                return;

            this.playerTransform = playerTransform;
            IsPickedUp = true;
            CurrentlyHeldInteractable = this; // Set static reference
            hasCalculatedRelativePosition = false;
            lastPlayerPosition = playerTransform.position;

            // --- Disable player movement when picked up ---
            var movement = playerTransform.GetComponent<TruthAndShadows.Player.PlayerMovement>();
            if (movement != null)
            {
                movement.canMove = false;
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
                // Drop object if too far
                float distanceToPlayer;
                float dropDistanceMultiplier;

                distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                dropDistanceMultiplier = 1f;

                if (distanceToPlayer > interactionDistance * dropDistanceMultiplier)
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

            // Optional: vertical movement (e.g., with jump/crouch or look input)
            float vertical = 0f;
            if (inputManager != null && inputManager.RotateHeld)
            {
                vertical = inputManager.LookInput.y * pickupMoveSpeed * Time.deltaTime;
            }

            // Target position
            Vector3 targetPos =
                transform.position + moveDir * pickupMoveSpeed * pickupMoveSpeed * Time.deltaTime;
            targetPos.y += vertical;

            // Smooth movement
            Vector3 lerped = Vector3.Lerp(
                transform.position,
                targetPos,
                Time.deltaTime * pickupSmoothness
            );
            transform.position = lerped;

            // Optionally, update rigidbody position if kinematic
            if (rigidBody.isKinematic)
            {
                rigidBody.position = transform.position;
            }
        }

        protected virtual void Update()
        {
            // --- Outline auto toggle using 'Player' tag with fade ---
            if (enableOutline && outlineComponents != null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                bool shouldShow = false;
                if (playerObj != null && playerObj.activeInHierarchy)
                {
                    float dist = Vector3.Distance(transform.position, playerObj.transform.position);
                    shouldShow = (dist <= interactionDistance);
                }
                if (shouldShow != outlineShouldBeVisible)
                {
                    outlineShouldBeVisible = shouldShow;
                    if (outlineFadeCoroutine != null)
                        StopCoroutine(outlineFadeCoroutine);
                    outlineFadeCoroutine = StartCoroutine(FadeOutline(shouldShow));
                }
            }
            // --- Laser update logic is now handled by PersistentLaserManager ---
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
            foreach (var outline in outlineComponents)
            {
                if (outline != null)
                    outline.enabled = true;
            }
            SetParticleEffectActive(true);
            while (elapsed < outlineFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / outlineFadeDuration);
                float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                foreach (var outline in outlineComponents)
                {
                    if (outline != null)
                    {
                        Color c = outline.OutlineColor;
                        c.a = alpha;
                        outline.OutlineColor = c;
                    }
                }
                yield return null;
            }
            foreach (var outline in outlineComponents)
            {
                if (outline != null)
                {
                    Color c = outline.OutlineColor;
                    c.a = endAlpha;
                    outline.OutlineColor = c;
                    outline.enabled = fadeIn;
                }
            }
            SetParticleEffectActive(fadeIn);
        }

        /// <summary>
        /// Check if player can interact with this object - uses collider bounds for better detection
        /// </summary>
        public virtual bool CanInteract(Vector3 playerPosition)
        {
            float centerDistance = Vector3.Distance(transform.position, playerPosition);

            if (useColliderBounds)
            {
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    Vector3 closestPoint = col.ClosestPoint(playerPosition);
                    float boundsDistance = Vector3.Distance(playerPosition, closestPoint);

                    float finalDistance = Mathf.Min(centerDistance, boundsDistance);
                    return finalDistance <= interactionDistance;
                }
            }
            return centerDistance <= interactionDistance;
        }

        /// <summary>
        /// Check if player can pickup this object - same as interaction by default
        /// </summary>
        public virtual bool CanPickup(Vector3 playerPosition)
        {
            return CanInteract(playerPosition);
        }
    }
}
