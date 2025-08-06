using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// A basic interactable that implements the minimum required functionality from InteractableBase.
    /// This serves as a simple template for creating new interactables.
    /// </summary>
    public class BasicInteractable : InteractableBase
    {
        [Header("Basic Interaction")]
        [SerializeField]
        [Tooltip("Message to log when this object is interacted with")]
        private string interactionMessage = "Basic interaction triggered!";

        [SerializeField]
        [Tooltip("Whether this object can be picked up and moved")]
        private bool allowPickup = false;

        protected override void Start()
        {
            base.Start();
            
            // Set pickup capability based on inspector setting
            // Note: canBePickedUp is a protected field in the base class
            // We can access it directly from derived classes
            canBePickedUp = allowPickup;
        }

        /// <summary>
        /// Called when the player interacts with this object
        /// </summary>
        public override void StartInteraction()
        {
            Debug.Log($"[BasicInteractable] {interactionMessage}");
            
            // Optional: Play interaction sound if one is assigned
            if (source != null && pickUpClip != null)
            {
                source.PlayOneShot(pickUpClip);
            }
        }

        /// <summary>
        /// Called every frame during continuous interaction (if RequiresContinuousInteraction is true)
        /// </summary>
        public override void ContinueInteraction()
        {
            // Basic implementation does nothing during continuous interaction
            // Override this in derived classes if needed
        }

        /// <summary>
        /// Called when the interaction ends
        /// </summary>
        public override void EndInteraction()
        {
            // Basic implementation does nothing when interaction ends
            // Override this in derived classes if needed
            Debug.Log($"[BasicInteractable] Interaction ended on {gameObject.name}");
        }
    }
}
