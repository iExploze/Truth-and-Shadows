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

        // Shared parent for all spotlight camera targets to keep the hierarchy clean
        private static Transform spotlightTargetsParent;

        public override bool RequiresContinuousInteraction => true;

        protected override void Start()
        {
            // Call the base Start method first to initialize outline and rigidbody
            base.Start();

            // Validate and setup components
            ValidateComponents();
            InitializeRotationValues();
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
                    // First, disable all other spotlight cameras to prevent interference
                    DisableAllOtherSpotlightCameras();

                    originalCameraPriority = virtualCamera.Priority;
                    virtualCamera.Priority = 100; // High priority to take control

                    // Ensure this camera is fully enabled
                    spotlightCamera.gameObject.SetActive(true);
                    spotlightCamera.enabled = true;

                    Debug.Log("Spotlight camera activated with priority 100, all others disabled");
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

            // Ensure we have a parent object for all spotlight targets
            if (spotlightTargetsParent == null)
            {
                GameObject parentObj = GameObject.Find("SpotlightTargetsParent");
                if (parentObj == null)
                {
                    parentObj = new GameObject("SpotlightTargetsParent");
                }
                spotlightTargetsParent = parentObj.transform;
            }

            // Position the look-at target in front of the spotlight along its forward direction
            Vector3 targetPosition =
                spotLight.transform.position + spotlightForward * targetDistance;
            cameraLookAtTarget.position = targetPosition;
            cameraLookAtTarget.SetParent(spotlightTargetsParent); // Parent to our organized container

            // Position the follow target behind the spotlight along the same line
            // This ensures the camera, follow target, and look-at target are all along the same line
            float followDistance = 1.5f; // Distance behind spotlight
            Vector3 followPosition =
                spotLight.transform.position
                - spotlightForward * followDistance
                + Vector3.up * 0.5f;
            cameraFollowTarget.position = followPosition;
            cameraFollowTarget.SetParent(spotlightTargetsParent); // Parent to our organized container

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

                // Debug visualization can be added here if needed for testing alignment issues
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
        private static void ApplyRotationAroundPivot(
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

            // Ensure the spotlight's GameObject is completely inactive to prevent any unintended camera activation
            if (spotlightCamera != null)
            {
                // Set lowest priority
                if (
                    spotlightCamera.TryGetComponent<CinemachineVirtualCameraBase>(
                        out var virtualCamera
                    )
                )
                {
                    virtualCamera.Priority = 0;
                }

                // Fully disable the camera object
                spotlightCamera.gameObject.SetActive(false);

                Debug.Log("Spotlight camera completely deactivated on EndInteraction");
            }

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
                    // Completely disable the virtual camera to prevent it from interfering
                    virtualCamera.Priority = 0; // Set to zero (lowest possible)
                    Debug.Log($"Force-reset camera priority to 0 (disabled)");

                    // Explicitly disable the game object to ensure it can't be activated
                    if (fullCleanup)
                    {
                        spotlightCamera.gameObject.SetActive(false);
                        Debug.Log("Completely disabled spotlight camera GameObject");
                    }
                }

                // // Set the Follow and LookAt targets to null first to prevent any camera tracking
                // spotlightCamera.Follow = null;
                // spotlightCamera.LookAt = null;

                // Force the spotlight camera to be inactive when picking up
                if (fullCleanup && spotlightCamera.gameObject != null)
                {
                    // This is a more aggressive approach to make sure the camera doesn't interfere
                    spotlightCamera.enabled = false;
                    spotlightCamera.gameObject.SetActive(false);

                    // Don't re-enable automatically - leave it fully disabled
                    // StartCoroutine(ReEnableCameraAfterDelay()); // DISABLED

                    Debug.Log(
                        "Spotlight camera completely disabled - will not be re-enabled automatically"
                    );
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
        /// This is only used in specific circumstances when we want the camera available but inactive
        /// </summary>
        private System.Collections.IEnumerator ReEnableCameraAfterDelay()
        {
            // Wait for a frame to let other things process
            yield return null;

            // Wait longer to ensure default camera has fully taken control
            yield return new WaitForSeconds(0.5f);

            // Re-enable the camera component but keep priority at zero and GameObject disabled
            if (spotlightCamera != null)
            {
                spotlightCamera.enabled = true;

                // Ensure priority stays at zero
                if (
                    spotlightCamera.TryGetComponent<CinemachineVirtualCameraBase>(
                        out var virtualCamera
                    )
                )
                {
                    virtualCamera.Priority = 0;
                }

                // DO NOT enable the GameObject - this is crucial to prevent it from taking over
                Debug.Log("Re-enabled spotlight camera component but keeping GameObject disabled");
            }
        }

        private void CleanupCameraTargets()
        {
            // Make sure to clean up completely
            try
            {
                // // Set a flag to check if we cleaned anything
                // bool cleanedSomething = false;

                // // Clear references from the camera first
                // if (spotlightCamera != null)
                // {
                //     if (spotlightCamera.Follow != null || spotlightCamera.LookAt != null)
                //     {
                //         cleanedSomething = true;
                //     }
                //     spotlightCamera.Follow = null;
                //     spotlightCamera.LookAt = null;
                // }

                // // Destroy follow target
                // if (cameraFollowTarget != null)
                // {
                //     // Detach from parent before destroying
                //     cameraFollowTarget.SetParent(null);
                //     Destroy(cameraFollowTarget.gameObject);
                //     cameraFollowTarget = null;
                //     cleanedSomething = true;
                // }

                // // Destroy look-at target
                // if (cameraLookAtTarget != null)
                // {
                //     // Detach from parent before destroying
                //     cameraLookAtTarget.SetParent(null);
                //     Destroy(cameraLookAtTarget.gameObject);
                //     cameraLookAtTarget = null;
                //     cleanedSomething = true;
                // }

                // // Find and destroy any orphaned targets that might have been created
                // var orphanedTargets = GameObject.FindObjectsOfType<Transform>(true); // Include inactive objects
                // int orphansDestroyed = 0;

                // foreach (var target in orphanedTargets)
                // {
                //     // Look for any objects that could be camera targets from this spotlight
                //     if (
                //         target != null
                //         && (
                //             (
                //                 target.name.Contains("LookAtTarget")
                //                 && target.name.Contains(gameObject.name)
                //             )
                //             || (
                //                 target.name.Contains("FollowTarget")
                //                 && target.name.Contains(gameObject.name)
                //             )
                //         )
                //     )
                //     {
                //         Destroy(target.gameObject);
                //         orphansDestroyed++;
                //         cleanedSomething = true;
                //     }
                // }

                // if (cleanedSomething)
                // {
                //     Debug.Log($"CleanupCameraTargets: Cleaned {orphansDestroyed} orphaned targets");
                // }
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
        /// Find and disable all spotlight cameras in the scene except the active one
        /// This prevents other spotlight cameras from interfering with the camera system
        /// </summary>
        /// <param name="activeCamera">The spotlight camera to keep active (can be null to disable all)</param>
        private void DisableAllOtherSpotlightCameras()
        {
            // Find all SpotlightController instances in the scene
            SpotlightController[] allSpotlights = FindObjectsOfType<SpotlightController>();

            Debug.Log($"Found {allSpotlights.Length} spotlight controllers in the scene");

            foreach (SpotlightController spotlight in allSpotlights)
            {
                // Skip the current spotlight with the active camera (this is the one we want active)
                if (spotlight == this)
                {
                    // Make sure THIS spotlight camera is fully enabled
                    if (spotlight.spotlightCamera != null)
                    {
                        spotlight.spotlightCamera.gameObject.SetActive(true);
                        spotlight.spotlightCamera.enabled = true;
                        Debug.Log(
                            $"Ensuring THIS spotlight camera is active: {spotlight.gameObject.name}"
                        );
                    }
                    continue;
                }

                // For all other spotlights, ensure their cameras are fully disabled
                if (spotlight.spotlightCamera != null)
                {
                    if (
                        spotlight.spotlightCamera.TryGetComponent<CinemachineVirtualCameraBase>(
                            out var virtualCamera
                        )
                    )
                    {
                        virtualCamera.Priority = 0;
                    }

                    // Disable the camera component
                    spotlight.spotlightCamera.enabled = false;

                    // Disable the camera GameObject
                    if (spotlight.spotlightCamera.gameObject != null)
                    {
                        spotlight.spotlightCamera.gameObject.SetActive(false);
                    }

                    Debug.Log($"Disabled camera for other spotlight: {spotlight.gameObject.name}");
                }
            }
        }
    }
}
