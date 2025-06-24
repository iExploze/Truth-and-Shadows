using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// Example of an interactable that requires the player to be stationary to interact with it.
    /// This could be used for delicate objects, reading, or precision tasks.
    /// </summary>
    public class StationaryInteractable : InteractableBase
    {
        [Header("Stationary Requirements")]
        [SerializeField]
        [Tooltip("If true, player must be standing still to interact")]
        private bool requirePlayerStationary = true;
        
        [SerializeField]
        [Tooltip("Maximum allowed player movement speed to consider as 'stationary'")]
        private float maxAllowedMovementSpeed = 0.1f;
        
        [SerializeField]
        [Tooltip("Message to show when player is moving too fast")]
        private string movingTooFastMessage = "You need to stand still to interact with this";
        
        /// <summary>
        /// Override the base CanInteract method to check if the player is stationary
        /// </summary>
        public override bool CanInteract(MonoBehaviour player)
        {
            // First check the base conditions
            if (!base.CanInteract(player))
                return false;
                
            // If we don't require player to be stationary, always allow
            if (!requirePlayerStationary)
                return true;
                
            // Check if player is moving using reflection to access velocity property
            // This avoids direct reference to PlayerController to prevent circular dependencies
            bool isPlayerMovingSlow = true;
            
            try
            {
                // Try to get CharacterController component from the player
                CharacterController characterController = player.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    // Check the player's velocity magnitude
                    float playerVelocity = characterController.velocity.magnitude;
                    isPlayerMovingSlow = playerVelocity <= maxAllowedMovementSpeed;
                    
                    // Show a message if the player is moving too fast
                    if (!isPlayerMovingSlow)
                    {
                        ShowPlayerMessage(movingTooFastMessage);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking player velocity in StationaryInteractable: {e.Message}");
            }
            
            return isPlayerMovingSlow;
        }
        
        /// <summary>
        /// Display a message to the player
        /// </summary>
        private void ShowPlayerMessage(string message)
        {
            // Find a UI manager or prompt system to show the message
            // This is just a placeholder - implement according to your game's UI system
            Debug.Log($"[Player Message]: {message}");
            
            // Example: If you have a UI message system
            var messageSystem = FindObjectOfType<ProximityPromptDisplay>();
            if (messageSystem != null)
            {
                // Use reflection to invoke ShowMessage method if it exists
                System.Reflection.MethodInfo showMessageMethod = 
                    messageSystem.GetType().GetMethod("ShowCustomMessage", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (showMessageMethod != null)
                {
                    showMessageMethod.Invoke(messageSystem, new object[] { message, 2.0f });
                }
            }
        }
        
        public override void StartInteraction()
        {
            Debug.Log("Starting interaction with stationary object");
            // Implement the actual interaction behavior here
        }
    }
}
