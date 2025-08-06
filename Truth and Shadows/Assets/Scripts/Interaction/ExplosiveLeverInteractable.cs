using System.Collections;
using UnityEngine;
using Cinemachine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// A lever interactable that triggers explosion effects when activated.
    /// Combines lever functionality with object enabling/disabling, sound effects, and screen shake.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class ExplosiveLeverInteractable : LeverInteractable
    {
        [Header("Explosion Effects")]
        [SerializeField]
        public GameObject objectToEnable;         // Assign the parent GameObject with children
        
        [SerializeField]
        public GameObject objectToDisable;        // Assign the parent GameObject with children
        
        [SerializeField]
        private AudioClip explosionSound;          // Assign explosion or rumble clip
        
        [SerializeField]
        private float shakeIntensity = 2f;
        
        [SerializeField]
        private float shakeDuration = 0.5f;

        [Header("Explosion Settings")]
        [SerializeField]
        [Tooltip("If true, explosion effects only trigger once. If false, they trigger every time the lever is activated.")]
        private bool oneTimeExplosion = true;
        
        [SerializeField]
        [Tooltip("If true, explosion effects trigger when lever turns ON. If false, they trigger when lever turns OFF.")]
        private bool explodeOnLeverOn = true;

        // Private components
        private AudioSource explosionAudioSource;
        private CinemachineImpulseSource impulseSource;
        
        // State tracking
        private bool hasExploded = false;

        protected override void Start()
        {
            base.Start();
            
            // Get or add explosion audio source (separate from base interaction audio)
            explosionAudioSource = GetComponent<AudioSource>();
            if (explosionAudioSource == null)
            {
                explosionAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Get or add impulse source for screen shake
            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
            {
                impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            }
        }

        public override void StartInteraction()
        {
            // Get the current state before toggling
            bool wasOn = IsLeverOn();
            
            // Call base interaction (toggles lever)
            base.StartInteraction();
            
            // Check if we should trigger explosion effects
            bool shouldExplode = false;
            
            if (explodeOnLeverOn && !wasOn && IsLeverOn())
            {
                // Lever was turned ON
                shouldExplode = true;
            }
            else if (!explodeOnLeverOn && wasOn && !IsLeverOn())
            {
                // Lever was turned OFF
                shouldExplode = true;
            }
            
            // Trigger explosion if conditions are met
            if (shouldExplode && (!oneTimeExplosion || !hasExploded))
            {
                TriggerExplosion();
                if (oneTimeExplosion)
                {
                    hasExploded = true;
                }
            }
        }

        /// <summary>
        /// Triggers all explosion effects: object enable/disable, sound, and screen shake
        /// </summary>
        private void TriggerExplosion()
        {
            // Enable the specified object with children
            if (objectToEnable != null)
            {
                objectToEnable.SetActive(true);
            }

            // Disable the specified object with children
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }

            // Play explosion sound
            if (explosionAudioSource != null && explosionSound != null)
            {
                explosionAudioSource.PlayOneShot(explosionSound);
            }

            // Trigger screen shake
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse(shakeIntensity);
            }
        }

        /// <summary>
        /// Public method to manually trigger explosion effects (for external scripts)
        /// </summary>
        public void ManuallyTriggerExplosion()
        {
            if (!oneTimeExplosion || !hasExploded)
            {
                TriggerExplosion();
                if (oneTimeExplosion)
                {
                    hasExploded = true;
                }
            }
        }

        /// <summary>
        /// Resets the explosion state, allowing it to be triggered again
        /// </summary>
        public void ResetExplosionState()
        {
            hasExploded = false;
        }

        /// <summary>
        /// Returns whether the explosion has already been triggered (only relevant if oneTimeExplosion is true)
        /// </summary>
        public bool HasExploded => hasExploded;
    }
}
