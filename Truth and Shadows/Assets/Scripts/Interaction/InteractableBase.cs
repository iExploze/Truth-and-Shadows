using Cinemachine;
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
        protected float interactionDistance = 2f;

        [SerializeField]
        protected bool useColliderBounds = true;

        [Header("Pickup Settings")]
        [SerializeField]
        protected bool canBePickedUp = true;

        [SerializeField]
        protected float pickupRaiseAmount = 0f;

        [SerializeField]
        protected float pickupSmoothness = 10f;

        //For Object interactable Sound
        //Rashai was here
        public AudioSource source;
        public AudioClip pickUpClip;

        [Header("Camera Settings")]
        [SerializeField]
        protected CinemachineVirtualCamera interactionCamera;

        [Header("Outline Settings")]
        [SerializeField]
        protected bool enableOutline = true;

        [SerializeField]
        protected Color outlineColor = new Color(0.2f, 0.6f, 1f, 1f); // Bright blue

        [SerializeField]
        protected float outlineWidth = 10f; // Double the previous size
        private bool isPickedUp = false;
        private Transform playerTransform;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Transform originalParent;
        private Rigidbody rigidBody;
        private Vector3 relativePosition;
        private bool hasCalculatedRelativePosition = false;

        // QuickOutline system variables
        private Outline[] outlineComponents;
        private Coroutine outlineFadeCoroutine;
        private bool outlineShouldBeVisible = false;
        private float outlineFadeDuration = 1f;

        [Header("Outline Particle Effect")]
        [SerializeField]
        protected ParticleSystem outlineParticlePrefab;
        private ParticleSystem outlineParticlesInstance;

        public virtual bool RequiresContinuousInteraction => requireContinuousHold;
        public virtual CinemachineVirtualCamera InteractionCamera => interactionCamera;
        public virtual bool CanBePickedUp => canBePickedUp;
        public virtual bool IsPickedUp => isPickedUp;

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
                outlineParticlesInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public abstract void StartInteraction();

        public virtual void ContinueInteraction() { }

        public virtual void EndInteraction() { }

        public virtual void StartPickup(Transform playerTransform)
        {
            if (!canBePickedUp || isPickedUp)
                return;

            this.playerTransform = playerTransform;
            isPickedUp = true;
            hasCalculatedRelativePosition = false;

            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalParent = transform.parent;
            //Rashai was here
            source.PlayOneShot(pickUpClip);

            if (isPickedUp && rigidBody.velocity.magnitude > 0)
            {
                source.Play();
            }

            if (rigidBody != null)
            {
                rigidBody.isKinematic = true;
                rigidBody.useGravity = false;
            }
            Vector3 raisedPosition = transform.position + Vector3.up * pickupRaiseAmount;
            transform.position = raisedPosition;

            Debug.Log($"Picked up: {gameObject.name}");
        }

        public virtual void EndPickup()
        {
            if (!isPickedUp)
                return;

            isPickedUp = false;
            hasCalculatedRelativePosition = false;

            transform.SetParent(originalParent);

            source.Stop();

            if (rigidBody != null)
            {
                rigidBody.isKinematic = false;
                rigidBody.useGravity = true;
            }

            playerTransform = null;

            Debug.Log($"Dropped: {gameObject.name}");
        }
        

        protected virtual void Update()
        {
            // Update pickup position
            if (isPickedUp && playerTransform != null)
            {
                if (!hasCalculatedRelativePosition)
                {
                    relativePosition = transform.position - playerTransform.position;
                    hasCalculatedRelativePosition = true;
                }

                Vector3 targetPosition = playerTransform.position + relativePosition;

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    Time.deltaTime * pickupSmoothness
                );
            }

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
            if (outlineParticlesInstance == null) return;
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
