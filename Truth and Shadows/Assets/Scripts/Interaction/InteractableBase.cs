using System;
using Cinemachine;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    public enum PickupMovementStyle
    {
        Held,
        HorizontalPushPull,
    }

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
        protected float interactionDistance = 2f;

        [SerializeField]
        protected bool useColliderBounds = true;

        [Header("Pickup Settings")]
        [SerializeField]
        protected bool canBePickedUp = true;

        [SerializeField]
        protected PickupMovementStyle movementStyle = PickupMovementStyle.Held;

        [SerializeField]
        protected float pickupRaiseAmount = 0.2f;

        [SerializeField]
        protected float pickupSmoothness = 10f;

        [SerializeField]
        protected float pickupMovementSmoothing = 15f;

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
            hasCalculatedRelativePosition = false;
            lastPlayerPosition = playerTransform.position;

            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalParent = transform.parent;

            if (source != null && pickUpClip != null)
            {
                source.PlayOneShot(pickUpClip);
            }

            if (rigidBody != null)
            {
                rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
                rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;

                switch (movementStyle)
                {
                    case PickupMovementStyle.Held:
                        rigidBody.isKinematic = PickupIsKinematic;
                        rigidBody.useGravity = !PickupIsKinematic;
                        break;
                    case PickupMovementStyle.HorizontalPushPull:
                        rigidBody.isKinematic = false;
                        rigidBody.useGravity = true;
                        rigidBody.drag = 5f;
                        rigidBody.angularDrag = 10f;
                        rigidBody.constraints =
                            RigidbodyConstraints.FreezePositionY
                            | RigidbodyConstraints.FreezeRotation;
                        break;
                }
            }

            // Apply pickup raise if specified, only for "Held" style
            if (pickupRaiseAmount > 0 && movementStyle == PickupMovementStyle.Held)
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
            hasCalculatedRelativePosition = false;

            transform.SetParent(originalParent);

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
                rigidBody.constraints = RigidbodyConstraints.None;
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

                if (movementStyle == PickupMovementStyle.HorizontalPushPull)
                {
                    distanceToPlayer = Vector3.Distance(
                        new Vector3(transform.position.x, 0, transform.position.z),
                        new Vector3(playerTransform.position.x, 0, playerTransform.position.z)
                    );
                    dropDistanceMultiplier = 1.5f;
                }
                else
                {
                    distanceToPlayer = Vector3.Distance(
                        transform.position,
                        playerTransform.position
                    );
                    dropDistanceMultiplier = 3f;
                }

                if (distanceToPlayer > interactionDistance * dropDistanceMultiplier)
                {
                    EndPickup();
                    return; // Stop further processing
                }
                UpdatePickupPosition();
            }
        }

        protected virtual void UpdatePickupPosition()
        {
            if (rigidBody == null || playerTransform == null)
                return;

            switch (movementStyle)
            {
                case PickupMovementStyle.Held:
                    if (!hasCalculatedRelativePosition)
                    {
                        relativePosition = transform.position - playerTransform.position;
                        hasCalculatedRelativePosition = true;
                    }

                    Vector3 targetPosition = playerTransform.position + relativePosition;

                    // Unified movement logic
                    if (rigidBody.isKinematic)
                    {
                        // For kinematic bodies, smoothly move to the target position.
                        Vector3 newPosition = Vector3.Lerp(
                            rigidBody.position,
                            targetPosition,
                            Time.fixedDeltaTime * pickupSmoothness
                        );
                        rigidBody.MovePosition(newPosition);
                    }
                    else
                    {
                        // For non-kinematic (physics) bodies, smoothly change velocity.
                        Vector3 targetVelocity =
                            (targetPosition - rigidBody.position) / Time.fixedDeltaTime;

                        Vector3 smoothedVelocity = Vector3.Lerp(
                            rigidBody.velocity,
                            targetVelocity,
                            Time.fixedDeltaTime * pickupMovementSmoothing
                        );
                        rigidBody.velocity = smoothedVelocity;
                    }
                    break;

                case PickupMovementStyle.HorizontalPushPull:
                    // Calculate how much the player has moved since last frame
                    Vector3 currentPlayerPos = playerTransform.position;
                    Vector3 playerDelta = currentPlayerPos - lastPlayerPosition;

                    // We only care about horizontal movement
                    playerDelta.y = 0;

                    // New velocity-based movement to prevent bouncing and physics glitches
                    if (playerDelta.magnitude > 0.001f)
                    {
                        // Calculate the desired velocity to match player movement
                        Vector3 targetVelocity = playerDelta / Time.fixedDeltaTime;

                        // Apply the velocity, but preserve existing vertical velocity (for gravity)
                        rigidBody.velocity = new Vector3(
                            targetVelocity.x,
                            rigidBody.velocity.y,
                            targetVelocity.z
                        );
                    }
                    else
                    {
                        // If the player isn't moving, stop the block's horizontal movement
                        rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, 0);
                    }
                    lastPlayerPosition = currentPlayerPos;
                    break;
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
