using System.Collections;
using TruthAndShadows.Interaction;
using UnityEngine;

namespace TruthAndShadows.Bridge
{
    /// <summary>
    /// A switch interactable that slides a wall when activated.
    /// Supports both keyboard and controller input through the interaction system.
    /// </summary>
    public class WallSwitchInteractable : InteractableBase
    {
        [Header("Wall Settings")]
        [SerializeField]
        private Transform wallToSlide;

        [SerializeField]
        private Vector3 localMoveDirection = Vector3.right;

        [SerializeField]
        private float moveDistance = 3f;

        [SerializeField]
        private float moveSpeed = 2f;

        [Header("Audio Settings")]
        [SerializeField]
        public AudioSource switchSoundSource;
        public AudioSource wallSoundSource;

        [SerializeField]
        private float audioFadeTime = 3f;

        // Wall movement variables
        private Vector3 closedPosition;
        private Vector3 openPosition;
        private bool isOpening = false;
        private bool activated = false;
        private Coroutine moveCoroutine;

        protected override void Start()
        {
            base.Start();

            // This is not a pickup object
            canBePickedUp = false;

            // Initialize wall positions
            if (wallToSlide != null)
            {
                closedPosition = wallToSlide.position;
                openPosition =
                    closedPosition
                    + wallToSlide.TransformDirection(localMoveDirection.normalized) * moveDistance;
            }
            else
            {
                Debug.LogError(
                    $"WallSwitchInteractable on {gameObject.name}: No wall transform assigned!"
                );
            }
        }

        public override void StartInteraction()
        {
            if (!activated && wallToSlide != null)
            {
                activated = true;

                // Play switch sound if available
                if (source != null && pickUpClip != null)
                {
                    source.clip = pickUpClip;
                    source.Play();
                }

                // Start moving the wall
                if (moveCoroutine != null)
                {
                    StopCoroutine(moveCoroutine);
                }

                moveCoroutine = StartCoroutine(SlideWallCoroutine());
                TryActivateCameraPan();
            }
        }

        private IEnumerator SlideWallCoroutine()
        {
            isOpening = true;

            // Start switch movement sound
            if (switchSoundSource != null)
            {
                switchSoundSource.Play();
            }

            // Start switch movement sound
            if (wallSoundSource != null)
            {
                wallSoundSource.Play();
            }
            // Move wall until reaching target position
            while (
                wallToSlide != null && Vector3.Distance(wallToSlide.position, openPosition) > 0.01f
            )
            {
                wallToSlide.position = Vector3.MoveTowards(
                    wallToSlide.position,
                    openPosition,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            // Ensure final position is exact
            if (wallToSlide != null)
            {
                wallToSlide.position = openPosition;
            }

            isOpening = false;

            // Fade out audio
            if (wallSoundSource != null && wallSoundSource.isPlaying)
            {
                // Start volume at current level
                float startVolume = wallSoundSource.volume;

                // Gradually reduce volume
                float elapsedTime = 0f;
                while (elapsedTime < audioFadeTime)
                {
                    wallSoundSource.volume = Mathf.Lerp(
                        startVolume,
                        0f,
                        elapsedTime / audioFadeTime
                    );
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Stop audio and reset volume for future use
                wallSoundSource.Stop();
                wallSoundSource.volume = startVolume;
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
