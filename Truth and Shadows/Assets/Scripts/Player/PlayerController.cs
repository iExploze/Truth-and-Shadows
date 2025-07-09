using TruthAndShadows.InputSystem;
using UnityEngine;

namespace TruthAndShadows.Player
{
    /// <summary>
    /// Central player controller that interprets processed input from InputManager
    /// and decides how to respond based on current gameplay state.
    /// This controller acts as the intermediary between raw input and actual player behaviors.
    /// </summary>    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        #region Input Permission System
        /// <summary>
        /// Wrapper for mapping centralized permissions to the local format
        /// </summary>
        [System.Serializable]
        private class InputPermissions
        {
            public bool AllowMovement;
            public bool AllowCameraLook;
            public bool AllowInteract;
            public bool AllowPickup;
            public bool AllowRotate;
            public bool AllowRun;
            public bool AllowHint;
            public bool AllowReset;

            /// <summary>
            /// Returns input permissions for the given player state using the centralized provider
            /// </summary>
            public static InputPermissions GetPermissionsForState(PlayerState state)
            {
                // Get permissions from centralized provider
                var centralPermissions = InputPermissionsProvider.GetPermissionsForState(state);

                // Convert to local format
                var result = new InputPermissions();
                result.AllowMovement = centralPermissions.CanMove;
                result.AllowCameraLook = centralPermissions.CanCameraLook;
                result.AllowInteract = centralPermissions.CanInteract;
                result.AllowPickup = centralPermissions.CanPickup;
                result.AllowRotate = centralPermissions.CanRotate;
                result.AllowRun = centralPermissions.CanRun;
                result.AllowHint = centralPermissions.CanHint;
                result.AllowReset = centralPermissions.CanReset;

                return result;
            }
        }
        #endregion

        #region Inspector Variables
        [Header("Movement Settings")]
        [SerializeField]
        private float walkSpeed = 5f;

        [SerializeField]
        private float sprintSpeed = 10f;

        [SerializeField]
        private float rotationSpeed = 10f;

        [Header("References")]
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AudioSource walkAudioSource;

        [SerializeField]
        private Transform cameraTransform;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugInfo = false;
        #endregion

        #region Private Variables
        private PlayerState _currentState = PlayerState.Normal;
        private Vector2 _smoothedMovement = Vector2.zero;
        private float _currentSpeed = 0f;
        private float _movementSmoothTime = 0.1f;
        private Vector2 _moveVelocity;
        private float _speedVelocity;
        private InputPermissions _currentPermissions;
        #endregion

        #region Public Properties
        /// <summary>
        /// Current state of the player
        /// </summary>
        public PlayerState CurrentState => _currentState;

        /// <summary>
        /// Whether the player can currently move
        /// </summary>
        public bool CanMove { get; set; } = true;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {

            // Use the provided animator or try to get one
            if (animator == null)
                animator = GetComponent<Animator>();

            // Set up camera reference if not assigned
            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;

            // Initialize permissions for the starting state
            _currentPermissions = InputPermissions.GetPermissionsForState(_currentState);

            // Initialize the centralized input context with our starting state
            OnStateChanged(_currentState, _currentState);
        }

        private void Update()
        {
            // Don't process if InputManager isn't available
            if (InputManager.Instance == null)
                return;

            // Update player state based on input
            UpdatePlayerState();

            // Process movement based on current state
            ProcessMovement();

            // Process interactions based on current state
            ProcessInteractions();

            // Debug logging
            if (showDebugInfo && Time.frameCount % 120 == 0)
            {
                Debug.Log(
                    $"[PlayerController] State: {_currentState}, Movement: {_smoothedMovement}, Speed: {_currentSpeed}"
                );
            }
        }
        #endregion

        #region State Management
        /// <summary>
        /// Updates the player's state based on input and context.
        /// Implements exclusivity logic where certain states take priority over others.
        /// </summary>
        private void UpdatePlayerState()
        {
            // Remember previous state
            PlayerState previousState = _currentState;

            // Default to normal state
            _currentState = PlayerState.Normal;

            // Priority order for states (highest to lowest):
            // 1. UI (when pointer is over UI element)
            // 2. Cutscene (controlled separately)
            // 3. Aiming (Rotate)
            // 4. Pickup
            // 5. Interacting
            // 6. Normal

            // Check for UI state - highest priority
            if (_currentState == PlayerState.Cutscene || _currentState == PlayerState.Disabled)
            {
                // Keep the current state if it's a cutscene or disabled
                // These states would be set from outside this method
            }
            // Check for aiming state - high priority
            else if (InputManager.Instance.RotateHeld)
            {
                _currentState = PlayerState.Aiming;
            }
            // Check for pickup state - medium priority
            else if (InputManager.Instance.PickupHeld)
            {
                _currentState = PlayerState.Pickup;
            }
            // Check for interaction state - lower priority
            else if (InputManager.Instance.InteractHeld)
            {
                _currentState = PlayerState.Interacting;
            }

            // If state changed, trigger any necessary transitions
            if (previousState != _currentState)
            {
                OnStateChanged(previousState, _currentState);
            }

            Debug.Log("Previous State: " + previousState + ", New State: " + _currentState);
        }

        /// <summary>
        /// Called when the player state changes
        /// </summary>
        private void OnStateChanged(PlayerState previousState, PlayerState newState)
        {
            // Update input permissions based on the new state
            _currentPermissions = InputPermissions.GetPermissionsForState(newState);
            // Update the centralized InputContextProvider if available
            InputContextProvider inputContextProvider = FindObjectOfType<InputContextProvider>();
            if (inputContextProvider != null)
            {
                // Update the centralized input context with our state
                inputContextProvider.UpdatePlayerState(newState);
            }

            // Handle state transition effects
            if (showDebugInfo)
            {
                Debug.Log($"[PlayerController] State changed: {previousState} -> {newState}");
                LogPermissionsChange(newState);
            }

            // Specific state transition behaviors
            switch (newState)
            {
                case PlayerState.Aiming:
                    // Setup for aiming mode
                    break;

                case PlayerState.Pickup:
                    // Setup for pickup mode
                    break;

                case PlayerState.Normal:
                    // Reset any state-specific variables
                    break;
            }
        }

        /// <summary>
        /// Logs changes to permissions for debugging purposes
        /// </summary>
        private void LogPermissionsChange(PlayerState state)
        {
            if (!showDebugInfo)
                return;

            Debug.Log(
                $"[PlayerController] Input permissions for state {state}:"
                    + $"\n Movement: {_currentPermissions.AllowMovement}"
                    + $"\n Camera: {_currentPermissions.AllowCameraLook}"
                    + $"\n Interact: {_currentPermissions.AllowInteract}"
                    + $"\n Pickup: {_currentPermissions.AllowPickup}"
                    + $"\n Rotate: {_currentPermissions.AllowRotate}"
                    + $"\n Run: {_currentPermissions.AllowRun}"
                    + $"\n Hint: {_currentPermissions.AllowHint}"
                    + $"\n Reset: {_currentPermissions.AllowReset}"
            );
        }
        #endregion

        #region Movement Processing        
        /// <summary>
        /// Process movement based on current state and input
        /// </summary>
        private void ProcessMovement()
        {
            if (!CanMove)
                return;

            // The InputManager already respects permissions from InputContextProvider
            // So we can just use the input directly
            Vector2 targetMovement = InputManager.Instance.CharacterMoveInput;

            // No need for state checks here as the InputContextProvider
            // and InputManager already handle this for us

            // Smoothly interpolate to the target movement
            _smoothedMovement = Vector2.SmoothDamp(
                _smoothedMovement,
                targetMovement,
                ref _moveVelocity,
                _movementSmoothTime
            );

            // Calculate move speed
            float targetSpeed = InputManager.Instance.IsRunning ? sprintSpeed : walkSpeed;
            _currentSpeed = Mathf.SmoothDamp(
                _currentSpeed,
                targetSpeed * _smoothedMovement.magnitude,
                ref _speedVelocity,
                _movementSmoothTime
            );

            // Calculate move direction in world space
            Vector3 moveDirection = Vector3.zero;

            if (_smoothedMovement.sqrMagnitude > 0.01f)
            {
                // Calculate movement direction relative to camera
                Vector3 cameraForward = cameraTransform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();

                Vector3 cameraRight = cameraTransform.right;
                cameraRight.y = 0f;
                cameraRight.Normalize();

                moveDirection = (
                    cameraForward * _smoothedMovement.y + cameraRight * _smoothedMovement.x
                ).normalized;

                // Rotate player to face movement direction
                if (_currentState != PlayerState.Aiming)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * rotationSpeed
                    );
                }
            }

            // Update animator parameters
            if (animator != null)
            {
                animator.SetFloat("Speed", _smoothedMovement.magnitude);

                // Set direction parameter if your animator uses it
                float direction = 0f;
                if (_smoothedMovement.y < 0f)
                    direction = _smoothedMovement.y;
                animator.SetFloat("Direction", direction);
            }

            // Handle footstep audio
            if (walkAudioSource != null)
            {
                if (_currentSpeed > 0.1f && !walkAudioSource.isPlaying)
                {
                    walkAudioSource.loop = true;
                    walkAudioSource.Play();
                }
                else if (_currentSpeed <= 0.1f && walkAudioSource.isPlaying)
                {
                    walkAudioSource.Stop();
                }
            }
        }
        #endregion

        #region Interaction Processing        /// <summary>
        /// Process interaction inputs based on current state
        /// Note: Most actual interaction handling is done by InteractionManager
        /// </summary>
        private void ProcessInteractions()
        {
            // Note: Interaction button presses are processed by InteractionManager
            // This method primarily handles auxiliary inputs and debug logging

            // Log button presses for debugging purposes
            if (showDebugInfo)
            {
                if (InputManager.Instance.InteractPressed)
                {
                    Debug.Log(
                        $"[PlayerController] Interact pressed - Current state: {_currentState}"
                    );
                }

                if (InputManager.Instance.PickupPressed)
                {
                    Debug.Log(
                        $"[PlayerController] Pickup pressed - Current state: {_currentState}"
                    );
                }

                if (InputManager.Instance.RotateHeld)
                {
                    Debug.Log($"[PlayerController] Rotate held - Current state: {_currentState}");
                }

                if (InputManager.Instance.ResetPressed)
                {
                    Debug.Log($"[PlayerController] Reset pressed - Current state: {_currentState}");
                }

                if (InputManager.Instance.HintPressed)
                {
                    Debug.Log($"[PlayerController] Hint pressed - Current state: {_currentState}");
                }
            }

            // Handle hint button functionality - this is handled here instead of InteractionManager
            if (InputManager.Instance.HintPressed)
            {
                // Hint system logic goes here - could trigger UI hints, waypoints, etc.
                // This might call into another system like HintManager.ShowHint()
            }
        }
        #endregion
    }
}
