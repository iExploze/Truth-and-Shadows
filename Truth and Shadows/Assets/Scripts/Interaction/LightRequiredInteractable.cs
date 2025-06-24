using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// Example of an interactable that requires the player to have their spotlight active.
    /// This could be used for objects that are only visible or usable when illuminated.
    /// </summary>
    public class LightRequiredInteractable : InteractableBase
    {
        [Header("Light Requirements")]
        [SerializeField]
        [Tooltip("If true, player must have spotlight active to interact")]
        private bool requireSpotlightActive = true;
        
        [SerializeField]
        [Tooltip("Message to show when spotlight is not active")]
        private string noSpotlightMessage = "You need to use your spotlight to interact with this";
        
        /// <summary>
        /// Tag of the player's spotlight object
        /// </summary>
        [SerializeField]
        private string spotlightTag = "PlayerSpotlight";
        
        /// <summary>
        /// Override the base CanInteract method to check if player has the spotlight active
        /// </summary>
        public override bool CanInteract(MonoBehaviour player)
        {
            // First check the base conditions
            if (!base.CanInteract(player))
                return false;
                
            // If we don't require spotlight, always allow
            if (!requireSpotlightActive)
                return true;
                
            // Try to find the player's spotlight
            bool isSpotlightActive = false;
            
            try
            {
                // First try to find spotlight as a child of the player
                Light[] playerLights = player.GetComponentsInChildren<Light>();
                foreach (Light light in playerLights)
                {
                    if (light.type == LightType.Spot && light.isActiveAndEnabled)
                    {
                        isSpotlightActive = true;
                        break;
                    }
                }
                
                // If not found as child, try to find by tag in scene
                if (!isSpotlightActive)
                {
                    GameObject spotlightObj = GameObject.FindGameObjectWithTag(spotlightTag);
                    if (spotlightObj != null)
                    {
                        Light spotLight = spotlightObj.GetComponent<Light>();
                        if (spotLight != null && spotLight.isActiveAndEnabled)
                        {
                            isSpotlightActive = true;
                        }
                    }
                }
                
                // Show a message if the spotlight is not active
                if (!isSpotlightActive)
                {
                    ShowPlayerMessage(noSpotlightMessage);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking player spotlight in LightRequiredInteractable: {e.Message}");
            }
            
            return isSpotlightActive;
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
            Debug.Log("Starting interaction with light-required object");
            // Implement the actual interaction behavior here
        }
    }
}
