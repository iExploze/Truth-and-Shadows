using Cinemachine;
using UnityEngine;

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

        [Header("Camera")]
        [SerializeField]
        private CinemachineVirtualCamera defaultCamera;
        private CinemachineVirtualCamera currentInteractionCamera;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugRay = true;

        private IInteractable currentInteractable;
        private bool isInteracting;
        private IInteractable pickedUpInteractable;

        void Start()
        {
            InitializeSource();
            Cursor.visible = false;
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

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("R key pressed - attempting interaction");
                TryStartInteraction();
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                Debug.Log("R key released - ending interaction");
                EndCurrentInteraction();
            }

            // Handle pickup functionality with F key - hold to keep picked up
            HandlePickupInput();
        }

        private void UpdateContinuousInteraction()
        {
            if (isInteracting && currentInteractable?.RequiresContinuousInteraction == true)
                currentInteractable.ContinueInteraction();
        }

        private void TryStartInteraction()
        {
            Debug.Log("TryStartInteraction called");

            if (!IsValidSource())
            {
                Debug.LogWarning("Invalid interaction source!");
                return;
            }

            Vector3 origin = GetInteractionOrigin();
            Vector3 direction = interactionSource.forward;

            Debug.Log(
                $"Interaction ray: Origin={origin}, Direction={direction}, Range={interactionRange}"
            );

            if (showDebugRay)
                Debug.DrawRay(origin, direction * interactionRange, Color.yellow, 0.1f);

            if (TryFindInteractable(origin, direction, out IInteractable interactable))
            {
                Debug.Log($"Found interactable: {((MonoBehaviour)interactable).gameObject.name}");
                currentInteractable = interactable;
                isInteracting = true;
                currentInteractable.StartInteraction(); // Switch to interactable's camera if it has one
                if (currentInteractable.InteractionCamera != null)
                {
                    currentInteractionCamera = currentInteractable.InteractionCamera;
                    SwitchToCamera(currentInteractionCamera);
                }
            }
            else
            {
                Debug.Log("No interactable found in range");
            }
        }

        private void SwitchToCamera(CinemachineVirtualCamera camera)
        {
            // Increase priority of the target camera and decrease others
            if (defaultCamera != null)
                defaultCamera.Priority = 0;

            if (currentInteractionCamera != null && currentInteractionCamera != camera)
                currentInteractionCamera.Priority = 0;

            camera.Priority = 10;
        }

        private bool IsValidSource()
        {
            return interactionSource != null;
        }

        private Vector3 GetInteractionOrigin()
        {
            // Offset the origin slightly backward to ensure detection when close to objects
            Vector3 offset = interactionSource.position - (interactionSource.forward * interactionRadius);
            return offset + Vector3.up;
        }

        private bool TryFindInteractable(
            Vector3 origin,
            Vector3 direction,
            out IInteractable interactable
        )
        {
            interactable = null;
            RaycastHit hit;

            if (Physics.SphereCast(origin, interactionRadius, direction, out hit, interactionRange))
            {
                interactable = hit.collider.GetComponent<IInteractable>();
            }

            return interactable != null;
        }

        public void EndCurrentInteraction()
        {
            if (!isInteracting || currentInteractable == null)
                return;

            currentInteractable.EndInteraction(); // Switch back to default camera if we switched away from it
            if (currentInteractionCamera != null)
            {
                currentInteractionCamera.Priority = 0;
                if (defaultCamera != null)
                    defaultCamera.Priority = 10;
                currentInteractionCamera = null;
            }

            // Clear interaction state for all types of interactions
            currentInteractable = null;
            isInteracting = false;
        }

        public void PreserveInteraction()
        {
            if (currentInteractable?.RequiresContinuousInteraction == true)
                isInteracting = Input.GetKey(KeyCode.R);
        }

        private void HandlePickupInput()
        {
            if (Input.GetKeyDown(KeyCode.F) && pickedUpInteractable == null)
            {
                Debug.Log("F key pressed - attempting pickup");
                TryPickupItem();
            }
            else if (Input.GetKeyUp(KeyCode.F) && pickedUpInteractable != null)
            {
                Debug.Log("F key released - dropping item");
                DropPickedUpItem();
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

            if (TryFindInteractable(origin, direction, out IInteractable interactable) && interactable.CanBePickedUp && !interactable.IsPickedUp)
            {
                if (isInteracting && currentInteractable == interactable)
                {
                    EndCurrentInteraction();
                }

                pickedUpInteractable = interactable;
                interactable.StartPickup(interactionSource);

                if (defaultCamera != null)
                {
                    defaultCamera.Priority = 10;
                }

                if (currentInteractionCamera != null)
                {
                    currentInteractionCamera.Priority = 0;
                    currentInteractionCamera = null;
                }

                Debug.Log($"Picked up: {((MonoBehaviour)interactable).gameObject.name}");
            }
            else
            {
                Debug.Log(interactable == null ? "No interactable found for pickup" : $"Item cannot be picked up: {((MonoBehaviour)interactable).gameObject.name}");
            }
        }

        private void DropPickedUpItem()
        {
            if (pickedUpInteractable != null)
            {
                Debug.Log($"Dropping: {((MonoBehaviour)pickedUpInteractable).gameObject.name}");
                pickedUpInteractable.EndPickup();
                pickedUpInteractable = null;
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
