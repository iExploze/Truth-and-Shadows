using UnityEngine;
using System.Collections;

namespace TruthAndShadows.Interaction.Feedback
{
    /// <summary>
    /// Provides visual feedback for interactable objects using material properties.
    /// Can be attached to any interactable object to add visual effects during interaction.
    /// </summary>
    [RequireComponent(typeof(InteractableEvents))]
    public class VisualInteractionFeedback : MonoBehaviour
    {
        [Header("Color Settings")]
        [SerializeField]
        private Color normalColor = Color.white;
        
        [SerializeField]
        private Color focusedColor = new Color(1f, 1f, 0.5f, 1f); // Yellow tint
        
        [SerializeField]
        private Color interactingColor = new Color(0.5f, 1f, 0.5f, 1f); // Green tint
        
        [SerializeField]
        private Color failedColor = new Color(1f, 0.5f, 0.5f, 1f); // Red tint
        
        [Header("Animation Settings")]
        [SerializeField]
        private bool animatePulse = true;
        
        [SerializeField]
        private float pulseSpeed = 2f;
        
        [SerializeField]
        private float pulseIntensity = 0.2f;
        
        [Header("References")]
        [SerializeField]
        private Renderer[] targetRenderers;
        
        private InteractableEvents events;
        private string emissionColorName = "_EmissionColor";
        private string emissionKeywordName = "_EMISSION";
        private Coroutine pulseCoroutine;
        private bool isInteracting = false;

        private void Awake()
        {
            events = GetComponent<InteractableEvents>();
            
            // If no renderers are assigned, try to get renderers from this object
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderers = GetComponentsInChildren<Renderer>();
            }
            
            // Setup material for emission if needed
            foreach (var renderer in targetRenderers)
            {
                foreach (var material in renderer.materials)
                {
                    material.EnableKeyword(emissionKeywordName);
                }
            }
            
            // Set initial color
            SetEmissionColor(normalColor);
        }

        private void OnEnable()
        {
            // Subscribe to interaction events
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
            
            // Stop any active coroutines
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            
            // Reset to normal color
            SetEmissionColor(normalColor);
        }

        private void OnFocused(InteractionEventData data)
        {
            if (!isInteracting)
            {
                SetEmissionColor(focusedColor);
                
                // Start pulsing if enabled
                if (animatePulse && pulseCoroutine == null)
                {
                    pulseCoroutine = StartCoroutine(PulseEffect(focusedColor));
                }
            }
        }

        private void OnUnfocused(InteractionEventData data)
        {
            if (!isInteracting)
            {
                // Stop pulsing
                if (pulseCoroutine != null)
                {
                    StopCoroutine(pulseCoroutine);
                    pulseCoroutine = null;
                }
                
                SetEmissionColor(normalColor);
            }
        }

        private void OnInteractionStarted(InteractionEventData data)
        {
            isInteracting = true;
            
            // Stop any previous pulse
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }
            
            // Start interaction pulse effect
            pulseCoroutine = StartCoroutine(PulseEffect(interactingColor));
            
            // Base color is the interacting color
            SetEmissionColor(interactingColor);
        }

        private void OnInteractionEnded(InteractionEventData data)
        {
            isInteracting = false;
            
            // Stop pulsing
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            
            // Return to normal state
            SetEmissionColor(normalColor);
        }

        private void OnInteractionFailed(InteractionEventData data)
        {
            // Flash failed color briefly
            StartCoroutine(FlashColor(failedColor, 0.5f));
        }

        private void SetEmissionColor(Color color)
        {
            // Apply emission color to all target renderers
            foreach (var renderer in targetRenderers)
            {
                if (renderer == null) continue;
                
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty(emissionColorName))
                    {
                        material.SetColor(emissionColorName, color);
                    }
                }
            }
        }

        private IEnumerator PulseEffect(Color baseColor)
        {
            float t = 0;
            while (true)
            {
                // Calculate pulse intensity based on sin wave
                float pulseValue = Mathf.Sin(t * pulseSpeed) * pulseIntensity + 1.0f;
                Color pulseColor = baseColor * pulseValue;
                
                // Apply color to materials
                SetEmissionColor(pulseColor);
                
                // Increment time
                t += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FlashColor(Color flashColor, float duration)
        {
            // Store original color
            Color originalColor = normalColor;
            if (isInteracting)
            {
                originalColor = interactingColor;
            }
            
            // Set flash color
            SetEmissionColor(flashColor);
            
            // Wait for duration
            yield return new WaitForSeconds(duration);
            
            // Return to original color if not interacting
            if (!isInteracting)
            {
                SetEmissionColor(originalColor);
            }
        }
    }
}
