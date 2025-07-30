using System.Collections;
using TruthAndShadows.Interaction;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// A switch interactable that raises a bridge when activated.
    /// Supports both keyboard and controller input through the interaction system.
    /// </summary>
    public class BridgeSwitchInteractable : LeverInteractable
    {
        [Header("Bridge Settings")]
        [SerializeField]
        private Transform bridge; // The bridge to move

        [SerializeField]
        public float raiseAmount = 3f; // How high the bridge moves

        [SerializeField]
        private float moveSpeed = 2f;

        [Header("Audio Settings")]
        [SerializeField]
        private AudioSource switchAudioSource;
        public AudioSource bridgeAudioSource;

        [SerializeField]
        private float audioFadeTime = 3f;

        // Bridge movement variables
        private Vector3 startBridgePos;
        private Vector3 targetBridgePos;
        private bool isRaising = false;
        private bool activated = false;
        private Coroutine moveCoroutine;

        protected override void Start()
        {
            base.Start();

            // This is not a pickup object
            canBePickedUp = false;

            // Initialize bridge position
            if (bridge != null)
            {
                startBridgePos = bridge.position;
                targetBridgePos = startBridgePos + Vector3.right * raiseAmount;
            }
            else
            {
                Debug.LogError(
                    $"BridgeSwitchInteractable on {gameObject.name}: No Bridge transform assigned!"
                );
            }
        }

        public override void StartInteraction()
        {
            // Toggle lever mesh state
            ToggleLever();

            if (!activated && bridge != null)
            {
                activated = true;

                // Play sound if available
                if (source != null && pickUpClip != null)
                {
                    source.clip = pickUpClip;
                    source.Play();
                }

                // Start raising the bridge
                if (moveCoroutine != null)
                {
                    StopCoroutine(moveCoroutine);
                }

                moveCoroutine = StartCoroutine(RaiseBridgeCoroutine());
                TryActivateCameraPan();
            }
        }

        private IEnumerator RaiseBridgeCoroutine()
        {
            isRaising = true;

            // Start switch movement sound
            if (switchAudioSource != null)
            {
                switchAudioSource.Play();
            }

            // Start bridge movement sound
            if (bridgeAudioSource != null)
            {
                switchAudioSource.Play();
            }

            // Move bridge until reaching target position
            while (bridge != null && Vector3.Distance(bridge.position, targetBridgePos) > 0.01f)
            {
                bridge.position = Vector3.MoveTowards(
                    bridge.position,
                    targetBridgePos,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            // Ensure final position is exact
            if (bridge != null)
            {
                bridge.position = targetBridgePos;
            }

            isRaising = false;

            // Fade out audio
            if (bridgeAudioSource != null && bridgeAudioSource.isPlaying)
            {
                // Start volume at current level
                float startVolume = bridgeAudioSource.volume;

                // Gradually reduce volume
                float elapsedTime = 0f;
                while (elapsedTime < audioFadeTime)
                {
                    bridgeAudioSource.volume = Mathf.Lerp(
                        startVolume,
                        0f,
                        elapsedTime / audioFadeTime
                    );
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Stop audio and reset volume for future use
                bridgeAudioSource.Stop();
                bridgeAudioSource.volume = startVolume;
            }

            moveCoroutine = null;
        }

        // Implement required interface methods
        public override void ContinueInteraction() { /* Not needed for this interactable */
        }

        public override void EndInteraction() { /* Not needed for this interactable */
        }
    }
}
