using UnityEngine;
using UnityEngine.Events;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// Defines the data passed to interaction events
    /// </summary>
    [System.Serializable]
    public class InteractionEventData
    {
        /// <summary>
        /// The interactable object involved in the interaction
        /// </summary>
        public GameObject interactableObject;
        
        /// <summary>
        /// The player or entity that initiated the interaction
        /// </summary>
        public GameObject interactor;
        
        /// <summary>
        /// Constructor for creating interaction event data
        /// </summary>
        /// <param name="interactable">The interactable object</param>
        /// <param name="interactor">The interacting entity (usually the player)</param>
        public InteractionEventData(GameObject interactable, GameObject interactor)
        {
            this.interactableObject = interactable;
            this.interactor = interactor;
        }
    }

    /// <summary>
    /// Custom UnityEvent type that passes interaction event data
    /// </summary>
    [System.Serializable]
    public class InteractionEvent : UnityEvent<InteractionEventData> { }
    
    /// <summary>
    /// Component that can be attached to interactable objects to expose Unity inspector events
    /// for interaction states. This allows designers to hook up visual and audio feedback
    /// without modifying the core interaction logic.
    /// </summary>
    public class InteractableEvents : MonoBehaviour
    {
        /// <summary>
        /// Event triggered when an interaction with this object begins
        /// </summary>
        [Header("Interaction Events")]
        public InteractionEvent onInteractionStarted = new InteractionEvent();
        
        /// <summary>
        /// Event triggered when an interaction with this object is ongoing
        /// </summary>
        public InteractionEvent onInteractionContinued = new InteractionEvent();
        
        /// <summary>
        /// Event triggered when an interaction with this object ends
        /// </summary>
        public InteractionEvent onInteractionEnded = new InteractionEvent();
        
        /// <summary>
        /// Event triggered when an interaction is attempted but conditions are not met
        /// </summary>
        public InteractionEvent onInteractionFailed = new InteractionEvent();
        
        /// <summary>
        /// Event triggered when this object becomes highlighted or focused
        /// </summary>
        [Header("Focus Events")]
        public InteractionEvent onFocused = new InteractionEvent();
        
        /// <summary>
        /// Event triggered when this object loses highlight or focus
        /// </summary>
        public InteractionEvent onUnfocused = new InteractionEvent();
        
        /// <summary>
        /// Event triggered when this object is picked up
        /// </summary>
        [Header("Pickup Events")]
        public InteractionEvent onPickupStarted = new InteractionEvent();
        
        /// <summary>
        /// Event triggered when this object is dropped or put down
        /// </summary>
        public InteractionEvent onPickupEnded = new InteractionEvent();
        
        private IInteractable _interactable;
        
        private void Awake()
        {
            // Get the IInteractable implementation (could be on this or a parent GameObject)
            _interactable = GetComponent<IInteractable>() ?? GetComponentInParent<IInteractable>();
            
            if (_interactable == null)
            {
                Debug.LogWarning($"InteractableEvents component on {gameObject.name} couldn't find an IInteractable implementation.");
                return;
            }
            
            // Subscribe to the IInteractable events
            _interactable.OnInteractionStarted += HandleInteractionStarted;
            _interactable.OnInteractionEnded += HandleInteractionEnded;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (_interactable != null)
            {
                _interactable.OnInteractionStarted -= HandleInteractionStarted;
                _interactable.OnInteractionEnded -= HandleInteractionEnded;
            }
        }
        
        /// <summary>
        /// Manually invoke the focus event (typically called by the interaction system)
        /// </summary>
        /// <param name="interactor">The entity that is focusing on this object</param>
        public void InvokeFocused(GameObject interactor)
        {
            var eventData = new InteractionEventData(gameObject, interactor);
            onFocused.Invoke(eventData);
        }
        
        /// <summary>
        /// Manually invoke the unfocus event (typically called by the interaction system)
        /// </summary>
        /// <param name="interactor">The entity that was focusing on this object</param>
        public void InvokeUnfocused(GameObject interactor)
        {
            var eventData = new InteractionEventData(gameObject, interactor);
            onUnfocused.Invoke(eventData);
        }
        
        /// <summary>
        /// Manually invoke the interaction failed event (typically called by the interaction system)
        /// </summary>
        /// <param name="interactor">The entity that attempted the interaction</param>
        public void InvokeInteractionFailed(GameObject interactor)
        {
            var eventData = new InteractionEventData(gameObject, interactor);
            onInteractionFailed.Invoke(eventData);
        }
        
        /// <summary>
        /// Manually invoke the continued interaction event (typically called during interaction)
        /// </summary>
        /// <param name="interactor">The entity that is interacting with this object</param>
        public void InvokeContinuedInteraction(GameObject interactor)
        {
            var eventData = new InteractionEventData(gameObject, interactor);
            onInteractionContinued.Invoke(eventData);
        }
        
        /// <summary>
        /// Handle the interaction started event from the IInteractable
        /// </summary>
        private void HandleInteractionStarted(GameObject interactor)
        {
            var eventData = new InteractionEventData(gameObject, interactor);
            onInteractionStarted.Invoke(eventData);
        }
        
        /// <summary>
        /// Handle the interaction ended event from the IInteractable
        /// </summary>
        private void HandleInteractionEnded(GameObject interactor)
        {
            var eventData = new InteractionEventData(gameObject, interactor);
            onInteractionEnded.Invoke(eventData);
        }
        
        /// <summary>
        /// Manually invoke the pickup started event (typically called by the interaction system)
        /// </summary>
        /// <param name="interactor">The entity that picked up this object</param>
        public void InvokePickupStarted(GameObject interactor)
        {
            var eventData = new InteractionEventData(gameObject, interactor);
            onPickupStarted.Invoke(eventData);
        }
        
        /// <summary>
        /// Manually invoke the pickup ended event (typically called by the interaction system)
        /// </summary>
        /// <param name="interactor">The entity that dropped this object</param>
        public void InvokePickupEnded(GameObject interactor)
        {
            var eventData = new InteractionEventData(gameObject, interactor);
            onPickupEnded.Invoke(eventData);
        }
    }
}
