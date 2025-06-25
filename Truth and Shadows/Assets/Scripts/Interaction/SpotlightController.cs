using System.Collections;
using Cinemachine;
using TruthAndShadows.InputSystem;
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

        [Header("Camera Settings")]
        [SerializeField]
        private CinemachineFreeLook spotlightCamera;
        public override Component InteractionCamera => spotlightCamera;

        [Header("Rotation Settings")]
        [SerializeField]
        [Range(0.1f, 10f)]
        [Tooltip("Base sensitivity multiplier for spotlight rotation")]
        private float sensitivity = 3f;

        [Header("Camera Sensitivity")]
        [SerializeField]
        [Range(0.1f, 100f)]
        [Tooltip("Camera horizontal rotation sensitivity")]
        private float cameraHorizontalSensitivity = 3f;

        [SerializeField]
        [Range(0.1f, 100f)]
        [Tooltip("Camera vertical rotation sensitivity")]
        private float cameraVerticalSensitivity = 3f;

        [Header("Spotlight Sensitivity")]
        [SerializeField]
        [Range(0.1f, 100f)]
        [Tooltip("Spotlight horizontal rotation sensitivity")]
        private float spotlightHorizontalSensitivity = 3f;

        [SerializeField]
        [Range(0.1f, 100f)]
        [Tooltip("Spotlight vertical rotation sensitivity")]
        private float spotlightVerticalSensitivity = 3f;

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
        [Tooltip("Point around which vertical rotation occurs (optional)")]
        private Transform verticalPivotPoint;

        [SerializeField]
        [Tooltip("Point around which horizontal rotation occurs (optional)")]
        private Transform horizontalPivotPoint;

        [Header("Smoothing")]
        [SerializeField]
        private bool useSmoothRotation = true;

        [SerializeField]
        [Range(1f, 20f)]
        private float rotationSmoothness = 5f;

        [Header("Controller Settings")]
        // Controller-specific settings (if needed)

        // Track accumulated input for angle constraints
        private float accumulatedVerticalInput = 0f;
        private float accumulatedHorizontalInput = 0f;

        // Mouse cursor state tracking
        private bool wasMouseVisible;
        private CursorLockMode previousCursorLockState;

        // Camera targets
        private Transform cameraLookAtTarget;
        private Transform cameraFollowTarget;

        // Camera priority cache for proper restoration
        private int originalCameraPriority = 10;

        public override bool RequiresContinuousInteraction => true;

        protected override void Start()
        {
            // Call the base Start method first to initialize outline and rigidbody
            base.Start();

            // Validate and setup components
            ValidateComponents();
            InitializeRotationValues();
            InitializePickupBehavior();

            // Verify outline setup
            VerifyOutlineComponents();
            Debug.Log(
                $"SpotlightController initialized - Pickup enabled: {canBePickedUp}, Outline enabled: {enableOutline}"
            );
        }

        private void ValidateComponents()
        {
            // Find spotlight component if not assigned
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

            // CRITICAL: Make sure pickup is enabled
            canBePickedUp = true;

            // Ensure the spotlight has a rigidbody for proper pickup handling
            rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                // Add and configure a new rigidbody
                rigidBody = gameObject.AddComponent<Rigidbody>();
                ConfigureRigidbody();
                Debug.Log(
                    $"Added Rigidbody component to {gameObject.name} for pickup functionality"
                );
            }
            else
            {
                // Configure the existing rigidbody
                ConfigureRigidbody();
            }

            // Validate rotation components
            if (verticallyRotatable == null)
            {
                Debug.LogWarning(
                    $"SpotlightController on {gameObject.name}: No vertically rotatable object assigned. Vertical rotation will be disabled."
                );
            }

            if (horizontallyRotatable == null)
            {
                Debug.LogWarning(
                    $"SpotlightController on {gameObject.name}: No horizontally rotatable object assigned. Horizontal rotation will be disabled."
                );
            }

            // Try to find the camera if not assigned
            if (spotlightCamera == null)
            {
                var cameraObject = GameObject.Find("SpotlightCamera");
                if (cameraObject != null)
                {
                    spotlightCamera = cameraObject.GetComponent<CinemachineFreeLook>();
                    if (spotlightCamera == null)
                    {
                        Debug.LogWarning(
                            "Found 'SpotlightCamera' GameObject, but it lacks a CinemachineFreeLook component."
                        );
                    }
                }
                else
                {
                    Debug.LogWarning(
                        "SpotlightController: No camera assigned and 'SpotlightCamera' not found."
                    );
                }
            }

            Debug.Log(
                $"Validated components - CanBePickedUp: {canBePickedUp}, Rigidbody: {(rigidBody != null)}"
            );
        }

        private void ConfigureRigidbody()
        {
            if (rigidBody == null)
                return;

            // Configure for pickup
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;
            rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // No rotation constraints - we want manual control
            rigidBody.constraints = RigidbodyConstraints.None;
            // Minimal drag
            rigidBody.drag = 0;
            rigidBody.angularDrag = 0.05f;
        }

        private void InitializeRotationValues()
        {
            if (verticallyRotatable != null)
            {
                // Extract current rotation for tracking
                Vector3 currentEuler = verticallyRotatable.localEulerAngles;
                accumulatedVerticalInput = currentEuler.x;

                // Normalize to -180 to 180 range
                if (accumulatedVerticalInput > 180f)
                {
                    accumulatedVerticalInput -= 360f;
                }
            }

            if (horizontallyRotatable != null)
            {
                accumulatedHorizontalInput = horizontallyRotatable.localEulerAngles.y;
            }
        }

        private void InitializePickupBehavior()
        {
            // Make sure the spotlight uses the Held movement style
            movementStyle = PickupMovementStyle.Held;

            // Set a higher pickup smoothness for more responsive movement
            pickupSmoothness = 15f;

            // Ensure outline is enabled
            enableOutline = true;

            // Set bright blue outline color
            outlineColor = new Color(0.2f, 0.6f, 1f, 1f);

            // Make sure it can be picked up
            canBePickedUp = true;
        }

        public override void StartInteraction()
        {
            // Check permissions from the centralized provider
            bool canInteractWithSpotlight = true;

            // Check for rotate permission from InputContextProvider if available
            if (InputContextProvider.Instance != null)
            {
                canInteractWithSpotlight =
                    InputContextProvider.Instance.CanRotate
                    && InputContextProvider.Instance.CanInteract;

                if (!canInteractWithSpotlight)
                {
                    Debug.LogWarning(
                        "Spotlight interaction attempted but permission denied by InputContextProvider"
                    );
                    return; // Don't proceed with interaction if not allowed
                }
            }

            if (spotlightCamera != null)
            {
                // Setup the camera and ensure proper alignment
                SetupCamera();
                EnsureCameraAlignment();

                // Store original camera priority for restoration
                if (
                    spotlightCamera.TryGetComponent<CinemachineVirtualCameraBase>(
                        out var virtualCamera
                    )
                )
                {
                    originalCameraPriority = virtualCamera.Priority;
                    virtualCamera.Priority = 100; // High priority to take control
                }
            }

            // Save and update cursor state
            wasMouseVisible = Cursor.visible;
            previousCursorLockState = Cursor.lockState;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Ensures that the camera is perfectly aligned with the spotlight direction at the start
        /// </summary>
        private void EnsureCameraAlignment()
        {
            if (
                spotlightCamera != null
                && spotLight != null
                && cameraFollowTarget != null
                && cameraLookAtTarget != null
            )
            {
                // Get spotlight direction
                Vector3 spotlightForward = spotLight.transform.forward;

                // Force the camera follow target to be positioned directly behind the spotlight
                float followDistance = 1.5f;
                Vector3 idealFollowPos =
                    spotLight.transform.position
                    - spotlightForward * followDistance
                    + Vector3.up * 0.2f;
                cameraFollowTarget.position = idealFollowPos;

                // Position the look target directly in front of the spotlight
                float lookDistance = 2f;
                cameraLookAtTarget.position =
                    spotLight.transform.position + spotlightForward * lookDistance;

                // Set the camera's initial rotation to match the spotlight's forward direction
                if (spotlightCamera.VirtualCameraGameObject != null)
                {
                    // Force an update of the camera to make it immediately align
                    spotlightCamera.OnTargetObjectWarped(
                        cameraFollowTarget,
                        cameraFollowTarget.position - cameraFollowTarget.forward * 0.01f
                    );
                }

                // Reset the camera's Y axis to the middle position
                spotlightCamera.m_YAxis.Value = 0.5f;

                Debug.Log("Camera alignment enforced on spotlight interaction start");
            }
        }

        private void SetupCamera()
        {
            // Create camera targets
            SetupCameraTargets();

            // Configure camera settings
            ConfigureCameraSettings();
        }

        private void SetupCameraTargets()
        {
            // Store original positions before camera setup
            Vector3 originalSpotlightPos = spotLight.transform.position;
            Vector3 originalVerticalPos =
                verticallyRotatable != null ? verticallyRotatable.position : Vector3.zero;
            Vector3 originalHorizontalPos =
                horizontallyRotatable != null ? horizontallyRotatable.position : Vector3.zero;

            // Create look-at target if needed
            if (cameraLookAtTarget == null)
            {
                cameraLookAtTarget = new GameObject($"{gameObject.name}_LookAtTarget").transform;
            }

            // Create follow target if needed
            if (cameraFollowTarget == null)
            {
                cameraFollowTarget = new GameObject($"{gameObject.name}_FollowTarget").transform;
            }

            // Don't parent to the spotlight at all to avoid affecting its position
            // Instead, place the camera targets at fixed positions in world space

            // Get the spotlight's forward direction
            Vector3 spotlightForward = spotLight.transform.forward;

            // Calculate positions along the same line
            float targetDistance = Mathf.Min(spotLight.range / 3f, 2f);

            // Position the look-at target in front of the spotlight along its forward direction
            Vector3 targetPosition =
                spotLight.transform.position + spotlightForward * targetDistance;
            cameraLookAtTarget.position = targetPosition;
            cameraLookAtTarget.SetParent(null); // Keep in world space

            // Position the follow target behind the spotlight along the same line
            // This ensures the camera, follow target, and look-at target are all along the same line
            float followDistance = 1.5f; // Distance behind spotlight
            Vector3 followPosition =
                spotLight.transform.position
                - spotlightForward * followDistance
                + Vector3.up * 0.5f;
            cameraFollowTarget.position = followPosition;
            cameraFollowTarget.SetParent(null); // Keep in world space

            // Set camera targets
            spotlightCamera.Follow = cameraFollowTarget;
            spotlightCamera.LookAt = cameraLookAtTarget;

            // Ensure spotlight positions haven't been affected
            if (spotLight.transform.position != originalSpotlightPos)
            {
                spotLight.transform.position = originalSpotlightPos;
            }

            if (verticallyRotatable != null && verticallyRotatable.position != originalVerticalPos)
            {
                verticallyRotatable.position = originalVerticalPos;
            }

            if (
                horizontallyRotatable != null
                && horizontallyRotatable.position != originalHorizontalPos
            )
            {
                horizontallyRotatable.position = originalHorizontalPos;
            }
        }

        private void ConfigureCameraSettings()
        {
            if (spotlightCamera.m_Orbits.Length >= 3)
            {
                // Use consistent radius for all orbits for predictable rotation
                float orbitRadius = 0.3f; // Reduced for tighter control

                // Configure the three orbits (top, middle, bottom)
                // Set all three orbits to be at the same height and radius for alignment
                for (int i = 0; i < spotlightCamera.m_Orbits.Length; i++)
                {
                    spotlightCamera.m_Orbits[i].m_Radius = orbitRadius;
                    spotlightCamera.m_Orbits[i].m_Height = 0f; // Set to 0 for perfect alignment
                }

                // Set camera speed based on sensitivity
                const float CAMERA_X_BASE_SPEED = 9f;
                const float CAMERA_Y_BASE_SPEED = 4f;

                spotlightCamera.m_XAxis.m_MaxSpeed =
                    CAMERA_X_BASE_SPEED * cameraHorizontalSensitivity * sensitivity;

                spotlightCamera.m_YAxis.m_MaxSpeed =
                    CAMERA_Y_BASE_SPEED * cameraVerticalSensitivity * sensitivity;

                // Match inversion settings
                spotlightCamera.m_XAxis.m_InvertInput = invertHorizontalInput;
                spotlightCamera.m_YAxis.m_InvertInput = invertVerticalInput;

                // Set initial position to middle value (0.5) to start at the center orbit
                spotlightCamera.m_YAxis.Value = 0.5f;

                // Ensure binding mode is correct
                spotlightCamera.m_BindingMode = CinemachineTransposer
                    .BindingMode
                    .SimpleFollowWithWorldUp;
            }
        }

        public override void ContinueInteraction()
        {
            // Check if we can continue the interaction based on permissions
            bool canContinueInteraction = true;

            // Check the centralized permissions provider
            if (InputContextProvider.Instance != null)
            {
                canContinueInteraction =
                    InputContextProvider.Instance.CanRotate
                    && InputContextProvider.Instance.CanInteract;

                // If permissions revoked, end the interaction early
                if (!canContinueInteraction)
                {
                    Debug.Log("Spotlight interaction permissions revoked - ending interaction");
                    EndInteraction(); // End interaction if permissions have been revoked
                    return;
                }
            }

            HandleMouseInput();
            UpdateRotations();
            UpdateLookAtTarget();
        }

        private void UpdateLookAtTarget()
        {
            if (spotLight != null && cameraLookAtTarget != null && cameraFollowTarget != null)
            {
                // Get the spotlight's forward direction
                Vector3 spotlightForward = spotLight.transform.forward;

                // Position the look-at target along the spotlight beam
                float targetDistance = Mathf.Min(spotLight.range / 3f, 2f);
                Vector3 targetPosition =
                    spotLight.transform.position + spotlightForward * targetDistance;

                // Update the look-at target position directly in world space
                cameraLookAtTarget.position = targetPosition;

                // Also update the follow target to maintain alignment
                float followDistance = 1.5f; // Distance behind spotlight
                Vector3 followPosition =
                    spotLight.transform.position
                    - spotlightForward * followDistance
                    + Vector3.up * 0.5f;
                cameraFollowTarget.position = followPosition;

                // Debug visualization to verify alignment (uncomment if needed for testing)
                // Debug.DrawLine(cameraFollowTarget.position, spotLight.transform.position, Color.blue);
                // Debug.DrawLine(spotLight.transform.position, cameraLookAtTarget.position, Color.red);
            }
        }

        private void HandleMouseInput()
        {
            // Camera input is handled by Cinemachine - we only need to update its sensitivity settings
            // Null check for InputManager.Instance
            if (InputManager.Instance == null)
            {
                Debug.LogError(
                    "InputManager.Instance is null! Cannot get input for spotlight control."
                );
                return;
            }

            // Get consistent input across devices (mouse or right stick)
            Vector2 lookInput = InputManager.Instance.LookInput;

            // Skip spotlight aiming unless the rotate button is pressed AND rotation is permitted by the permissions system
            // Check both the local input and the centralized permission system
            bool canRotate = true;

            // Check the centralized permission provider if available
            if (InputContextProvider.Instance != null)
            {
                canRotate = InputContextProvider.Instance.CanRotate;
            }

            // Only allow rotation if both the button is pressed and permissions allow it
            if (!InputManager.Instance.RotateHeld || !canRotate)
            {
                return;
            }

            // Calculate input for spotlight movement
            float spotlightMouseX =
                lookInput.x * spotlightHorizontalSensitivity * sensitivity * Time.deltaTime;
            float spotlightMouseY =
                lookInput.y * spotlightVerticalSensitivity * sensitivity * Time.deltaTime; // Apply inversion if needed
            if (invertHorizontalInput)
            {
                // Only apply to spotlight, camera rotation is handled by Cinemachine
                spotlightMouseX = -spotlightMouseX;
            }

            if (invertVerticalInput)
            {
                // Only apply to spotlight, camera rotation is handled by Cinemachine
                spotlightMouseY = -spotlightMouseY;
            }

            // Update horizontal rotation (left/right) using spotlight sensitivity
            if (horizontallyRotatable != null)
            {
                accumulatedHorizontalInput += spotlightMouseX;
            }

            // Update vertical rotation (up/down) using spotlight sensitivity
            if (verticallyRotatable != null)
            {
                accumulatedVerticalInput -= spotlightMouseY; // Subtract because Y is inverted for looking up/down
                accumulatedVerticalInput = Mathf.Clamp(
                    accumulatedVerticalInput,
                    verticalMinAngle,
                    verticalMaxAngle
                );
            }

            // Update camera sensitivity if active
            if (spotlightCamera != null)
            {
                // The multiplier is to make the base values work well
                const float CAMERA_X_BASE_SPEED = 9f;
                const float CAMERA_Y_BASE_SPEED = 4f;

                spotlightCamera.m_XAxis.m_MaxSpeed =
                    CAMERA_X_BASE_SPEED * cameraHorizontalSensitivity;
                spotlightCamera.m_YAxis.m_MaxSpeed =
                    CAMERA_Y_BASE_SPEED * cameraVerticalSensitivity;
            }
        }

        private void UpdateRotations()
        {
            // Store original positions before applying rotations
            Vector3 originalVerticalPos =
                verticallyRotatable != null ? verticallyRotatable.position : Vector3.zero;

            Vector3 originalHorizontalPos =
                horizontallyRotatable != null ? horizontallyRotatable.position : Vector3.zero;

            if (horizontallyRotatable != null)
            {
                // Apply horizontal rotation
                Quaternion targetRotation = Quaternion.Euler(0, accumulatedHorizontalInput, 0);

                if (horizontalPivotPoint != null)
                {
                    // Use pivot point for rotation around a specific axis
                    ApplyRotationAroundPivot(
                        horizontallyRotatable,
                        targetRotation,
                        horizontalPivotPoint.position,
                        useSmoothRotation,
                        rotationSmoothness
                    );
                }
                else
                {
                    // Apply simple local rotation
                    if (useSmoothRotation)
                    {
                        horizontallyRotatable.localRotation = Quaternion.Slerp(
                            horizontallyRotatable.localRotation,
                            targetRotation,
                            rotationSmoothness * Time.deltaTime
                        );
                    }
                    else
                    {
                        horizontallyRotatable.localRotation = targetRotation;
                    }

                    // Restore original position when not using pivot point
                    if (horizontallyRotatable.position != originalHorizontalPos)
                    {
                        horizontallyRotatable.position = originalHorizontalPos;
                    }
                }
            }

            if (verticallyRotatable != null)
            {
                // Apply vertical rotation
                Quaternion targetRotation = Quaternion.Euler(accumulatedVerticalInput, 0, 0);

                if (verticalPivotPoint != null)
                {
                    // Use pivot point for rotation around a specific axis
                    ApplyRotationAroundPivot(
                        verticallyRotatable,
                        targetRotation,
                        verticalPivotPoint.position,
                        useSmoothRotation,
                        rotationSmoothness
                    );
                }
                else
                {
                    // Apply simple local rotation
                    if (useSmoothRotation)
                    {
                        verticallyRotatable.localRotation = Quaternion.Slerp(
                            verticallyRotatable.localRotation,
                            targetRotation,
                            rotationSmoothness * Time.deltaTime
                        );
                    }
                    else
                    {
                        verticallyRotatable.localRotation = targetRotation;
                    }

                    // Restore original position when not using pivot point
                    if (verticallyRotatable.position != originalVerticalPos)
                    {
                        verticallyRotatable.position = originalVerticalPos;
                    }
                }
            }
        }

        /// <summary>
        /// Applies rotation around a specific pivot point
        /// </summary>
        private void ApplyRotationAroundPivot(
            Transform objectToRotate,
            Quaternion targetLocalRotation,
            Vector3 pivotPoint,
            bool useSmoothing,
            float smoothness
        )
        {
            // Store original position and rotation
            Vector3 originalPosition = objectToRotate.position;
            Quaternion originalRotation = objectToRotate.rotation;

            // Calculate the vector from pivot to object
            Vector3 pivotToObject = originalPosition - pivotPoint;

            // Apply rotation
            if (useSmoothing)
            {
                // Apply smooth rotation
                objectToRotate.localRotation = Quaternion.Slerp(
                    objectToRotate.localRotation,
                    targetLocalRotation,
                    smoothness * Time.deltaTime
                );
            }
            else
            {
                // Apply immediate rotation
                objectToRotate.localRotation = targetLocalRotation;
            }

            // Calculate the new rotated offset
            Vector3 rotatedOffset =
                (objectToRotate.rotation * Quaternion.Inverse(originalRotation)) * pivotToObject;

            // Apply the new position
            objectToRotate.position = pivotPoint + rotatedOffset;
        }

        public override void EndInteraction()
        {
            // Fully reset the camera state
            ResetSpotlightCamera(true);

            // Restore cursor state
            Cursor.visible = wasMouseVisible;
            Cursor.lockState = previousCursorLockState;

            Debug.Log("Spotlight interaction ended, camera reset and cursor restored");
        }

        /// <summary>
        /// Reset the spotlight camera to its original state
        /// </summary>
        /// <param name="fullCleanup">If true, destroy camera targets; if false, just reset properties</param>
        private void ResetSpotlightCamera(bool fullCleanup)
        {
            // Restore camera priority
            if (spotlightCamera != null)
            {
                // Force the camera to be VERY low priority to ensure it doesn't take control
                if (
                    spotlightCamera.TryGetComponent<CinemachineVirtualCameraBase>(
                        out var virtualCamera
                    )
                )
                {
                    // Set to a very low priority (lower than original) to guarantee it won't activate
                    virtualCamera.Priority = 1; // Use lowest possible priority
                    Debug.Log($"Force-reset camera priority to 1 (lowest possible)");
                }

                // Set the Follow and LookAt targets to null first to prevent any camera tracking
                spotlightCamera.Follow = null;
                spotlightCamera.LookAt = null;

                // Force the spotlight camera to be inactive when picking up
                if (fullCleanup && spotlightCamera.gameObject != null)
                {
                    // This is a more aggressive approach to make sure the camera doesn't interfere
                    spotlightCamera.enabled = false;

                    // Schedule re-enabling after a short delay to allow other cameras to take control
                    StartCoroutine(ReEnableCameraAfterDelay());
                }
            }

            // Clean up camera targets if requested
            if (fullCleanup)
            {
                CleanupCameraTargets();
            }
        }

        /// <summary>
        /// Re-enable the camera after a short delay to prevent any transition issues
        /// </summary>
        private System.Collections.IEnumerator ReEnableCameraAfterDelay()
        {
            // Wait for a frame to let other things process
            yield return null;

            // Wait another short time to be safe
            yield return new WaitForSeconds(0.1f);

            // Re-enable the camera but keep priority low
            if (spotlightCamera != null)
            {
                spotlightCamera.enabled = true;
                Debug.Log("Re-enabled spotlight camera with low priority");
            }
        }

        private void CleanupCameraTargets()
        {
            // Make sure to clean up completely
            try
            {
                // Set a flag to check if we cleaned anything
                bool cleanedSomething = false;

                // Clear references from the camera first
                if (spotlightCamera != null)
                {
                    if (spotlightCamera.Follow != null || spotlightCamera.LookAt != null)
                    {
                        cleanedSomething = true;
                    }
                    spotlightCamera.Follow = null;
                    spotlightCamera.LookAt = null;
                }

                // Destroy follow target
                if (cameraFollowTarget != null)
                {
                    // Detach from parent before destroying
                    cameraFollowTarget.SetParent(null);
                    Destroy(cameraFollowTarget.gameObject);
                    cameraFollowTarget = null;
                    cleanedSomething = true;
                }

                // Destroy look-at target
                if (cameraLookAtTarget != null)
                {
                    // Detach from parent before destroying
                    cameraLookAtTarget.SetParent(null);
                    Destroy(cameraLookAtTarget.gameObject);
                    cameraLookAtTarget = null;
                    cleanedSomething = true;
                }

                // Find and destroy any orphaned targets that might have been created
                var orphanedTargets = GameObject.FindObjectsOfType<Transform>(true); // Include inactive objects
                int orphansDestroyed = 0;

                foreach (var target in orphanedTargets)
                {
                    // Look for any objects that could be camera targets from this spotlight
                    if (
                        target != null
                        && (
                            (
                                target.name.Contains("LookAtTarget")
                                && target.name.Contains(gameObject.name)
                            )
                            || (
                                target.name.Contains("FollowTarget")
                                && target.name.Contains(gameObject.name)
                            )
                        )
                    )
                    {
                        Destroy(target.gameObject);
                        orphansDestroyed++;
                        cleanedSomething = true;
                    }
                }

                if (cleanedSomething)
                {
                    Debug.Log($"CleanupCameraTargets: Cleaned {orphansDestroyed} orphaned targets");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Error during camera target cleanup: {ex.Message}");
            }
        }

        public void ResetToDefault()
        {
            accumulatedVerticalInput = 0f;
            accumulatedHorizontalInput = 0f;

            if (!useSmoothRotation)
            {
                UpdateRotations();
            }
        }

        public void SetSensitivity(float newSensitivity)
        {
            sensitivity = Mathf.Clamp(newSensitivity, 0.1f, 10f);

            // Update camera speed if active
            if (spotlightCamera != null)
            {
                spotlightCamera.m_XAxis.m_MaxSpeed = 9f * sensitivity;
                spotlightCamera.m_YAxis.m_MaxSpeed = 4f * sensitivity;
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

            // Clamp sensitivity to valid range
            sensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
        } // Methods required by SensitivitySettingsMenu and PlayerPrefsManager

        /// <summary>
        /// Get the current base sensitivity.
        /// </summary>
        public float GetBaseSensitivity()
        {
            return sensitivity;
        }

        /// <summary>
        /// Get the current horizontal sensitivity.
        /// </summary>
        public float GetHorizontalSensitivity()
        {
            return cameraHorizontalSensitivity;
        }

        /// <summary>
        /// Get the current vertical sensitivity.
        /// </summary>
        public float GetVerticalSensitivity()
        {
            return cameraVerticalSensitivity;
        }

        /// <summary>
        /// Set the base sensitivity.
        /// </summary>
        public void SetBaseSensitivity(float value)
        {
            sensitivity = Mathf.Clamp(value, 0.1f, 10f);
            ConfigureCameraSettings();
        }

        /// <summary>
        /// Set the horizontal sensitivity.
        /// </summary>
        public void SetHorizontalSensitivity(float value)
        {
            cameraHorizontalSensitivity = Mathf.Clamp(value, 0.1f, 10f);
            spotlightHorizontalSensitivity = cameraHorizontalSensitivity;
            ConfigureCameraSettings();
        }

        /// <summary>
        /// Set the vertical sensitivity.
        /// </summary>
        public void SetVerticalSensitivity(float value)
        {
            cameraVerticalSensitivity = Mathf.Clamp(value, 0.1f, 10f);
            spotlightVerticalSensitivity = cameraVerticalSensitivity;
            ConfigureCameraSettings();
        }

        /// <summary>
        /// Set all sensitivity values at once.
        /// </summary>
        public void SetAllSensitivity(float baseSens, float horizontalSens, float verticalSens)
        {
            SetBaseSensitivity(baseSens);
            SetHorizontalSensitivity(horizontalSens);
            SetVerticalSensitivity(verticalSens);
        }

        /// <summary>
        /// Reset all sensitivity values to default.
        /// </summary>
        public void ResetSensitivity()
        {
            // Reset all sensitivity values to their defaults
            sensitivity = 3f;
            cameraHorizontalSensitivity = 3f;
            cameraVerticalSensitivity = 3f;
            spotlightHorizontalSensitivity = 3f;
            spotlightVerticalSensitivity = 3f;

            // Update camera settings
            ConfigureCameraSettings();
        }

        /// <summary>
        /// Override to ensure pickup functionality works properly with the spotlight
        /// </summary>
        public override void StartPickup(Transform playerTransform)
        {
            // Important: Check if we're currently in interaction mode (spotlight camera active)
            bool wasInteracting = false; // Check if currently active in a camera
            if (spotlightCamera != null)
            {
                // CRITICAL: Disable the camera component immediately to prevent any chance of activation
                bool wasEnabled = spotlightCamera.enabled;
                spotlightCamera.enabled = false;

                if (
                    spotlightCamera.TryGetComponent<CinemachineVirtualCameraBase>(
                        out var virtualCamera
                    )
                )
                {
                    // Remember if we were in interaction mode
                    wasInteracting = virtualCamera.Priority > 10;

                    // Force priority to absolute minimum
                    virtualCamera.Priority = 0;
                }

                // Use our dedicated method for full camera cleanup with target destruction
                ResetSpotlightCamera(true);

                // Additional direct camera deactivation steps
                if (spotlightCamera.gameObject != null)
                {
                    // Handle any CinemachineBrain components that might reference this camera
                    var brains = FindObjectsOfType<CinemachineBrain>();
                    foreach (var brain in brains)
                    {
                        if (
                            brain.ActiveVirtualCamera != null
                            && brain.ActiveVirtualCamera.Name == spotlightCamera.gameObject.name
                        )
                        {
                            // Force the brain to update by disabling/enabling
                            brain.enabled = false;
                            brain.enabled = true;
                        }
                    }

                    Debug.Log(
                        $"StartPickup: Fully reset spotlight camera (disabled={!wasEnabled}, wasInteracting={wasInteracting})"
                    );
                }
            }

            // Restore cursor state only if we were interacting
            if (wasInteracting)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            // Store the rigidbody reference before base call
            rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                // Add a rigidbody if it doesn't have one
                rigidBody = gameObject.AddComponent<Rigidbody>();
            }

            // Reset the relationship to the player
            hasCalculatedRelativePosition = false;

            // Call the base implementation to handle common pickup logic
            base.StartPickup(playerTransform);

            // CRITICAL: Force the rigidbody settings AFTER base call
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;
            rigidBody.interpolation = RigidbodyInterpolation.Interpolate;

            // Store current position in base class's protected field
            originalPosition = transform.position;

            // Display debug information
            Debug.Log(
                $"Spotlight picked up by player - Kinematic: {rigidBody.isKinematic}, Was interacting: {wasInteracting}"
            );
        }

        /// <summary>
        /// Override to ensure pickup functionality works properly with the spotlight
        /// </summary>
        public override void EndPickup()
        {
            // FIRST: Store our current position before base.EndPickup() potentially changes things
            Vector3 currentPosition = transform.position;

            // Call base implementation
            base.EndPickup();

            // IMPORTANT: After base.EndPickup, ensure we maintain our exact position
            transform.position = currentPosition;

            // Re-validate our rigidbody settings
            if (rigidBody != null)
            {
                // FIXED: Allow the spotlight to fall when dropped by turning off kinematic mode and enabling gravity
                rigidBody.isKinematic = false; // Was set to true, now set to false to allow falling
                rigidBody.useGravity = true; // Was set to false, now set to true to allow gravity
                rigidBody.velocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;

                // Force position update on rigidbody
                rigidBody.position = currentPosition;

                Debug.Log(
                    $"Spotlight dropped at position {currentPosition} - non-kinematic, gravity enabled"
                );
            }
            else
            {
                Debug.LogError("Rigidbody is null during EndPickup! This shouldn't happen.");
                // Just in case, try to get it again
                rigidBody = GetComponent<Rigidbody>();
                if (rigidBody == null)
                {
                    rigidBody = gameObject.AddComponent<Rigidbody>();
                    ConfigureRigidbody();

                    // Ensure it's not kinematic when dropped
                    rigidBody.isKinematic = false;
                    rigidBody.useGravity = true;
                }
            }
        } // Override the base property to use kinematic only while being picked up

        // When dropped, we want the spotlight to fall naturally
        protected override bool PickupIsKinematic => IsPickedUp;

        protected override void UpdatePickupPosition()
        {
            // Check essential components
            if (rigidBody == null || playerTransform == null || !IsPickedUp)
                return;

            if (!hasCalculatedRelativePosition)
            {
                // Calculate position relative to player's forward direction, accounting for the pickup raise amount
                relativePosition = playerTransform.forward * 1.3f + Vector3.up * pickupRaiseAmount;
                hasCalculatedRelativePosition = true;
            }

            // Calculate position by adding relative position to player position
            Vector3 heldPosition = playerTransform.position + relativePosition;

            // Smooth movement using the base class's pickup smoothing value
            Vector3 smoothedPosition = Vector3.Lerp(
                transform.position,
                heldPosition,
                Time.deltaTime * pickupSmoothness
            );

            // Move to follow player with smoothing
            transform.position = smoothedPosition;

            // Update rigidbody as well
            if (rigidBody.isKinematic)
            {
                rigidBody.position = smoothedPosition;
            }
        }

        private void VerifyOutlineComponents()
        {
            if (outlineComponents == null || outlineComponents.Length == 0)
            {
                Debug.LogWarning("No outline components found on spotlight - outlines won't work!");
            }
            else
            {
                Debug.Log($"Found {outlineComponents.Length} outline components on spotlight");

                // Check if any of them are visible
                bool anyOutlineVisible = false;
                foreach (var outline in outlineComponents)
                {
                    if (outline != null && outline.enabled)
                    {
                        anyOutlineVisible = true;
                        break;
                    }
                }

                Debug.Log($"Outline visibility status: {anyOutlineVisible}");
            }

            if (outlineParticlesInstance == null)
            {
                Debug.LogWarning("No outline particle effect found on spotlight");
            }
            else
            {
                Debug.Log(
                    $"Outline particle system status: {(outlineParticlesInstance.isPlaying ? "playing" : "not playing")}"
                );
            }
        }
    }
}
