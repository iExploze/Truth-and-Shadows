using TruthAndShadows.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TruthAndShadows.InputSystem
{
    /// <summary>
    /// Centralized service that provides context about what inputs are allowed in different gameplay situations.
    /// Acts as a bridge between PlayerController state and other systems that need to check input permissions.
    /// </summary>
    public class InputContextProvider : MonoBehaviour
    {
        private static InputContextProvider _instance;
        public static InputContextProvider Instance => _instance;

        [SerializeField]
        private bool logInputPermissionChanges = true;

        // Current player state
        private PlayerState _currentPlayerState = PlayerState.Normal;

        // Cached input permissions
        private bool _canMove = true;
        private bool _canInteract = true;
        private bool _canPickup = true;
        private bool _canRotate = true;
        private bool _canRun = true;
        private bool _canCameraLook = true;
        private bool _canReset = true;
        private bool _canHint = true;
        private bool _canMenu = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            // Removed DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Optionally reset context or re-initialize if needed
            // (No scene-dependent references in current code)
        }

        #region Public API - Permissions

        /// <summary>
        /// Whether player movement is currently allowed
        /// </summary>
        public bool CanMove => _canMove;

        /// <summary>
        /// Whether player camera look is currently allowed
        /// </summary>
        public bool CanCameraLook => _canCameraLook;

        /// <summary>
        /// Whether interactions are currently allowed
        /// </summary>
        public bool CanInteract => _canInteract;

        /// <summary>
        /// Whether pickups are currently allowed
        /// </summary>
        public bool CanPickup => _canPickup;

        /// <summary>
        /// Whether spotlight rotation is currently allowed
        /// </summary>
        public bool CanRotate => _canRotate;

        /// <summary>
        /// Whether running is currently allowed
        /// </summary>
        public bool CanRun => _canRun;

        /// <summary>
        /// Whether reset action is currently allowed
        /// </summary>
        public bool CanReset => _canReset;

        /// <summary>
        /// Whether hint action is currently allowed
        /// </summary>
        public bool CanHint => _canHint;
        
        /// <summary>
        /// Whether menu action is currently allowed
        /// </summary>
        public bool CanMenu => _canMenu;

        /// <summary>
        /// Current player state
        /// </summary>
        public PlayerState CurrentPlayerState => _currentPlayerState;

        #endregion

        #region Public API - State Management

        /// <summary>
        /// Updates the current player state and recalculates input permissions
        /// </summary>
        /// <param name="newState">The new player state</param>
        public void UpdatePlayerState(PlayerState newState)
        {
            if (_currentPlayerState == newState)
                return;

            PlayerState oldState = _currentPlayerState;
            _currentPlayerState = newState;

            // Update permissions based on new state
            UpdatePermissions();

            if (logInputPermissionChanges)
            {
                Debug.Log($"[InputContextProvider] Player state changed: {oldState} -> {newState}");
                LogPermissions();
            }
        }

        /// <summary>
        /// Forces a specific input permission regardless of state
        /// </summary>
        public void ForcePermission(string permissionName, bool allowed)
        {
            switch (permissionName.ToLower())
            {
                case "move":
                    _canMove = allowed;
                    break;
                case "cameralook":
                    _canCameraLook = allowed;
                    break;
                case "interact":
                    _canInteract = allowed;
                    break;
                case "pickup":
                    _canPickup = allowed;
                    break;
                case "rotate":
                    _canRotate = allowed;
                    break;
                case "run":
                    _canRun = allowed;
                    break;
                case "reset":
                    _canReset = allowed;
                    break;
                case "hint":
                    _canHint = allowed;
                    break;
                case "menu":
                    _canMenu = allowed;
                    break;
                default:
                    Debug.LogWarning($"Unknown permission name: {permissionName}");
                    break;
            }

            if (logInputPermissionChanges)
            {
                Debug.Log(
                    $"[InputContextProvider] Forced permission '{permissionName}' to {allowed}"
                );
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Updates input permissions based on the current player state
        /// Uses the centralized InputPermissionsProvider to get permissions
        /// </summary>
        private void UpdatePermissions()
        {
            // Get permissions from the centralized provider
            var permissions = InputPermissionsProvider.GetPermissionsForState(_currentPlayerState);

            // Update local permission cache
            _canMove = permissions.CanMove;
            _canCameraLook = permissions.CanCameraLook;
            _canInteract = permissions.CanInteract;
            _canPickup = permissions.CanPickup;
            _canRotate = permissions.CanRotate;
            _canRun = permissions.CanRun;
            _canReset = permissions.CanReset;
            _canHint = permissions.CanHint;
            _canMenu = permissions.CanMenu;
        }

        public void LogPermissions()
        {
            Debug.Log(
                $"[InputContextProvider] Permissions for state {_currentPlayerState}:"
                    + $"\n Movement: {_canMove}"
                    + $"\n Camera: {_canCameraLook}"
                    + $"\n Interact: {_canInteract}"
                    + $"\n Pickup: {_canPickup}"
                    + $"\n Rotate: {_canRotate}"
                    + $"\n Run: {_canRun}"
                    + $"\n Reset: {_canReset}"
                    + $"\n Hint: {_canHint}"
                    + $"\n Menu: {_canMenu}"
            );
        }
        #endregion
    }
}
