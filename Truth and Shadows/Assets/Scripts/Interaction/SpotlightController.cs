using Cinemachine;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// Interactable spotlight that allows players to aim it using mouse input.
    /// Supports vertical and horizontal rotation with configurable limits.
    /// </summary>
    public class SpotlightController : InteractableBase
    {
        [Header("Spotlight Settings")]
        [SerializeField]
        private Light spotLight;

        [SerializeField]
        private Transform verticallyRotatable; // Object that rotates up/down

        [SerializeField]
        private Transform horizontallyRotatable; // Object that rotates left/right

        [Header("Rotation Settings")]
        [SerializeField]
        private float mouseSensitivity = 2f;

        [SerializeField]
        private float verticalMinAngle = -30f;

        [SerializeField]
        private float verticalMaxAngle = 60f;

        [SerializeField]
        private bool invertVerticalInput = false;

        [SerializeField]
        private bool invertHorizontalInput = false;

        [Header("Pivot Points")]
        [SerializeField]
        private Transform verticalPivotPoint;

        [SerializeField]
        private Transform horizontalPivotPoint;

        [Header("Smoothing")]
        [SerializeField]
        private bool useSmoothRotation = true;

        [SerializeField]
        private float rotationSmoothness = 5f;

        // Quaternion-based rotation tracking
        private Quaternion currentVerticalRotation = Quaternion.identity;
        private Quaternion currentHorizontalRotation = Quaternion.identity;
        private Quaternion targetVerticalRotation = Quaternion.identity;
        private Quaternion targetHorizontalRotation = Quaternion.identity;

        // Track cumulative mouse input for angle constraints
        private float accumulatedVerticalInput = 0f;
        private float accumulatedHorizontalInput = 0f;

        private bool wasMouseVisible;
        private CursorLockMode previousCursorLockState;

        public override bool RequiresContinuousInteraction => true;

        protected override void Start()
        {
            base.Start();

            // Validate required components
            if (spotLight == null)
            {
                spotLight = GetComponentInChildren<Light>();
                if (spotLight == null)
                {
                    Debug.LogError(
                        $"SpotlightController on {gameObject.name} requires a Light component!"
                    );
                }
            }

            if (verticallyRotatable == null)
            {
                Debug.LogWarning(
                    $"SpotlightController on {gameObject.name}: No VerticallyRotatable object assigned. Vertical rotation will be disabled."
                );
            }

            if (horizontallyRotatable == null)
            {
                Debug.LogWarning(
                    $"SpotlightController on {gameObject.name}: No HorizontallyRotatable object assigned. Horizontal rotation will be disabled."
                );
            }

            // Initialize rotation values from current transform states
            if (verticallyRotatable != null)
            {
                currentVerticalRotation = targetVerticalRotation =
                    verticallyRotatable.localRotation;
                // Extract the current X rotation for angle constraint tracking
                Vector3 currentEuler = verticallyRotatable.localEulerAngles;
                accumulatedVerticalInput = currentEuler.x;
                // Normalize to -180 to 180 range for constraint checking
                if (accumulatedVerticalInput > 180f)
                    accumulatedVerticalInput -= 360f;
            }

            if (horizontallyRotatable != null)
            {
                currentHorizontalRotation = targetHorizontalRotation =
                    horizontallyRotatable.localRotation;
                accumulatedHorizontalInput = horizontallyRotatable.localEulerAngles.y;
            }
        }

        public override void StartInteraction()
        {
            // Store current cursor state
            wasMouseVisible = Cursor.visible;
            previousCursorLockState = Cursor.lockState;

            // Lock cursor for mouse look
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Debug.Log($"Started controlling spotlight: {gameObject.name}");
        }

        public override void ContinueInteraction()
        {
            HandleMouseInput();
            UpdateRotations();
        }

        public override void EndInteraction()
        {
            // Restore cursor state
            Cursor.visible = wasMouseVisible;
            Cursor.lockState = previousCursorLockState;

            Debug.Log($"Stopped controlling spotlight: {gameObject.name}");
        }

        private void HandleMouseInput()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Apply inversion if enabled
            if (invertHorizontalInput)
                mouseX = -mouseX;
            if (invertVerticalInput)
                mouseY = -mouseY;

            // Update target rotations using Quaternions
            if (horizontallyRotatable != null)
            {
                accumulatedHorizontalInput += mouseX;
                // Create rotation around Y axis
                targetHorizontalRotation = Quaternion.AngleAxis(
                    accumulatedHorizontalInput,
                    Vector3.up
                );
            }

            if (verticallyRotatable != null)
            {
                accumulatedVerticalInput -= mouseY; // Subtract because Y input is typically inverted for looking
                accumulatedVerticalInput = Mathf.Clamp(
                    accumulatedVerticalInput,
                    verticalMinAngle,
                    verticalMaxAngle
                );
                // Create rotation around X axis
                targetVerticalRotation = Quaternion.AngleAxis(
                    accumulatedVerticalInput,
                    Vector3.right
                );
            }
        }

        private void UpdateRotations()
        {
            if (useSmoothRotation)
            {
                // Smooth rotation using Quaternion.Slerp
                if (verticallyRotatable != null)
                {
                    currentVerticalRotation = Quaternion.Slerp(
                        currentVerticalRotation,
                        targetVerticalRotation,
                        rotationSmoothness * Time.deltaTime
                    );
                    ApplyVerticalRotation(currentVerticalRotation);
                }

                if (horizontallyRotatable != null)
                {
                    currentHorizontalRotation = Quaternion.Slerp(
                        currentHorizontalRotation,
                        targetHorizontalRotation,
                        rotationSmoothness * Time.deltaTime
                    );
                    ApplyHorizontalRotation(currentHorizontalRotation);
                }
            }
            else
            {
                // Instant rotation
                if (verticallyRotatable != null)
                {
                    currentVerticalRotation = targetVerticalRotation;
                    ApplyVerticalRotation(currentVerticalRotation);
                }

                if (horizontallyRotatable != null)
                {
                    currentHorizontalRotation = targetHorizontalRotation;
                    ApplyHorizontalRotation(currentHorizontalRotation);
                }
            }
        }

        private void ApplyVerticalRotation(Quaternion rotation)
        {
            if (verticalPivotPoint != null)
            {
                // Store current position and rotation
                Vector3 originalPosition = verticallyRotatable.position;
                Quaternion originalRotation = verticallyRotatable.rotation;

                // Set the target rotation
                verticallyRotatable.localRotation = rotation;

                // Calculate the position offset due to rotation around pivot
                Vector3 pivotToObject = originalPosition - verticalPivotPoint.position;
                Vector3 rotatedOffset =
                    (verticallyRotatable.rotation * Quaternion.Inverse(originalRotation))
                    * pivotToObject;

                // Apply the new position
                verticallyRotatable.position = verticalPivotPoint.position + rotatedOffset;
            }
            else
            {
                // Use local rotation (original behavior)
                verticallyRotatable.localRotation = rotation;
            }
        }

        private void ApplyHorizontalRotation(Quaternion rotation)
        {
            if (horizontalPivotPoint != null)
            {
                // Store current position and rotation
                Vector3 originalPosition = horizontallyRotatable.position;
                Quaternion originalRotation = horizontallyRotatable.rotation;

                // Set the target rotation
                horizontallyRotatable.localRotation = rotation;

                // Calculate the position offset due to rotation around pivot
                Vector3 pivotToObject = originalPosition - horizontalPivotPoint.position;
                Vector3 rotatedOffset =
                    (horizontallyRotatable.rotation * Quaternion.Inverse(originalRotation))
                    * pivotToObject;

                // Apply the new position
                horizontallyRotatable.position = horizontalPivotPoint.position + rotatedOffset;
            }
            else
            {
                // Use local rotation (original behavior)
                horizontallyRotatable.localRotation = rotation;
            }
        }

        #region Debug and Utility Methods

        /// <summary>
        /// Get the current direction the spotlight is pointing
        /// </summary>
        public Vector3 GetSpotlightDirection()
        {
            if (spotLight != null)
                return spotLight.transform.forward;

            // Fallback: calculate from rotations
            Vector3 direction = Vector3.forward;

            if (horizontallyRotatable != null)
                direction = horizontallyRotatable.rotation * direction;

            if (verticallyRotatable != null)
                direction = verticallyRotatable.rotation * direction;

            return direction;
        }

        /// <summary>
        /// Reset spotlight to default rotation
        /// </summary>
        public void ResetToDefault()
        {
            accumulatedVerticalInput = 0f;
            accumulatedHorizontalInput = 0f;
            targetVerticalRotation = Quaternion.identity;
            targetHorizontalRotation = Quaternion.identity;

            if (!useSmoothRotation)
            {
                currentVerticalRotation = Quaternion.identity;
                currentHorizontalRotation = Quaternion.identity;
                UpdateRotations();
            }
        }

        void OnValidate()
        {
            // Ensure angle limits are valid
            if (verticalMinAngle > verticalMaxAngle)
            {
                float temp = verticalMinAngle;
                verticalMinAngle = verticalMaxAngle;
                verticalMaxAngle = temp;
            }

            // Clamp angles to reasonable ranges
            verticalMinAngle = Mathf.Clamp(verticalMinAngle, -90f, 90f);
            verticalMaxAngle = Mathf.Clamp(verticalMaxAngle, -90f, 90f);
        }

        #endregion
    }
}
