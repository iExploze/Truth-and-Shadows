using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TruthAndShadows.InputSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TruthAndShadows.Interaction
{
    public class InteractionManager : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField]
        private float interactionRange = 0.1f;

        [SerializeField]
        private float interactionRadius = 0.1f;

        [SerializeField]
        private Transform interactionSource;

        [Header("References")]
        [SerializeField]
        private MonoBehaviour playerController;

        // Reference to the input context provider
        private MonoBehaviour inputContextProvider;

        [Header("Camera")]
        [SerializeField]
        private CinemachineVirtualCamera defaultCamera;
        private Component currentInteractionCamera;

        // Dictionary to store original camera priorities before interaction
        private Dictionary<CinemachineVirtualCameraBase, int> originalCameraPriorities =
            new Dictionary<CinemachineVirtualCameraBase, int>();

        [Header("Camera Debug")]
        [SerializeField]
        private bool logCameraChanges = true;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugRay = true;

        private IInteractable currentInteractable;
        private bool isInteracting;
        private IInteractable pickedUpInteractable;
        private bool UseLoadingIntermediaryScene = true;

        void Start()
        {
            InitializeSource();
            Cursor.visible = false;

            // Try to find the InputContextProvider
            inputContextProvider =
                FindObjectOfType(
                    System.Type.GetType("TruthAndShadows.InputSystem.InputContextProvider")
                ) as MonoBehaviour;
            if (inputContextProvider == null)
            {
                Debug.LogWarning(
                    "InputContextProvider not found! Input permissions will default to allowed."
                );
            }

            // Get player controller if not already assigned
            if (playerController == null)
            {
                // Try to find it by type name to avoid direct reference issues
                playerController =
                    FindObjectOfType(System.Type.GetType("TruthAndShadows.Player.PlayerController"))
                    as MonoBehaviour;
                if (playerController == null)
                {
                    Debug.LogWarning(
                        "PlayerController not found! Interactable custom conditions may not work properly."
                    );
                }
            }
        }

        void Update()
        {
            HandleInteractionInput();
            UpdateContinuousInteraction();
        }

        private void InitializeSource()
        {
            if (interactionSource == null)
                interactionSource = transform;
        }

        /// <summary>
        /// Tries to invoke a method on an InteractableEvents component attached to the given GameObject
        /// </summary>
        /// <param name="gameObject">The GameObject that might have an InteractableEvents component</param>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="interactorGameObject">The GameObject to pass to the method</param>
        private void TryInvokeInteractableEvent(
            GameObject gameObject,
            string methodName,
            GameObject interactorGameObject
        )
        {
            if (
                gameObject == null
                || string.IsNullOrEmpty(methodName)
                || interactorGameObject == null
            )
                return;

            try
            {
                // Find components with names containing "InteractableEvents"
                var components = gameObject.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (component.GetType().Name.Contains("InteractableEvents"))
                    {
                        // Try to find the method by name
                        var method = component.GetType().GetMethod(methodName);
                        if (method != null)
                        {
                            // Invoke the method with the interactor GameObject
                            method.Invoke(component, new object[] { interactorGameObject });
                            break;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error invoking interactable event: {e.Message}");
            }
        }

        /// <summary>
        /// Checks if an interactable can be interacted with based on its own custom conditions
        /// </summary>
        /// <param name="interactable">The interactable to check</param>
        /// <returns>True if the interactable can be interacted with, false otherwise</returns>
        private bool CheckInteractableConditions(IInteractable interactable)
        {
            try
            {
                // If the playerController is set, pass it to the CanInteract method
                if (playerController != null)
                {
                    // Directly call the interface method (no reflection needed anymore)
                    return interactable.CanInteract(playerController);
                }
                else
                {
                    // Fall back to returning true if we don't have a player reference
                    Debug.LogWarning(
                        "PlayerController reference not set in InteractionManager. Assuming interaction is allowed."
                    );
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking interactable conditions: {e.Message}");
                return true; // Default to allowing interaction on error
            }
        }

        private void HandleInteractionInput()
        {
            // Check if InputManager is available
            if (InputManager.Instance == null)
            {
                Debug.LogError(
                    "InputManager.Instance is null! This will prevent interaction input from working correctly."
                );
                return;
            } // Use InputManager's properties for consistent input handling across devices            // Check if interaction is allowed based on the current player state
            // Using the centralized InputContextProvider to check permissions
            bool canInteract = true; // Default to allowed            // Get permission from InputContextProvider if available
            if (inputContextProvider != null)
            {
                // Use reflection to safely access the CanInteract property
                System.Reflection.PropertyInfo propertyInfo = inputContextProvider
                    .GetType()
                    .GetProperty("CanInteract");
                if (propertyInfo != null)
                {
                    bool? value = propertyInfo.GetValue(inputContextProvider) as bool?;
                    if (value.HasValue)
                    {
                        canInteract = value.Value;
                    }
                }
            }

            if (InputManager.Instance.InteractPressed && canInteract)
            {
                Debug.Log("Interact button pressed and allowed - attempting interaction");

                // If we have a picked-up item, drop it before starting a new interaction
                if (pickedUpInteractable != null)
                {
                    DropPickedUpItem();
                }

                TryStartInteraction();
            }
            else if (InputManager.Instance.InteractReleased)
            {
                Debug.Log("Interact button released - ending interaction");
                EndCurrentInteraction();
            }

            // Handle pickup functionality
            HandlePickupInput();
            Reset();
        }

        private void UpdateContinuousInteraction()
        {
            if (isInteracting && currentInteractable?.RequiresContinuousInteraction == true)
                currentInteractable.ContinueInteraction();
        }

        private void TryStartInteraction()
        {
            //Debug.Log("TryStartInteraction called");

            if (!IsValidSource())
            {
                //Debug.LogWarning("Invalid interaction source!");
                return;
            }

            Vector3 origin = GetInteractionOrigin();
            Vector3 direction = interactionSource.forward;

            //Debug.Log(
            //    $"Interaction ray: Origin={origin}, Direction={direction}, Range={interactionRange}"
            //);

            if (showDebugRay)
                Debug.DrawRay(origin, direction * interactionRange, Color.yellow, 0.1f);
            if (TryFindInteractable(origin, direction, out IInteractable interactable))
            {
                Debug.Log($"Found interactable: {((MonoBehaviour)interactable).gameObject.name}");

                // Check if the interactable can be interacted with based on its own conditions
                bool canInteract = CheckInteractableConditions(interactable);
                if (canInteract)
                {
                    currentInteractable = interactable;
                    isInteracting = true;

                    // Start the interaction
                    currentInteractable.StartInteraction();
                    // Find attached InteractableEvents component if it exists (using reflection since we might have assembly reference issues)
                    var interactableGameObject = ((MonoBehaviour)interactable).gameObject;
                    TryInvokeInteractableEvent(
                        interactableGameObject,
                        "InvokeContinuedInteraction",
                        playerController?.gameObject
                    );

                    // Switch to interactable's camera if it has one
                    if (currentInteractable.InteractionCamera != null)
                    {
                        currentInteractionCamera = currentInteractable.InteractionCamera;
                        SwitchToCamera(currentInteractionCamera);
                    }
                }
                else
                {
                    Debug.Log("Interactable conditions not met - interaction denied");
                    // Find attached InteractableEvents component if it exists
                    var interactableGameObject = ((MonoBehaviour)interactable).gameObject;
                    TryInvokeInteractableEvent(
                        interactableGameObject,
                        "InvokeInteractionFailed",
                        playerController?.gameObject
                    );
                }
            }
            else
            {
                Debug.Log("No interactable found in range");
            }
        }

        private void SwitchToCamera(Component camera)
        {
            if (logCameraChanges)
                Debug.Log($"Switching to camera: {camera.gameObject.name}");

            // Clear any previous camera priorities
            originalCameraPriorities.Clear();

            // Disable all Cinemachine cameras in the scene except the one we're switching to
            // This will also store their original priorities
            DisableAllCameras(camera); // Increase priority of the new camera
            // Use a higher priority to ensure it takes precedence over any other camera
            SetCameraPriority(camera, 999); // Using a very high priority to guarantee it takes precedence
        }

        private void DisableAllCameras(Component exceptCamera)
        {
            // Store original priorities before disabling
            if (originalCameraPriorities.Count == 0)
            {
                if (logCameraChanges)
                    Debug.Log("Storing original camera priorities before interaction");

                // Clear the dictionary in case there's anything left from a previous call
                originalCameraPriorities.Clear();
            }

            // Find all Cinemachine Virtual Camera Base objects in the scene
            var vcamBases = FindObjectsOfType<CinemachineVirtualCameraBase>();

            if (logCameraChanges)
                Debug.Log($"Found {vcamBases.Length} Cinemachine cameras in the scene");

            foreach (var cam in vcamBases)
            {
                // Skip the camera we want to activate
                if (exceptCamera != null && cam.gameObject == exceptCamera.gameObject)
                {
                    continue;
                }

                // Get current priority and store it if we haven't already
                int currentPriority = cam.Priority;
                if (!originalCameraPriorities.ContainsKey(cam))
                {
                    originalCameraPriorities.Add(cam, currentPriority);
                    if (logCameraChanges)
                        Debug.Log(
                            $"Stored original priority for {cam.gameObject.name}: {currentPriority}"
                        );
                }

                // Skip any cameras that are already at priority 0
                if (currentPriority == 0)
                {
                    continue;
                }

                // Set priority to 0
                if (logCameraChanges)
                    Debug.Log(
                        $"Disabling camera: {cam.gameObject.name} (was priority: {currentPriority})"
                    );

                cam.Priority = 0;
            }

            // If defaultCamera isn't part of the cameras found above, explicitly handle it
            if (
                defaultCamera != null
                && (exceptCamera == null || defaultCamera.gameObject != exceptCamera.gameObject)
            )
            {
                // Store its priority if we haven't already
                if (!originalCameraPriorities.ContainsKey(defaultCamera))
                {
                    originalCameraPriorities.Add(defaultCamera, defaultCamera.Priority);
                    if (logCameraChanges)
                        Debug.Log(
                            $"Stored original priority for default camera: {defaultCamera.Priority}"
                        );
                }

                defaultCamera.Priority = 0;
            }
        } // This method is kept for backward compatibility

        private void DisableAllCameras()
        {
            DisableAllCameras(null);
        }

        private void SetCameraPriority(Component camera, int priority)
        {
            if (camera == null)
                return;

            string cameraName = camera.gameObject.name;

            // Handle different types of Cinemachine cameras
            if (camera is CinemachineVirtualCamera vcam)
            {
                if (logCameraChanges)
                    Debug.Log(
                        $"Setting CinemachineVirtualCamera '{cameraName}' priority to {priority}"
                    );
                vcam.Priority = priority;
            }
            else if (camera is CinemachineFreeLook freelook)
            {
                if (logCameraChanges)
                    Debug.Log($"Setting CinemachineFreeLook '{cameraName}' priority to {priority}");

                // For freelook cameras, ensure we're setting a high enough priority difference
                // to override any other cameras in the scene
                freelook.Priority = priority;

                // Check if we actually need to reset the camera position
                if (priority == 0)
                {
                    // Clear targets to avoid lingering influences
                    if (freelook.Follow != null || freelook.LookAt != null)
                    {
                        if (logCameraChanges)
                            Debug.Log($"Clearing targets for FreeLook camera '{cameraName}'");
                    }
                }
            }
            else if (camera is CinemachineBrain brain)
            {
                // Special handling for CinemachineBrain if needed
                if (logCameraChanges)
                    Debug.Log("CinemachineBrain camera handling not implemented");
            }
            else
            {
                // Try to use reflection to set priority for other Cinemachine camera types
                var priorityProperty = camera.GetType().GetProperty("Priority");
                if (priorityProperty != null && priorityProperty.PropertyType == typeof(int))
                {
                    if (logCameraChanges)
                        Debug.Log(
                            $"Setting {camera.GetType().Name} '{cameraName}' priority to {priority} via reflection"
                        );
                    priorityProperty.SetValue(camera, priority);
                }
                else
                {
                    Debug.LogWarning(
                        $"Unsupported camera type: {camera.GetType().Name}. Cannot set priority."
                    );
                }
            }
        }

        private bool IsValidSource()
        {
            return interactionSource != null;
        }

        private Vector3 GetInteractionOrigin()
        {
            // Offset the origin slightly backward to ensure detection when close to objects
            Vector3 offset =
                interactionSource.position - (interactionSource.forward * interactionRadius);
            return offset + Vector3.up;
        }

        private bool TryFindInteractable(
            Vector3 origin,
            Vector3 direction,
            out IInteractable interactable
        )
        {
            interactable = null;

            // First attempt - sphere cast to find interactables in line of sight
            RaycastHit hit;
            if (Physics.SphereCast(origin, interactionRadius, direction, out hit, interactionRange))
            {
                interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    return true;
                }
            }

            // Second attempt - proximity check for items very close to the player
            // This helps with interactables that might be partially behind the player or just outside the forward view
            Collider[] nearbyColliders = Physics.OverlapSphere(origin, interactionRadius * 1.5f);

            float closestDistance = float.MaxValue;
            IInteractable closestInteractable = null;

            foreach (Collider col in nearbyColliders)
            {
                IInteractable potentialInteractable = col.GetComponent<IInteractable>();
                if (potentialInteractable != null)
                {
                    // Check distance to find the closest one
                    float distance = Vector3.Distance(origin, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = potentialInteractable;
                    }
                }
            }

            // If we found any interactable in proximity and it's within our interaction range
            if (closestInteractable != null && closestDistance <= interactionRange)
            {
                interactable = closestInteractable;
                return true;
            }

            return false;
        }

        public void EndCurrentInteraction()
        {
            if (!isInteracting || currentInteractable == null)
                return;

            // Get a reference to the interactable's GameObject before ending the interaction
            GameObject interactableGameObject = ((MonoBehaviour)currentInteractable).gameObject;

            // End the interaction
            currentInteractable.EndInteraction();
            // Find attached InteractableEvents component if it exists
            TryInvokeInteractableEvent(
                interactableGameObject,
                "InvokeInteractionEnded",
                playerController?.gameObject
            );

            // First disable the interaction camera
            if (currentInteractionCamera != null)
            {
                if (logCameraChanges)
                    Debug.Log(
                        $"Setting interaction camera {currentInteractionCamera.gameObject.name} priority to 0"
                    );

                SetCameraPriority(currentInteractionCamera, 0);
                currentInteractionCamera = null;
            }

            // Restore all camera priorities to their original values
            RestoreAllCameraPriorities();

            // Clear interaction state for all types of interactions
            currentInteractable = null;
            isInteracting = false;
        }

        /// <summary>
        /// Restores all camera priorities to their original values before the interaction
        /// </summary>
        private void RestoreAllCameraPriorities()
        {
            if (originalCameraPriorities.Count == 0)
            {
                Debug.LogWarning("No original camera priorities stored to restore!");

                // Fallback to default camera if available
                if (defaultCamera != null)
                {
                    if (logCameraChanges)
                        Debug.Log(
                            $"Fallback: Setting default camera {defaultCamera.gameObject.name} priority to 10"
                        );

                    defaultCamera.Priority = 10;
                }
                return;
            }

            if (logCameraChanges)
                Debug.Log($"Restoring {originalCameraPriorities.Count} camera priorities...");

            // Restore all stored priorities
            foreach (var cameraPair in originalCameraPriorities)
            {
                CinemachineVirtualCameraBase camera = cameraPair.Key;
                int originalPriority = cameraPair.Value;

                if (camera == null)
                {
                    Debug.LogWarning("Stored camera reference is null, cannot restore priority");
                    continue;
                }

                if (logCameraChanges)
                    Debug.Log($"Restoring {camera.gameObject.name} priority to {originalPriority}");

                camera.Priority = originalPriority;
            }

            // Clear the stored priorities after restoring
            originalCameraPriorities.Clear();
        }

        public void PreserveInteraction()
        {
            if (currentInteractable?.RequiresContinuousInteraction == true)
            {
                // Safe null check for InputManager.Instance
                if (InputManager.Instance != null)
                {
                    isInteracting = InputManager.Instance.InteractHeld;
                }
                else
                {
                    Debug.LogError("InputManager.Instance is null in PreserveInteraction!");
                    // Default to not interacting if we can't check the button state
                    isInteracting = false;
                }
            }
        }

        private void HandlePickupInput()
        {
            // Check if InputManager is available
            if (InputManager.Instance == null)
            {
                Debug.LogError("InputManager.Instance is null! Cannot process pickup input.");
                return;
            } // Check if pickup is allowed based on the current player state
            bool canPickup = true; // Default to allowed
            // Get permission from InputContextProvider if available
            if (inputContextProvider != null)
            {
                // Use reflection to safely access the CanPickup property
                System.Reflection.PropertyInfo propertyInfo = inputContextProvider
                    .GetType()
                    .GetProperty("CanPickup");
                if (propertyInfo != null)
                {
                    bool? value = propertyInfo.GetValue(inputContextProvider) as bool?;
                    if (value.HasValue)
                    {
                        canPickup = value.Value;
                    }
                }
            }

            if (InputManager.Instance.PickupPressed && pickedUpInteractable == null && canPickup)
            {
                Debug.Log("Pickup button pressed and allowed - attempting pickup");
                TryPickupItem();
            }
            else if (InputManager.Instance.PickupReleased && pickedUpInteractable != null)
            {
                Debug.Log("Pickup button released - dropping item");
                DropPickedUpItem();
            }
        }

        private void Reset()
        {
            // Check if InputManager is available
            if (InputManager.Instance == null)
            {
                Debug.LogError("InputManager.Instance is null! Cannot process reset input.");
                return;
            } // Check if reset is allowed based on the current player state
            bool canReset = true; // Default to allowed
            // Get permission from InputContextProvider if available
            if (inputContextProvider != null)
            {
                // Use reflection to safely access the CanReset property
                System.Reflection.PropertyInfo propertyInfo = inputContextProvider
                    .GetType()
                    .GetProperty("CanReset");
                if (propertyInfo != null)
                {
                    bool? value = propertyInfo.GetValue(inputContextProvider) as bool?;
                    if (value.HasValue)
                    {
                        canReset = value.Value;
                    }
                }
            }

            if (InputManager.Instance.ResetPressed && canReset)
            {
                Debug.Log("Reset button pressed and allowed - reloading scene");
                LevelManager.Instance.LoadScene(SceneManager.GetActiveScene().name, "CrossFade");
            }
        }

        private void TryPickupItem()
        {
            if (!IsValidSource())
            {
                Debug.LogWarning("Invalid interaction source for pickup!");
                return;
            }

            Vector3 origin = GetInteractionOrigin();
            Vector3 direction = interactionSource.forward;

            if (
                TryFindInteractable(origin, direction, out IInteractable interactable)
                && interactable.CanBePickedUp
                && !interactable.IsPickedUp
            )
            {
                if (isInteracting && currentInteractable == interactable)
                {
                    EndCurrentInteraction();
                }
                pickedUpInteractable = interactable;
                interactable.StartPickup(interactionSource);

                // Restore camera priorities when picking up an item
                if (currentInteractionCamera != null)
                {
                    SetCameraPriority(currentInteractionCamera, 0);
                    currentInteractionCamera = null;
                }

                // Restore all camera priorities to their original values
                RestoreAllCameraPriorities();

                Debug.Log($"Picked up: {((MonoBehaviour)interactable).gameObject.name}");
            }
            else
            {
                //Debug.Log(interactable == null ? "No interactable found for pickup" : $"Item cannot be picked up: {((MonoBehaviour)interactable).gameObject.name}");
            }
        }

        public void DropPickedUpItem()
        {
            if (pickedUpInteractable != null)
            {
                //Debug.Log($"Dropping: {((MonoBehaviour)pickedUpInteractable).gameObject.name}");
                pickedUpInteractable.EndPickup();
                pickedUpInteractable = null;
            }
        }

        /// <summary>
        /// Resets to the default camera, ensuring its priority is set correctly.
        /// Can be called from other scripts or events to ensure proper camera state.
        /// </summary>
        public void ResetToDefaultCamera()
        {
            if (logCameraChanges)
                Debug.Log("Explicitly resetting to default camera");

            if (originalCameraPriorities.Count > 0)
            {
                // Restore all camera priorities if we have them stored
                RestoreAllCameraPriorities();
            }
            else
            {
                // If we don't have stored priorities, just ensure default camera is enabled
                if (defaultCamera != null)
                {
                    if (logCameraChanges)
                        Debug.Log(
                            $"No priorities stored - setting default camera {defaultCamera.gameObject.name} priority to 10"
                        );

                    defaultCamera.Priority = 10;
                }
                else
                {
                    // If no default camera, try to find the highest priority camera in the scene
                    var cameras = FindObjectsOfType<CinemachineVirtualCameraBase>();
                    if (cameras.Length > 0)
                    {
                        // Enable the first camera we find with priority > 0
                        foreach (var cam in cameras)
                        {
                            if (cam.Priority > 0)
                            {
                                if (logCameraChanges)
                                    Debug.Log(
                                        $"No default camera - using {cam.gameObject.name} as default with priority {cam.Priority}"
                                    );
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No Cinemachine cameras found in the scene!");
                    }
                }
            }

            // Clear any current interaction camera reference
            currentInteractionCamera = null;
        }

        private void OnDisable()
        {
            // Ensure we restore all camera priorities when script is disabled
            if (originalCameraPriorities.Count > 0)
            {
                RestoreAllCameraPriorities();
            }
            currentInteractionCamera = null;
        }

        private void OnEnable()
        {
            // Check if we need to initialize camera priorities on enable
            var cameras = FindObjectsOfType<CinemachineVirtualCameraBase>();
            bool anyActiveCameras = false;

            foreach (var cam in cameras)
            {
                if (cam.Priority > 0)
                {
                    anyActiveCameras = true;
                    break;
                }
            }

            // If no active cameras and we have a default, enable it
            if (!anyActiveCameras && defaultCamera != null)
            {
                if (logCameraChanges)
                    Debug.Log(
                        $"No active cameras found - enabling default camera with priority 10"
                    );

                defaultCamera.Priority = 10;
            }
        }

        private void OnDrawGizmos()
        {
            if (interactionSource != null)
            {
                // Draw the interaction radius as a yellow wire sphere
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(interactionSource.position, interactionRadius);

                // Draw the interaction range as a red line
                Vector3 start = interactionSource.position;
                Vector3 end = start + interactionSource.forward * interactionRange;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
