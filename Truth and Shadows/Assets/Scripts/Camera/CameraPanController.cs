using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TruthAndShadows.InputSystem;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// Allows temporary camera panning to a specific virtual camera for a set duration.
    /// After the duration, the camera returns to its original state.
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
        [Tooltip("Priority to set for the camera during panning (should be high)")]
        private int cameraPriority = 100;

        [SerializeField]
        [Tooltip("Smooth transition time when activating/deactivating the camera pan")]
        private float blendTime = 0.5f;

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
        private int originalCameraPriority = 10;
        private Coroutine panCoroutine;
        private Dictionary<CinemachineVirtualCameraBase, int> originalCameraPriorities =
            new Dictionary<CinemachineVirtualCameraBase, int>();

        private void Start()
        {
            // Store the original priority
            if (panCamera != null)
            {
                originalCameraPriority = panCamera.Priority;

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

            RestoreOriginalCameras();

            if (showDebugLogs)
            {
                Debug.Log("[CameraPanController] Camera pan manually deactivated");
            }
        }

        /// <summary>
        /// Coroutine to handle the camera pan timing and transitions
        /// </summary>
        private IEnumerator PanCameraCoroutine()
        {
            // Store original camera priorities and disable other cameras
            StoreAndDisableOtherCameras();

            // Activate our camera with high priority
            panCamera.gameObject.SetActive(true);
            panCamera.Priority = cameraPriority;

            if (showDebugLogs)
            {
                Debug.Log(
                    $"[CameraPanController] Camera {panCamera.name} set to priority {cameraPriority}"
                );
            }

            // Wait for the specified duration
            yield return new WaitForSeconds(panDuration);

            // Restore original camera states
            RestoreOriginalCameras();

            panCoroutine = null;

            if (showDebugLogs)
            {
                Debug.Log("[CameraPanController] Camera pan completed");
            }
        }

        /// <summary>
        /// Stores original camera priorities and disables all cameras except the pan camera
        /// </summary>
        private void StoreAndDisableOtherCameras()
        {
            // Clear any previous stored priorities
            originalCameraPriorities.Clear();

            // Find all virtual cameras in the scene
            var allCameras = FindObjectsOfType<CinemachineVirtualCameraBase>();

            foreach (var cam in allCameras)
            {
                // Skip our pan camera
                if (cam.gameObject == panCamera.gameObject)
                {
                    continue;
                }

                // Store original priority
                originalCameraPriorities[cam] = cam.Priority;

                // Disable by setting priority to 0
                cam.Priority = 0;

                if (showDebugLogs && cam.Priority > 0)
                {
                    Debug.Log(
                        $"[CameraPanController] Stored camera {cam.name} with priority {originalCameraPriorities[cam]}"
                    );
                }
            }
        }

        /// <summary>
        /// Restores all cameras to their original priorities
        /// </summary>
        private void RestoreOriginalCameras()
        {
            // Restore original camera priorities
            foreach (var cameraPair in originalCameraPriorities)
            {
                if (cameraPair.Key != null)
                {
                    cameraPair.Key.Priority = cameraPair.Value;

                    if (showDebugLogs)
                    {
                        Debug.Log(
                            $"[CameraPanController] Restored {cameraPair.Key.name} to priority {cameraPair.Value}"
                        );
                    }
                }
            }

            // Reset our pan camera to its original state
            if (panCamera != null)
            {
                panCamera.Priority = originalCameraPriority;

                if (showDebugLogs)
                {
                    Debug.Log(
                        $"[CameraPanController] Reset pan camera to priority {originalCameraPriority}"
                    );
                }
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
