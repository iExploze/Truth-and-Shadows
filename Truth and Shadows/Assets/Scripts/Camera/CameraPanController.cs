using System.Collections;
using Cinemachine;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// Allows temporary camera panning to a specific virtual camera for a set duration.
    /// After the duration, the camera returns to its original state by leveraging Cinemachine's priority system.
    /// </summary>
    [AddComponentMenu("Truth and Shadows/Camera/Camera Pan Controller")]
    public class CameraPanController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField]
        private CinemachineVirtualCamera panCamera;

        [SerializeField]
        [Tooltip("Duration in seconds that the camera pan will last")]
        private float panDuration = 3.0f;

        [SerializeField]
        [Tooltip("Set to true if you only want this camera pan to happen once")]
        private bool oneTimeOnly = true;

        [SerializeField]
        [Tooltip("Activate the camera pan automatically when the script starts")]
        private bool activateOnStart = false;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugLogs = false;

        // Private variables
        private bool hasBeenActivated = false;
        private Coroutine panCoroutine;
        
        // The very high priority we'll set for the pan camera when active
        private const int HIGH_PRIORITY = 10000;

        private void Start()
        {
            if (panCamera != null)
            {
                // Start with camera disabled (priority 0)
                panCamera.Priority = 0;

                if (showDebugLogs)
                {
                    Debug.Log($"[CameraPanController] Initialized with camera: {panCamera.name}");
                }
            }
            else
            {
                Debug.LogError("[CameraPanController] No virtual camera assigned!");
            }

            if (activateOnStart)
            {
                Activate();
            }
        }

        /// <summary>
        /// Public method to activate the camera pan
        /// </summary>
        public void Activate()
        {
            // Check if we can activate
            if (oneTimeOnly && hasBeenActivated)
            {
                if (showDebugLogs)
                {
                    Debug.Log("[CameraPanController] Camera pan already used (one-time only)");
                }
                return;
            }

            if (panCamera == null)
            {
                Debug.LogError("[CameraPanController] Cannot activate - no camera assigned!");
                return;
            }

            // Cancel any existing pan coroutine
            if (panCoroutine != null)
            {
                StopCoroutine(panCoroutine);
            }

            // Start new pan coroutine
            panCoroutine = StartCoroutine(PanCameraCoroutine());
            hasBeenActivated = true;

            if (showDebugLogs)
            {
                Debug.Log($"[CameraPanController] Activating camera pan for {panDuration} seconds");
            }
        }

        /// <summary>
        /// Public method to manually stop the camera pan before the duration ends
        /// </summary>
        public void DeactivateImmediately()
        {
            if (panCoroutine != null)
            {
                StopCoroutine(panCoroutine);
                panCoroutine = null;
            }

            // Set camera priority to 0 to disable it
            if (panCamera != null)
            {
                panCamera.Priority = 0;
                
                if (showDebugLogs)
                {
                    Debug.Log("[CameraPanController] Camera pan manually deactivated");
                }
            }
        }

        /// <summary>
        /// Coroutine to handle the camera pan timing
        /// </summary>
        private IEnumerator PanCameraCoroutine()
        {
            // Simply set our pan camera to a very high priority to override all other cameras
            panCamera.gameObject.SetActive(true);
            panCamera.Priority = HIGH_PRIORITY;

            if (showDebugLogs)
            {
                Debug.Log($"[CameraPanController] Camera {panCamera.name} set to priority {HIGH_PRIORITY}");
            }

            // Wait for the specified duration
            yield return new WaitForSeconds(panDuration);

            // Just set priority to 0 to disable the pan camera
            // Cinemachine will automatically switch to the next highest priority camera
            panCamera.Priority = 0;

            panCoroutine = null;

            if (showDebugLogs)
            {
                Debug.Log("[CameraPanController] Camera pan completed");
            }
        }

        /// <summary>
        /// Public method to camera pan with a custom duration
        /// </summary>
        public void CameraPan(float customDuration = -1)
        {
            // Use the custom duration if provided, otherwise use the default
            if (customDuration > 0)
            {
                panDuration = customDuration;
            }

            Activate();
        }
    }
}
