/*  Truth and Shadows – Dolly Nav Tour
    ──────────────────────────────────
    Put this on a GameObject in your checkpoint prefab.
    Requirements:
      • a CinemachineVirtualCamera with a CinemachineDollyCart component
      • the DollyCart’s Track field to a Cinemachine Dolly Track in the scene
      • “Position Units” on the DollyCart set to Distance   */

using System.Collections;
using Cinemachine;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    public class CameraPathPanController : MonoBehaviour
    {
        private const int HIGH_PRIORITY = 10000;
        private const int DISABLED_PRIORITY = 0;

        [Header("Camera Settings")]
        [SerializeField]
        private CinemachineVirtualCamera vcam;

        [SerializeField]
        private CinemachineDollyCart dollyCart;

        [Header("Movement")]
        [SerializeField]
        private float moveSpeed = 4f;

        [SerializeField]
        private float speedTransitionTime = 0.2f; // Time to smoothly change speed

        [SerializeField]
        private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Focus Points")]
        [SerializeField]
        private float focusHoldTime = 0.2f;

        [SerializeField]
        private float rollThreshold = 0.99f; // Minimum roll angle to trigger a pause

        [Header("Camera Priority")]
        [SerializeField]
        private int tourPriority = 15;

        [Header("Object Control")]
        [SerializeField]
        private SpinObject targetSpinObject;  // Reference to the spinning object

        private float originalSpinSpeed;  // Store original speed

        [Header("Playback Settings")]
        [SerializeField]
        private bool playOnce = true;

        private bool tourRunning;
        private bool hasPlayed;
        private float initialSpeed;

        private float targetSpeed;
        private float currentSpeed;
        private Coroutine speedTransitionCoroutine;
        private float lastPausePosition = -1f; // Track the last position we paused at
        private float minPauseDistance = 1f; // Minimum distance between pause points

        private void Start()
        {
            if (vcam == null)
            {
                vcam = GetComponent<CinemachineVirtualCamera>();
            }

            if (dollyCart == null)
            {
                dollyCart = GetComponent<CinemachineDollyCart>();
            }

            if (vcam != null && dollyCart != null)
            {
                // Start with camera disabled and cart stopped
                vcam.Priority = DISABLED_PRIORITY;
                initialSpeed = dollyCart.m_Speed;
                currentSpeed = 0;
                targetSpeed = 0;
                dollyCart.m_Speed = 0;
                Debug.Log($"[CameraPathPanController] Initialized with camera: {vcam.name}");
            }
            else
            {
                Debug.LogError("[CameraPathPanController] Missing required components!");
            }
        }

        private void Update()
        {
            // Smoothly update the cart's speed
            if (dollyCart != null && Mathf.Abs(dollyCart.m_Speed - targetSpeed) > 0.01f)
            {
                currentSpeed = Mathf.Lerp(
                    currentSpeed,
                    targetSpeed,
                    Time.deltaTime / speedTransitionTime
                );
                dollyCart.m_Speed = currentSpeed;
            }
        }

        public void PlayTourIfAble()
        {
            if (playOnce && hasPlayed)
            {
                Debug.Log("[CameraPathPanController] Tour already played (one-time only)");
                return;
            }

            if (tourRunning || vcam == null || dollyCart == null)
            {
                Debug.LogWarning("[CameraPathPanController] Cannot play tour - check setup");
                return;
            }

            // Store original spin speed if we have a target object
            if (targetSpinObject != null)
            {
                originalSpinSpeed = targetSpinObject.getSpinSpeed();
            }

            // Set high priority to override other cameras
            vcam.Priority = HIGH_PRIORITY;
            Debug.Log(
                $"[CameraPathPanController] Camera {vcam.name} set to priority {HIGH_PRIORITY}"
            );

            StartCoroutine(TourRoutine());
            hasPlayed = true;
        }

        private IEnumerator TourRoutine()
        {
            tourRunning = true;

            // Stop spinning if we have a target object
            if (targetSpinObject != null)
            {
                targetSpinObject.setSpinSpeed(0);
            }

            // Start movement
            SetTargetSpeed(moveSpeed);

            while (dollyCart.m_Position < dollyCart.m_Path.PathLength)
            {
                // Check if we're at a focus point
                if (ShouldPauseAtCurrentPosition())
                {
                    yield return HandleFocusPoint();
                }

                yield return null;
            }

            // Ensure we've reached the final position
            dollyCart.m_Position = dollyCart.m_Path.PathLength;
            yield return new WaitForSeconds(0.5f); // Short delay at end

            // Reset and disable
            SetTargetSpeed(0);
            vcam.Priority = DISABLED_PRIORITY;

            // Resume spinning if we have a target object
            if (targetSpinObject != null)
            {
                targetSpinObject.setSpinSpeed(originalSpinSpeed);
            }

            tourRunning = false;
            Debug.Log(
                $"[CameraPathPanController] Tour completed, camera disabled (Priority: {DISABLED_PRIORITY})"
            );
        }

        private void SetTargetSpeed(float speed)
        {
            targetSpeed = speed;
            currentSpeed = dollyCart.m_Speed; // Start from current speed
        }

        private bool ShouldPauseAtCurrentPosition()
        {
            if (dollyCart?.m_Path == null)
                return false;

            float pathPosition = dollyCart.m_Position;

            // Don't pause if we're too close to the last pause point
            if (
                lastPausePosition >= 0
                && Mathf.Abs(pathPosition - lastPausePosition) < minPauseDistance
            )
            {
                return false;
            }

            // Check if we're using a CinemachineSmoothPath
            var smoothPath = dollyCart.m_Path as CinemachineSmoothPath;
            if (smoothPath == null)
            {
                Debug.LogWarning("[CameraPathPanController] Path must be a CinemachineSmoothPath");
                return false;
            }

            // Find the nearest waypoint
            int waypointCount = smoothPath.m_Waypoints.Length;
            float nearestWaypointDist = float.MaxValue;
            int nearestWaypointIndex = -1;

            // Convert current position to a normalized path position (0 to 1)
            float normalizedPosition = pathPosition / smoothPath.PathLength;

            for (int i = 0; i < waypointCount; i++)
            {
                // Get the normalized position of this waypoint
                float wpPosition = (float)i / (waypointCount - 1);
                float dist = Mathf.Abs(normalizedPosition - wpPosition);

                if (dist < nearestWaypointDist)
                {
                    nearestWaypointDist = dist;
                    nearestWaypointIndex = i;
                }
            }

            // Only check roll if we're very close to a waypoint (in normalized space)
            if (nearestWaypointDist > 0.01f) // Adjusted threshold for normalized space
            {
                return false;
            }

            // Get the roll directly from the waypoint
            float roll = smoothPath.m_Waypoints[nearestWaypointIndex].roll;
            bool shouldPause = Mathf.Abs(roll) > rollThreshold;

            if (shouldPause)
            {
                lastPausePosition = pathPosition;
                Debug.Log(
                    $"[CameraPathPanController] Pause point detected at waypoint {nearestWaypointIndex} with roll {roll:F2}"
                );
            }

            return shouldPause;
        }

        private IEnumerator HandleFocusPoint()
        {
            // Smoothly stop
            SetTargetSpeed(0);

            // Wait until we've actually stopped
            yield return new WaitUntil(() => Mathf.Abs(dollyCart.m_Speed) < 0.01f);

            // Hold at focus point
            yield return new WaitForSeconds(focusHoldTime);

            // Smoothly resume
            SetTargetSpeed(moveSpeed);
        }

        public void PauseTour()
        {
            if (tourRunning)
            {
                SetTargetSpeed(0);
            }
        }

        public void ResumeTour()
        {
            if (tourRunning)
            {
                SetTargetSpeed(moveSpeed);
            }
        }

        private void ResetTour()
        {
            lastPausePosition = -1f; // Reset the last pause position
            // ...existing code...
        }
    }
}
