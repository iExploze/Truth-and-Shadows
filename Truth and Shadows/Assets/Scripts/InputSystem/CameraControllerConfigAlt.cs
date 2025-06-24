using Cinemachine;
using UnityEngine;

namespace TruthAndShadows.InputSystem
{
    /// <summary>
    /// Version of CameraControllerConfig that uses input axis names
    /// instead of directly setting input values.
    /// </summary>
    public class CameraControllerConfigAlt : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField]
        public float mouseSensitivity = 1.0f;

        [SerializeField]
        public float controllerSensitivity = 1.0f;

        [SerializeField]
        public bool invertY = true; // Default to inverted vertical for more natural camera feel

        [Header("Input Axes")]
        [SerializeField]
        public string horizontalAxisName = "RightStickHorizontal";

        [SerializeField]
        public string verticalAxisName = "RightStickVertical";

        [SerializeField]
        public bool autoDetectController = true;

        // Store references to modified cameras
        private CinemachineFreeLook[] cameras;

        private void Start()
        {
            // Initialize lastMousePosition
            lastMousePosition = Input.mousePosition;

            // Register for important events
            RegisterForEvents();

            // Configure all cameras
            ConfigureAllCameras();

            Debug.Log(
                "CameraControllerConfigAlt initialized - mouse and keyboard during pickup fix active"
            );
        }

        private void OnEnable()
        {
            ConfigureAllCameras();
        }

        private void OnDisable()
        {
            // Reset cameras to default state
            if (cameras != null)
            {
                foreach (var camera in cameras)
                {
                    if (camera == null)
                        continue;

                    // Reset input values to ensure they work normally when we're disabled
                    camera.m_XAxis.m_InputAxisValue = 0;
                    camera.m_YAxis.m_InputAxisValue = 0;
                }
            }
        }

        /// <summary>
        /// Adds this component as a listener for important Unity events
        /// </summary>
        private void RegisterForEvents()
        {
            // Register for important Unity events to ensure our camera control works consistently
#if UNITY_EDITOR
            Debug.Log("CameraControllerConfigAlt registered for input events");
#endif
            // Reset mouse delta tracking when focus changes
            Application.focusChanged += (hasFocus) =>
            {
                if (hasFocus)
                {
                    lastMousePosition = Input.mousePosition;
                }
            };
        }

        public void ConfigureAllCameras()
        {
            // Find all CinemachineFreeLook cameras in the scene
            cameras = FindObjectsOfType<CinemachineFreeLook>();

            foreach (var camera in cameras)
            {
                ConfigureCamera(camera);
            }

            Debug.Log($"Configured {cameras.Length} cameras for controller input");
        }

        private void ConfigureCamera(CinemachineFreeLook camera)
        {
            if (camera == null)
                return;

            // For mouse/keyboard input
            float mouseXSpeed = 200f * mouseSensitivity;
            float mouseYSpeed = 2f * mouseSensitivity;

            // For controller input
            float controllerXSpeed = 80f * controllerSensitivity;
            float controllerYSpeed = 1f * controllerSensitivity;

            // Choose speed based on input device
            float xSpeed =
                autoDetectController && IsUsingController() ? controllerXSpeed : mouseXSpeed;
            float ySpeed =
                autoDetectController && IsUsingController() ? controllerYSpeed : mouseYSpeed;

            // Configure X axis (horizontal rotation)
            if (autoDetectController && IsUsingController())
            {
                camera.m_XAxis.m_InputAxisName = horizontalAxisName;
            }
            else
            {
                camera.m_XAxis.m_InputAxisName = "Mouse X";
            }

            camera.m_XAxis.m_MaxSpeed = xSpeed;
            camera.m_XAxis.m_AccelTime = 0.05f; // Faster acceleration for more responsive feeling
            camera.m_XAxis.m_DecelTime = 0.1f;

            // Configure Y axis (vertical rotation)
            if (autoDetectController && IsUsingController())
            {
                camera.m_YAxis.m_InputAxisName = verticalAxisName;
            }
            else
            {
                camera.m_YAxis.m_InputAxisName = "Mouse Y";
            }

            camera.m_YAxis.m_MaxSpeed = ySpeed;
            camera.m_YAxis.m_AccelTime = 0.05f; // Faster acceleration for more responsive feeling
            camera.m_YAxis.m_DecelTime = 0.1f;

            // Configure inversions
            camera.m_XAxis.m_InvertInput = false;
            camera.m_YAxis.m_InvertInput = invertY;

            // Disable any automatic recentering to give player full camera control
            camera.m_RecenterToTargetHeading.m_enabled = false;
            camera.m_YAxisRecentering.m_enabled = false;

            Debug.Log($"Configured camera: {camera.name} - recentering disabled");
        }

        // Track input device state
        private bool lastUsingController = false;

        private void Update()
        {
            // Direct fix for the F key holding issue - always monitor for F key
            bool fKeyHeld = Input.GetKey(KeyCode.F); // Apply direct camera control during pickup to fix potential input conflicts
            // This is critical for ensuring camera works while holding F
            // This MUST run before any other camera control logic
            if (fKeyHeld || (InputManager.Instance != null && InputManager.Instance.PickupHeld))
            {
                // Call this EVERY frame when F is held for best results
                EnforceDirectCameraInput();

                // Log diagnostic info periodically
                if (Time.frameCount % 60 == 0) // Once per second at 60fps
                {
                    Debug.Log("F key held: Enforcing direct camera control");

                    // Check if movement is working
                    float h = Input.GetAxis("Horizontal");
                    float v = Input.GetAxis("Vertical");
                    if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
                    {
                        Debug.Log($"Movement input during pickup: ({h}, {v})");
                    }
                }
            }

            // Handle normal controller detection and configuration
            if (autoDetectController && cameras != null && cameras.Length > 0)
            {
                // Check if we've switched input devices
                bool currentUsingController = IsUsingController();

                if (currentUsingController != lastUsingController)
                {
                    Debug.Log(
                        $"Input device changed: {(currentUsingController ? "Controller" : "Mouse/Keyboard")}"
                    );
                    lastUsingController = currentUsingController;

                    // Update camera config for the new input device
                    foreach (var camera in cameras)
                    {
                        ConfigureCamera(camera);
                    }
                }
            }
        }

        private bool IsUsingController()
        {
            float deadzone = 0.2f;

            // Check if InputManager exists and use its detection
            if (InputManager.Instance != null)
            {
                return InputManager.Instance.UsingController;
            }

            // Fallback to direct input check
            try
            {
                // Check for controller input - first check axis movement
                if (Mathf.Abs(Input.GetAxis(horizontalAxisName)) > deadzone)
                    return true;

                if (Mathf.Abs(Input.GetAxis(verticalAxisName)) > deadzone)
                    return true;

                // Then check for any joystick buttons
                for (int i = 0; i < 20; i++)
                {
                    if (Input.GetKey(KeyCode.JoystickButton0 + i))
                        return true;
                }
            }
            catch (System.Exception)
            {
                // Input axes probably don't exist, assume mouse/keyboard
                return false;
            }

            // If we get here, no controller input was detected
            return false;
        }

        /// <summary>
        /// Directly set the camera's input values from raw input sources.
        /// This bypasses any potential input conflicts by directly manipulating the camera.
        /// </summary>
        public void EnforceDirectCameraInput()
        {
            if (cameras == null || cameras.Length == 0)
            {
                // Make sure we have cameras to work with
                ConfigureAllCameras();
                return;
            } // Check if we're using a pickup object (F key held)
            bool isPickupActive =
                Input.GetKey(KeyCode.F)
                || (InputManager.Instance != null && InputManager.Instance.PickupHeld);
            if (!isPickupActive)
                return; // CRITICAL FIX: Get input values that are guaranteed to work during pickup
            Vector2 lookInput = Vector2.zero;
            bool usingInputManagerValues = false; // For most reliable results, check both input sources
            if (InputManager.Instance != null)
            {
                // Use direct access to the InputManager's public property
                lookInput = InputManager.Instance.PickupCameraInput;

                // Check if InputManager provided usable values
                if (lookInput.sqrMagnitude > 0.01f)
                {
                    usingInputManagerValues = true;
                    // Force very high sensitivity during pickup for best results
                    lookInput *= 1.25f;

                    if (Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"[CAMERA FIX] Using InputManager pickup values: {lookInput}");
                    }
                }
            }

            // Fallback to direct mouse delta tracking if InputManager didn't work
            if (!usingInputManagerValues)
            {
                // Get direct mouse delta as primary input source
                Vector2 mouseDelta = GetMouseDeltaPosition();
                float mouseX = mouseDelta.x;
                float mouseY = mouseDelta.y;

                // Check if mouse is providing meaningful input
                if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
                {
                    lookInput = mouseDelta;

                    // Log when we have to use direct mouse tracking during pickup
                    if (Time.frameCount % 120 == 0)
                    {
                        Debug.Log($"Using direct mouse tracking: ({mouseX:F3}, {mouseY:F3})");
                    }
                }
                else
                {
                    // No mouse movement, try controller input
                    float stickX = Input.GetAxisRaw("RightStickHorizontal");
                    float stickY = Input.GetAxisRaw("RightStickVertical");

                    if (Mathf.Abs(stickX) > 0.2f || Mathf.Abs(stickY) > 0.2f)
                    {
                        lookInput = new Vector2(stickX, stickY);

                        if (Time.frameCount % 120 == 0)
                        {
                            Debug.Log($"Using direct controller input: ({stickX:F3}, {stickY:F3})");
                        }
                    }
                }
            }

            // If we have no input at all, exit early
            if (lookInput.sqrMagnitude < 0.001f)
                return;

            // Determine if we're using controller or mouse based on input type
            bool useController =
                autoDetectController
                && (
                    (InputManager.Instance != null && InputManager.Instance.UsingController)
                    || (
                        !usingInputManagerValues
                        && Mathf.Abs(Input.GetAxisRaw("RightStickHorizontal")) > 0.2f
                    )
                );

            // Apply input directly to cameras during pickup
            foreach (var camera in cameras)
            {
                if (camera == null || !camera.isActiveAndEnabled)
                    continue;

                // Make sure axis names are set correctly
                camera.m_XAxis.m_InputAxisName = useController ? horizontalAxisName : "Mouse X";
                camera.m_YAxis.m_InputAxisName = useController ? verticalAxisName : "Mouse Y";

                if (useController)
                {
                    // Apply controller input with appropriate sensitivity
                    float xSpeed = controllerSensitivity * 150f * Time.deltaTime;
                    float ySpeed = controllerSensitivity * 2f * Time.deltaTime;

                    camera.m_XAxis.m_InputAxisValue = lookInput.x * xSpeed;
                    camera.m_YAxis.m_InputAxisValue = invertY
                        ? -lookInput.y * ySpeed
                        : lookInput.y * ySpeed;
                }
                else
                {
                    // Apply mouse input with appropriate sensitivity
                    float xSpeed = mouseSensitivity * 300f * Time.deltaTime;
                    float ySpeed = mouseSensitivity * 3f * Time.deltaTime;

                    camera.m_XAxis.m_InputAxisValue = lookInput.x * xSpeed;
                    camera.m_YAxis.m_InputAxisValue = invertY
                        ? -lookInput.y * ySpeed
                        : lookInput.y * ySpeed;
                }
            }
        }

        private Vector2 lastMousePosition;

        /// <summary>
        /// Get mouse delta between frames, more reliable than Mouse X/Y during pickup
        /// </summary>
        private Vector2 GetMouseDeltaPosition()
        {
            Vector2 mouseDelta = Vector2.zero;

            // Get current mouse position
            Vector2 currentMousePosition = Input.mousePosition;

            // Calculate delta from last frame if we have it
            if (lastMousePosition != Vector2.zero)
            {
                mouseDelta = currentMousePosition - lastMousePosition;
            }

            // Store current position for next frame
            lastMousePosition = currentMousePosition;

            return mouseDelta;
        }
    }
}
