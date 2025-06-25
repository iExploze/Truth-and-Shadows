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
            ConfigureAllCameras();
        }

        private void OnEnable()
        {
            ConfigureAllCameras();
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
    }
}
