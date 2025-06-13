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

        [SerializeField]
        protected float interactionDistance = 2f;
        
        [SerializeField]
        protected bool useColliderBounds = true;

        [Header("Pickup Settings")]
        [SerializeField]
        protected bool canBePickedUp = true;

        [SerializeField]
        protected float pickupRaiseAmount = 0.2f;

        [SerializeField]
        protected float pickupSmoothness = 10f;

        //For Object interactable Sound
        //Rashai was here
        public AudioSource source;
        public AudioClip clip;

        [Header("Camera Settings")]
        [SerializeField]
        protected CinemachineVirtualCamera interactionCamera;
        private bool isPickedUp = false;
        private Transform playerTransform;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Transform originalParent;
        private Rigidbody rigidBody;
        private Vector3 relativePosition;
        private bool hasCalculatedRelativePosition = false;

        public virtual bool RequiresContinuousInteraction => requireContinuousHold;
        public virtual CinemachineVirtualCamera InteractionCamera => interactionCamera;
        public virtual bool CanBePickedUp => canBePickedUp;
        public virtual bool IsPickedUp => isPickedUp;

        protected virtual void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
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

            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalParent = transform.parent;
            //Rashai was here
            source.PlayOneShot(clip);
           
            if (rigidBody != null)
            {
                rigidBody.isKinematic = true;
                rigidBody.useGravity = false;
            }
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

            transform.SetParent(originalParent);

            if (rigidBody != null)
            {
                rigidBody.isKinematic = false;
                rigidBody.useGravity = true;
            }

            playerTransform = null;

            Debug.Log($"Dropped: {gameObject.name}");
        }

        protected virtual void Update()
        {
            // Update pickup position
            if (isPickedUp && playerTransform != null)
            {
                if (!hasCalculatedRelativePosition)
                {
                    relativePosition = transform.position - playerTransform.position;
                    hasCalculatedRelativePosition = true;
                }

                Vector3 targetPosition = playerTransform.position + relativePosition;

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    Time.deltaTime * pickupSmoothness
                );
            }
        }

        /// <summary>
        /// Check if player can interact with this object - uses collider bounds for better detection
        /// </summary>
        public virtual bool CanInteract(Vector3 playerPosition)
        {
            float centerDistance = Vector3.Distance(transform.position, playerPosition);
            
            if (useColliderBounds)
            {
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    Vector3 closestPoint = col.ClosestPoint(playerPosition);
                    float boundsDistance = Vector3.Distance(playerPosition, closestPoint);
                    
                    float finalDistance = Mathf.Min(centerDistance, boundsDistance);
                    return finalDistance <= interactionDistance;
                }
            }
            return centerDistance <= interactionDistance; 
        }

        /// <summary>
        /// Check if player can pickup this object - same as interaction by default
        /// </summary>
        public virtual bool CanPickup(Vector3 playerPosition)
        {
            return CanInteract(playerPosition);
        }
    }
}
