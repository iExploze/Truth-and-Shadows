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
        private float sprintSpeed = 4f;

        [SerializeField]
        private float rotationSpeed = 10f;

        // --- State & Permissions ---
        private PlayerState _currentState = PlayerState.Normal;
        private InputPermissions _currentPermissions;
        private float _movementSmoothTime = 0.1f;
        private Vector2 _moveVelocity;
        private float _speedVelocity;

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
        public bool canMove = true;

        // --- Sound ---
        [SerializeField]
        public AudioSource walkSource;
        public AudioClip[] walkClips;
        public AudioClip[] walkSandClips;
        private bool istouchingSand = false;
        private bool isWalkSoundPlaying = false;  // Track sound state
        private float lastSoundStartTime = 0f;     // Track when we last started playing
        private const float MIN_SOUND_INTERVAL = 0.4f; // Minimum time between sound triggers

        private Rigidbody rb;

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

        private CharacterAnimation characterAnimation;

        void Start()
        {
            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;
            mainCamera = Camera.main;
            _currentPermissions = InputPermissions.GetPermissionsForState(_currentState);
            OnStateChanged(_currentState, _currentState);

            rb = GetComponent<Rigidbody>();
            characterAnimation = rb.GetComponent<CharacterAnimation>();
            if (walkSource == null)
            {
                walkSource = GetComponent<AudioSource>();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Sand"))
            {
                istouchingSand = true;
            }
        }
        void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Sand"))
            {
                istouchingSand = false;
            }
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
                input = InputManager.Instance.CharacterMoveInput;
                // Do not set input = Vector2.zero here!
            }
            // Movement smoothing and speed
            Vector2 targetMovement = input;
            Vector2 _smoothedMovement = Vector2.SmoothDamp(
                Vector2.zero, // always smooth from zero for simplicity
                targetMovement,
                ref _moveVelocity,
                _movementSmoothTime
            );
            // set speed to both vertical and horizontal inputs
            if (useCharacterForward)
                speed = Mathf.Abs(input.x) + input.y;
            else
                speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
            speed = Mathf.Clamp(speed, 0f, 1f);
            if (input.y < 0f && useCharacterForward)
                direction = input.y;
            else
                direction = 0f;
            UpdateTargetDirection();
            // Only allow movement/rotation if allowed
            if (
                canMove
                && InputContextProvider.Instance.CanMove
                && _currentPermissions.AllowMovement
            )
            {
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
                //simple movement by Ian
                Vector3 forward = transform.forward * speed * sprintSpeed * Time.fixedDeltaTime;
                rb.MovePosition(rb.position + forward);
                characterAnimation.updateMovement(forward);

                // handle walk sound with better state management
                if (walkClips.Length > 0)
                {
                    bool shouldBePlaying = characterAnimation.IsMoving();
                    float timeSinceLastStart = Time.time - lastSoundStartTime;

                    if (shouldBePlaying && !isWalkSoundPlaying && timeSinceLastStart >= MIN_SOUND_INTERVAL)
                    {
                        int randomIndex = Random.Range(0, walkClips.Length);
                        walkSource.PlayOneShot(walkClips[randomIndex]);                        lastSoundStartTime = Time.time;
                        walkSource.pitch = Random.Range(1f, 1.05f);
                    }
                    else if (!shouldBePlaying && isWalkSoundPlaying)
                    {
                        walkSource.Stop();
                        isWalkSoundPlaying = false;
                    }
                }
                 if (walkSandClips.Length > 0)
                {
                    bool shouldBePlaying = characterAnimation.IsMoving();
                    float timeSinceLastStart = Time.time - lastSoundStartTime;

                    if (shouldBePlaying && !isWalkSoundPlaying && istouchingSand && timeSinceLastStart >= MIN_SOUND_INTERVAL)
                    {
                        int randomIndex = Random.Range(0, walkClips.Length);
                        walkSource.PlayOneShot(walkSandClips[randomIndex]);                        lastSoundStartTime = Time.time;
                        walkSource.pitch = Random.Range(1f, 1.05f);
                    }
                    else if (!shouldBePlaying && isWalkSoundPlaying)
                    {
                        walkSource.Stop();
                        isWalkSoundPlaying = false;
                    }
                }
            }
            else
            {
                // Block only player movement, not input
                characterAnimation.updateMovement(Vector3.zero);
                if (walkSource != null && isWalkSoundPlaying)
                {
                    walkSource.Stop();
                    isWalkSoundPlaying = false;
                }
            }
#else
            InputSystemHelper.EnableBackendsWarningMessage();
#endif
        }

        private void UpdatePlayerState()
        {
            PlayerState previousState = _currentState;
            _currentState = PlayerState.Normal;
            if (_currentState == PlayerState.Cutscene || _currentState == PlayerState.Disabled)
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
