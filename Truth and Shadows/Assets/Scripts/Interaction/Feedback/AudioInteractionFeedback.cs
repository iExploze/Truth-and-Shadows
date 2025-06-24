using UnityEngine;

namespace TruthAndShadows.Interaction.Feedback
{
    /// <summary>
    /// Provides audio feedback for interactable objects.
    /// Can be attached to any interactable object to add sound effects during interaction.
    /// </summary>
    [RequireComponent(typeof(InteractableEvents))]
    public class AudioInteractionFeedback : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField]
        private AudioClip hoverSound;
        
        [SerializeField]
        private AudioClip interactStartSound;
        
        [SerializeField]
        private AudioClip interactEndSound;
        
        [SerializeField]
        private AudioClip interactFailSound;
        
        [SerializeField]
        private AudioClip pickupSound;
        
        [SerializeField]
        private AudioClip dropSound;
        
        [Header("Audio Settings")]
        [SerializeField]
        private float volume = 0.7f;
        
        [SerializeField]
        private float pitchVariation = 0.1f;
        
        private InteractableEvents events;
        private AudioSource audioSource;

        private void Awake()
        {
            events = GetComponent<InteractableEvents>();
            
            // Get or create audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configure audio source
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // Full 3D
            audioSource.volume = volume;
            audioSource.dopplerLevel = 0f; // No doppler effect for interaction sounds
        }

        private void OnEnable()
        {
            // Subscribe to events
            if (events != null)
            {
                events.onFocused.AddListener(OnFocused);
                events.onInteractionStarted.AddListener(OnInteractionStarted);
                events.onInteractionEnded.AddListener(OnInteractionEnded);
                events.onInteractionFailed.AddListener(OnInteractionFailed);
                events.onPickupStarted.AddListener(OnPickupStarted);
                events.onPickupEnded.AddListener(OnPickupEnded);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (events != null)
            {
                events.onFocused.RemoveListener(OnFocused);
                events.onInteractionStarted.RemoveListener(OnInteractionStarted);
                events.onInteractionEnded.RemoveListener(OnInteractionEnded);
                events.onInteractionFailed.RemoveListener(OnInteractionFailed);
                events.onPickupStarted.RemoveListener(OnPickupStarted);
                events.onPickupEnded.RemoveListener(OnPickupEnded);
            }
        }

        private void OnFocused(InteractionEventData data)
        {
            PlaySound(hoverSound);
        }

        private void OnInteractionStarted(InteractionEventData data)
        {
            PlaySound(interactStartSound);
        }

        private void OnInteractionEnded(InteractionEventData data)
        {
            PlaySound(interactEndSound);
        }

        private void OnInteractionFailed(InteractionEventData data)
        {
            PlaySound(interactFailSound);
        }

        private void OnPickupStarted(InteractionEventData data)
        {
            PlaySound(pickupSound);
        }

        private void OnPickupEnded(InteractionEventData data)
        {
            PlaySound(dropSound);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip == null || audioSource == null)
                return;
                
            // Add slight pitch variation for more natural sound
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
