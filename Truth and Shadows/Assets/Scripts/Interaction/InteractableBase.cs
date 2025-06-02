using Cinemachine;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// Base class for objects that can be interacted with. Provides common functionality
    /// and optional camera support.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField]
        protected bool requireContinuousHold = false;

        [Header("Pickup Settings")]
        [SerializeField]
        protected bool canBePickedUp = true;

        [SerializeField]
        protected float pickupRaiseAmount = 0.2f; // How much to raise the item when picked up

        [SerializeField]
        protected float pickupSmoothness = 10f; // How smoothly the item follows the player

        [Header("Camera Settings")]
        [SerializeField]
        protected CinemachineVirtualCamera interactionCamera; // Pickup state
        private bool isPickedUp = false;
        private Transform playerTransform;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Transform originalParent;
        private Rigidbody rigidBody;
        private Collider[] colliders;
        private Vector3 relativePosition; // Position relative to player when picked up
        private bool hasCalculatedRelativePosition = false;

        public virtual bool RequiresContinuousInteraction => requireContinuousHold;
        public virtual CinemachineVirtualCamera InteractionCamera => interactionCamera;
        public virtual bool CanBePickedUp => canBePickedUp;
        public virtual bool IsPickedUp => isPickedUp;

        protected virtual void Start()
        {
            // Cache components
            rigidBody = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
        }

        public abstract void StartInteraction();

        public virtual void ContinueInteraction() { }

        public virtual void EndInteraction() { }

        public virtual void StartPickup(Transform playerTransform)
        {
            if (!canBePickedUp || isPickedUp)
                return;

            this.playerTransform = playerTransform;
            isPickedUp = true;
            hasCalculatedRelativePosition = false;

            // Store original state
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalParent = transform.parent;

            // Disable physics
            if (rigidBody != null)
            {
                rigidBody.isKinematic = true;
                rigidBody.useGravity = false;
            }

            // Disable colliders to prevent interference
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // Don't attach to player, keep in world space but track relative position
            // First, raise the item slightly in place
            Vector3 raisedPosition = transform.position + Vector3.up * pickupRaiseAmount;
            transform.position = raisedPosition;

            Debug.Log($"Picked up: {gameObject.name}");
        }

        public virtual void EndPickup()
        {
            if (!isPickedUp)
                return;

            isPickedUp = false;
            hasCalculatedRelativePosition = false;

            // Restore parent
            transform.SetParent(originalParent);

            // Re-enable physics
            if (rigidBody != null)
            {
                rigidBody.isKinematic = false;
                rigidBody.useGravity = true;
            } // Re-enable colliders
            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            // Keep the item at its current position - it will drop straight down due to gravity
            // No need to teleport it anywhere

            playerTransform = null;

            Debug.Log($"Dropped: {gameObject.name}");
        }

        protected virtual void Update()
        {
            // Update pickup position
            if (isPickedUp && playerTransform != null)
            {
                // Calculate relative position on first frame after pickup
                if (!hasCalculatedRelativePosition)
                {
                    relativePosition = transform.position - playerTransform.position;
                    hasCalculatedRelativePosition = true;
                }

                // Maintain the same relative position to the player
                Vector3 targetPosition = playerTransform.position + relativePosition;

                // Smoothly move to target position
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    Time.deltaTime * pickupSmoothness
                );

                // Keep the original rotation (don't make it face the player direction)
                // This preserves the object's orientation when picked up
            }
        }
    }
}
