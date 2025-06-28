using TruthAndShadows.InputSystem;
using UnityEngine;

namespace TruthAndShadows.Player
{
    /// <summary>
    /// Handles player movement, animation, state, and input permissions.
    /// Integrates state management and input permission logic from PlayerController.
    /// </summary>
    [AddComponentMenu("")] // Don't display in add component menu
    public class CharacterMovement : MonoBehaviour
    {
        // --- Movement Settings ---
        public bool useCharacterForward = false;
        public bool lockToCameraForward = false;
        public float turnSpeed = 10f;

        [SerializeField]
        private float walkSpeed = 5f;

        [SerializeField]
        private float sprintSpeed = 10f;

        [SerializeField]
        private float rotationSpeed = 10f;

        // --- State & Permissions ---
        private PlayerState _currentState = PlayerState.Normal;
        private InputPermissions _currentPermissions;
        private float _movementSmoothTime = 0.1f;
        private Vector2 _moveVelocity;
        private float _speedVelocity;

        // --- Animation & Audio ---
        private Animator anim;

        [SerializeField]
        private AudioSource walkAudioSource;

        // --- Camera ---
        private Camera mainCamera;

        [SerializeField]
        private Transform cameraTransform;

        // --- Movement ---
        private float turnSpeedMultiplier;
        private float speed = 0f;
        private float direction = 0f;
        private Vector3 targetDirection;
        private Vector2 input;
        private Quaternion freeRotation;
        private float velocity;
        public bool canMove = true;

        // --- Debug ---
        [SerializeField]
        private bool showDebugInfo = false;

        // --- Input Permissions Helper ---
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

            public static InputPermissions GetPermissionsForState(PlayerState state)
            {
                var centralPermissions = InputPermissionsProvider.GetPermissionsForState(state);
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

        // --- Unity Lifecycle ---
        void Start()
        {
            anim = GetComponent<Animator>();
            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;
            mainCamera = Camera.main;
            _currentPermissions = InputPermissions.GetPermissionsForState(_currentState);
            OnStateChanged(_currentState, _currentState);
        }

        void Update()
        {
            if (InputManager.Instance == null)
                return;
            UpdatePlayerState();
            // Movement and animation in FixedUpdate for physics consistency
        }

        void FixedUpdate()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (InputManager.Instance != null)
            {
                input = InputManager.Instance.MoveInput;
                if (!InputContextProvider.Instance.CanMove || !_currentPermissions.AllowMovement)
                {
                    input = Vector2.zero;
                    if (showDebugInfo)
                    {
                        Debug.Log("Movement blocked by InputContextProvider or permissions");
                        InputContextProvider.Instance.LogPermissions();
                    }
                }
                if (InputManager.Instance.PickupHeld && input.magnitude < 0.1f)
                {
                    input = InputManager.Instance.MoveInputRaw;
                    if (input.magnitude > 0.1f && Time.frameCount % 120 == 0)
                        Debug.Log($"Using raw input during pickup: {input}");
                }
            }
            // Movement smoothing and speed
            Vector2 targetMovement = input;
            Vector2 _smoothedMovement = Vector2.SmoothDamp(
                Vector2.zero, // always smooth from zero for simplicity
                targetMovement,
                ref _moveVelocity,
                _movementSmoothTime
            );
            float targetSpeed =
                InputManager.Instance != null && InputManager.Instance.IsRunning
                    ? sprintSpeed
                    : walkSpeed;
            float _currentSpeed = Mathf.SmoothDamp(
                speed,
                targetSpeed * _smoothedMovement.magnitude,
                ref _speedVelocity,
                _movementSmoothTime
            );
            // set speed to both vertical and horizontal inputs
            if (useCharacterForward)
                speed = Mathf.Abs(input.x) + input.y;
            else
                speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
            speed = Mathf.Clamp(speed, 0f, 1f);
            speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref velocity, 0.1f);
            anim.SetFloat("Speed", speed);
            // Play walking sound if moving, stop if not
            if (walkAudioSource != null)
            {
                if (_currentSpeed > 0.05f && !walkAudioSource.isPlaying)
                {
                    walkAudioSource.loop = true;
                    walkAudioSource.Play();
                }
                else if (_currentSpeed <= 0.05f && walkAudioSource.isPlaying)
                {
                    walkAudioSource.Stop();
                }
            }
            if (input.y < 0f && useCharacterForward)
                direction = input.y;
            else
                direction = 0f;
            anim.SetFloat("Direction", direction);
            UpdateTargetDirection();
            if (!canMove)
                return;
            if (input != Vector2.zero && targetDirection.magnitude > 0.1f)
            {
                Vector3 lookDirection = targetDirection.normalized;
                freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
                var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
                var eulerY = transform.eulerAngles.y;
                if (diferenceRotation < 0 || diferenceRotation > 0)
                    eulerY = freeRotation.eulerAngles.y;
                var euler = new Vector3(0, eulerY, 0);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.Euler(euler),
                    turnSpeed * turnSpeedMultiplier * Time.deltaTime
                );
            }
            // Optionally, apply Rigidbody-based movement here if needed
#else
            InputSystemHelper.EnableBackendsWarningMessage();
#endif
        }

        private void UpdatePlayerState()
        {
            PlayerState previousState = _currentState;
            _currentState = PlayerState.Normal;
            if (UnityEngine.EventSystems.EventSystem.current?.IsPointerOverGameObject() ?? false)
            {
                _currentState = PlayerState.InUI;
            }
            else if (_currentState == PlayerState.Cutscene || _currentState == PlayerState.Disabled)
            {
                // Keep the current state if it's a cutscene or disabled
            }
            else if (InputManager.Instance.RotateHeld)
            {
                _currentState = PlayerState.Aiming;
            }
            else if (InputManager.Instance.PickupHeld)
            {
                _currentState = PlayerState.Pickup;
            }
            else if (InputManager.Instance.InteractHeld)
            {
                _currentState = PlayerState.Interacting;
            }
            if (previousState != _currentState)
            {
                OnStateChanged(previousState, _currentState);
            }
            if (showDebugInfo && Time.frameCount % 120 == 0)
            {
                Debug.Log(
                    $"[CharacterMovement] State: {_currentState}, Movement: {input}, Speed: {speed}"
                );
            }
        }

        private void OnStateChanged(PlayerState previousState, PlayerState newState)
        {
            _currentPermissions = InputPermissions.GetPermissionsForState(newState);
            InputContextProvider inputContextProvider = FindObjectOfType<InputContextProvider>();
            if (inputContextProvider != null)
            {
                inputContextProvider.UpdatePlayerState(newState);
            }
            if (showDebugInfo)
            {
                Debug.Log($"[CharacterMovement] State changed: {previousState} -> {newState}");
                LogPermissionsChange(newState);
            }
            // Add state-specific logic here if needed
        }

        private void LogPermissionsChange(PlayerState state)
        {
            if (!showDebugInfo)
                return;
            Debug.Log(
                $"[CharacterMovement] Input permissions for state {state}:"
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

        public virtual void UpdateTargetDirection()
        {
            if (!useCharacterForward)
            {
                turnSpeedMultiplier = 1f;
                var forward = mainCamera.transform.TransformDirection(Vector3.forward);
                forward.y = 0;
                var right = mainCamera.transform.TransformDirection(Vector3.right);
                targetDirection = input.x * right + input.y * forward;
            }
            else
            {
                turnSpeedMultiplier = 0.2f;
                var forward = transform.TransformDirection(Vector3.forward);
                forward.y = 0;
                var right = transform.TransformDirection(Vector3.right);
                targetDirection = input.x * right + Mathf.Abs(input.y) * forward;
            }
        }
    }
}
