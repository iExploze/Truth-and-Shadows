namespace TruthAndShadows.Player
{
    /// <summary>
    /// Represents the various gameplay states the player can be in
    /// </summary>
    public enum PlayerState
    {
        Normal, // Regular movement and camera control
        Aiming, // Aiming the spotlight
        Pickup, // Picking up/manipulating objects
        Interacting, // Interacting with objects/NPCs
        InUI, // In a menu or UI element
        Cutscene, // In a cutscene (no control)
        Disabled, // Controls disabled for any reason
    }
}
