using UnityEngine;
using System.Collections;

namespace TruthAndShadows.Interaction.Feedback
{
    /// <summary>
    /// Provides physical feedback for interactable objects using transform manipulations.
    /// Can be attached to any interactable object to add movement effects during interaction.
    /// </summary>
    [RequireComponent(typeof(InteractableEvents))]
    public class PhysicalInteractionFeedback : MonoBehaviour
    {
        [Header("Hover Effects")]
        [SerializeField]
        private bool enableHoverBob = true;
        
        [SerializeField]
        private float hoverHeight = 0.05f;
        
        [SerializeField]
        private float hoverSpeed = 1f;
        
        [Header("Interaction Effects")]
        [SerializeField]
        private bool enableInteractionShake = true;
        
        [SerializeField]
        private float shakeAmount = 0.05f;
        
        [SerializeField]
        private float shakeDuration = 0.2f;
        
        [Header("Failure Effects")]
        [SerializeField]
        private bool enableFailureBounce = true;
        
        [SerializeField]
        private float bounceIntensity = 0.2f;
        
        [SerializeField]
        private float bounceDuration = 0.3f;
        
        [Header("References")]
        [SerializeField]
        private Transform targetTransform;
        
        private InteractableEvents events;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Coroutine hoverCoroutine;
        private Coroutine shakeCoroutine;
        private Coroutine bounceCoroutine;
        private bool isHovering = false;

        private void Awake()
        {
            events = GetComponent<InteractableEvents>();
            
            // If no target transform is specified, use this object's transform
            if (targetTransform == null)
            {
                targetTransform = transform;
            }
            
            // Store original position and rotation
            originalPosition = targetTransform.localPosition;
            originalRotation = targetTransform.localRotation;
        }

        private void OnEnable()
        {
            // Subscribe to events
            if (events != null)
            {
                events.onFocused.AddListener(OnFocused);
                events.onUnfocused.AddListener(OnUnfocused);
                events.onInteractionStarted.AddListener(OnInteractionStarted);
                events.onInteractionEnded.AddListener(OnInteractionEnded);
                events.onInteractionFailed.AddListener(OnInteractionFailed);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (events != null)
            {
                events.onFocused.RemoveListener(OnFocused);
                events.onUnfocused.RemoveListener(OnUnfocused);
                events.onInteractionStarted.RemoveListener(OnInteractionStarted);
                events.onInteractionEnded.RemoveListener(OnInteractionEnded);
                events.onInteractionFailed.RemoveListener(OnInteractionFailed);
            }
            
            // Stop all coroutines
            StopAllCoroutines();
            
            // Reset position and rotation
            ResetTransform();
        }

        private void OnFocused(InteractionEventData data)
        {
            if (enableHoverBob && !isHovering)
            {
                isHovering = true;
                hoverCoroutine = StartCoroutine(HoverEffect());
            }
        }

        private void OnUnfocused(InteractionEventData data)
        {
            if (isHovering)
            {
                isHovering = false;
                if (hoverCoroutine != null)
                {
                    StopCoroutine(hoverCoroutine);
                    hoverCoroutine = null;
                }
                ResetTransform();
            }
        }

        private void OnInteractionStarted(InteractionEventData data)
        {
            if (enableInteractionShake)
            {
                if (shakeCoroutine != null)
                {
                    StopCoroutine(shakeCoroutine);
                }
                shakeCoroutine = StartCoroutine(ShakeEffect());
            }
        }

        private void OnInteractionEnded(InteractionEventData data)
        {
            // Stop shake if it's still happening
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            
            // Reset position only if not hovering
            if (!isHovering)
            {
                ResetTransform();
            }
        }

        private void OnInteractionFailed(InteractionEventData data)
        {
            if (enableFailureBounce)
            {
                if (bounceCoroutine != null)
                {
                    StopCoroutine(bounceCoroutine);
                }
                bounceCoroutine = StartCoroutine(BounceEffect());
            }
        }

        private void ResetTransform()
        {
            if (targetTransform != null)
            {
                targetTransform.localPosition = originalPosition;
                targetTransform.localRotation = originalRotation;
            }
        }

        private IEnumerator HoverEffect()
        {
            float timeOffset = Random.Range(0f, Mathf.PI); // Random starting position in the cycle
            
            while (isHovering)
            {
                // Only adjust position if we're not currently being shaken or bounced
                if (shakeCoroutine == null && bounceCoroutine == null)
                {
                    float yOffset = Mathf.Sin(Time.time * hoverSpeed + timeOffset) * hoverHeight;
                    Vector3 newPosition = originalPosition + new Vector3(0f, yOffset, 0f);
                    
                    targetTransform.localPosition = newPosition;
                }
                
                yield return null;
            }
        }

        private IEnumerator ShakeEffect()
        {
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                // Generate random offset
                Vector3 offset = Random.insideUnitSphere * shakeAmount;
                
                // Apply offset to position
                targetTransform.localPosition = originalPosition + offset;
                
                // Increment time
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Reset position when done
            if (!isHovering)
            {
                targetTransform.localPosition = originalPosition;
            }
            
            shakeCoroutine = null;
        }

        private IEnumerator BounceEffect()
        {
            float elapsed = 0f;
            
            while (elapsed < bounceDuration)
            {
                // Calculate bounce amount (starts high, ends at 0)
                float t = elapsed / bounceDuration;
                float bounceAmount = Mathf.Lerp(bounceIntensity, 0f, t);
                
                // Apply sine wave bounce
                float yOffset = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 4)) * bounceAmount;
                Vector3 newPosition = originalPosition + new Vector3(0f, yOffset, 0f);
                
                targetTransform.localPosition = newPosition;
                
                // Increment time
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Reset position when done
            if (!isHovering)
            {
                targetTransform.localPosition = originalPosition;
            }
            
            bounceCoroutine = null;
        }
    }
}
