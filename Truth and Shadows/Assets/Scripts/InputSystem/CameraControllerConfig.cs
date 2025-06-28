using Cinemachine;
using UnityEngine;

namespace TruthAndShadows.InputSystem
{
    /// <summary>
    /// Configures all CinemachineFreeLook cameras in the scene to support both mouse and controller input.
    /// Automatically detects input device, applies sensitivity and inversion settings, and ensures smooth camera control during normal gameplay and object pickup.
    /// Handles direct input enforcement to resolve input conflicts (e.g., while holding the pickup key).
    /// </summary>
    public class CameraControllerConfig : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField]
        public float mouseSensitivity = 1.0f;

        [SerializeField]
        public float controllerSensitivity = 1.0f;

        [SerializeField]
        public bool invertY = true;

        [Header("Input Axes")]
        [SerializeField]
        public string horizontalAxisName = "RightStickHorizontal";

        [SerializeField]
        public string verticalAxisName = "RightStickVertical";

        [SerializeField]
        public bool autoDetectController = true;

        private CinemachineFreeLook[] cameras;

        private Vector3 lastMousePosition;

        private bool lastUsingController = false;

        private void Start()
        {
            lastMousePosition = Input.mousePosition;

            RegisterForEvents();

            ConfigureAllCameras();

            Debug.Log("CameraControllerConfig initialized");
        }

        private void OnEnable()
        {
            ConfigureAllCameras();
        }

        private void OnDisable()
        {
            if (cameras != null)
            {
                foreach (var camera in cameras)
                {
                    if (camera == null)
                        continue;
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
            cameras = FindObjectsOfType<CinemachineFreeLook>();

            foreach (var camera in cameras)
            {
                ConfigureCamera(camera);
            }

            Debug.Log($"Configured {cameras.Length} cameras for controller input");
        }

        /// <summary>
        /// Configures a single CinemachineFreeLook camera for both mouse and controller input.
        /// Sets axis names, sensitivity, inversion, and disables recentering for smooth manual control.
        /// Called automatically for all cameras in the scene and whenever input device changes.
        /// </summary>
        private void ConfigureCamera(CinemachineFreeLook camera)
        {
            if (camera == null)
                return;

            float mouseXSpeed = 200f * mouseSensitivity;
            float mouseYSpeed = 2f * mouseSensitivity;

            float controllerXSpeed = 80f * controllerSensitivity;
            float controllerYSpeed = 1f * controllerSensitivity;

            float xSpeed =
                autoDetectController && IsUsingController() ? controllerXSpeed : mouseXSpeed;
            float ySpeed =
                autoDetectController && IsUsingController() ? controllerYSpeed : mouseYSpeed;

            if (autoDetectController && IsUsingController())
            {
                camera.m_XAxis.m_InputAxisName = horizontalAxisName;
            }
            else
            {
                camera.m_XAxis.m_InputAxisName = "Mouse X";
            }

            camera.m_XAxis.m_MaxSpeed = xSpeed;
            camera.m_XAxis.m_AccelTime = 0.05f;
            camera.m_XAxis.m_DecelTime = 0.1f;

            if (autoDetectController && IsUsingController())
            {
                camera.m_YAxis.m_InputAxisName = verticalAxisName;
            }
            else
            {
                camera.m_YAxis.m_InputAxisName = "Mouse Y";
            }

            camera.m_YAxis.m_MaxSpeed = ySpeed;
            camera.m_YAxis.m_AccelTime = 0.05f;
            camera.m_YAxis.m_DecelTime = 0.1f;

            camera.m_XAxis.m_InvertInput = false;
            camera.m_YAxis.m_InvertInput = invertY;

            camera.m_RecenterToTargetHeading.m_enabled = false;
            camera.m_YAxisRecentering.m_enabled = false;

            Debug.Log($"Configured camera: {camera.name} - recentering disabled");
        }

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
            // Check if InputManager exists and use its detection
            if (InputManager.Instance != null)
            {
                return InputManager.Instance.UsingController;
            }

            Debug.LogWarning(
                "InputManager not found! Interaction may not work correctly without it."
            );
            return false;
        }
    }
}
