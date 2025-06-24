using TruthAndShadows.Player;
using UnityEngine;

namespace TruthAndShadows.InputSystem
{
    /// <summary>
    /// Centralized provider for input permissions based on player state.
    /// This class eliminates duplication of permission logic between PlayerController and InputContextProvider.
    /// </summary>
    public static class InputPermissionsProvider
    {
        /// <summary>
        /// Defines the full set of input permissions for a given state
        /// </summary>
        public class Permissions
        {
            public bool CanMove { get; set; }
            public bool CanCameraLook { get; set; }
            public bool CanInteract { get; set; }
            public bool CanPickup { get; set; }
            public bool CanRotate { get; set; }
            public bool CanRun { get; set; }
            public bool CanHint { get; set; }
            public bool CanReset { get; set; }
        }

        /// <summary>
        /// Get the set of permissions for a specific player state
        /// </summary>
        /// <param name="state">The player state to get permissions for</param>
        /// <returns>A permissions object with all permission flags set appropriately</returns>
        public static Permissions GetPermissionsForState(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Normal:
                    return new Permissions
                    {
                        CanMove = true,
                        CanCameraLook = true,
                        CanInteract = true,
                        CanPickup = true,
                        CanRotate = true,
                        CanRun = true,
                        CanHint = true,
                        CanReset = true,
                    };

                case PlayerState.Aiming:
                    return new Permissions
                    {
                        CanMove = false,
                        CanCameraLook = true,
                        CanInteract = false,
                        CanPickup = false,
                        CanRotate = true,
                        CanRun = false,
                        CanHint = true,
                        CanReset = true,
                    };

                case PlayerState.Pickup:
                    return new Permissions
                    {
                        CanMove = true,
                        CanCameraLook = true,
                        CanInteract = false,
                        CanPickup = true,
                        CanRotate = false,
                        CanRun = false,
                        CanHint = true,
                        CanReset = true,
                    };

                case PlayerState.Interacting:
                    return new Permissions
                    {
                        CanMove = true,
                        CanCameraLook = true,
                        CanInteract = true,
                        CanPickup = false,
                        CanRotate = false,
                        CanRun = true,
                        CanHint = true,
                        CanReset = true,
                    };

                case PlayerState.InUI:
                    return new Permissions
                    {
                        CanMove = false,
                        CanCameraLook = false,
                        CanInteract = false,
                        CanPickup = false,
                        CanRotate = false,
                        CanRun = false,
                        CanHint = false,
                        CanReset = false,
                    };

                case PlayerState.Cutscene:
                    return new Permissions
                    {
                        CanMove = false,
                        CanCameraLook = false,
                        CanInteract = false,
                        CanPickup = false,
                        CanRotate = false,
                        CanRun = false,
                        CanHint = false,
                        CanReset = false,
                    };

                case PlayerState.Disabled:
                default:
                    return new Permissions
                    {
                        CanMove = false,
                        CanCameraLook = false,
                        CanInteract = false,
                        CanPickup = false,
                        CanRotate = false,
                        CanRun = false,
                        CanHint = false,
                        CanReset = true,
                    };
            }
        }
    }
}
