using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Input = UnityEngine.Input;

namespace TruthAndShadows.InputSystem
{
    /// <summary>
    /// Centralized input manager that handles both keyboard/mouse and controller inputs.
    /// Provides consistent input methods regardless of input device.
    /// All game systems should query this manager rather than using Input directly.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        public static InputManager Instance => _instance;

        #region Input State Properties
        // These properties provide immediate access to current input state
        // Other systems should use these rather than querying Input directly

        // Movement
        public Vector2 CharacterMoveInput { get; private set; }
        public Vector2 InteractableMoveInput { get; private set; }
        public Vector2 MoveInputRaw { get; private set; }
        public bool IsRunning { get; private set; }

        // Camera
        public Vector2 LookInput { get; private set; }
        public Vector2 PickupCameraInput { get; private set; }

        // Interactions
        public bool InteractPressed { get; private set; }
        public bool InteractHeld { get; private set; }
        public bool InteractReleased { get; private set; }

        public bool PickupPressed { get; private set; }
        public bool PickupHeld { get; private set; }
        public bool PickupReleased { get; private set; }

        public bool RotateHeld { get; private set; }
        public bool ResetPressed { get; private set; }
        public bool MenuPressed { get; private set; }

        public bool HintPressed { get; private set; }
        public bool HintHeld { get; private set; }
        public bool HintReleased { get; private set; }
        #endregion

        [Header("Controller Settings")]
        [SerializeField]
        private float joystickDeadzone = 0.2f;

        [SerializeField]
        private float rightStickSensitivity = 1.0f;

        [SerializeField]
        private bool invertRightStickY = false;

        #region Controller Mappings
        private static readonly KeyCode[] interactButtons = new KeyCode[]
        {
            KeyCode.R, // Keyboard
            KeyCode.JoystickButton4, // Xbox LB, PS L1, Switch L
        };

        private readonly KeyCode[] pickupButtons = new KeyCode[]
        {
            KeyCode.F, // Keyboard
            KeyCode.JoystickButton5, // Xbox RB, PS R1, Switch R
        };

        private readonly KeyCode[] rotateButtons = new KeyCode[]
        {
            KeyCode.R, // Keyboard
            KeyCode.JoystickButton4, // Xbox LB, PS L1, Switch L
        };

        private readonly KeyCode[] resetButtons = new KeyCode[]
        {
            KeyCode.L, // Keyboard
            KeyCode.JoystickButton8, // Xbox Back/View, PS Share, Switch -
        };

        private readonly KeyCode[] hintButtons = new KeyCode[]
        {
            KeyCode.K, // Keyboard
            KeyCode.JoystickButton0, // Xbox B, PS Circle, Switch A
        };

        private readonly KeyCode[] menuButtons = new KeyCode[]
        {
            KeyCode.U,
            KeyCode.Escape,
            KeyCode.JoystickButton3, // Xbox X, PS Square, Switch Y
            KeyCode.JoystickButton7, // Xbox Menu/Start, PS Options, Switch +
        };
        #endregion

        private bool _usingController = false;
        public bool UsingController => _usingController;

        // Track previous inputs for detecting button state changes
        private bool _prevPickupHeld = false;
        private bool _prevInteractHeld = false;
        private bool _prevHintHeld = false;

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
            // Optionally reset input state or re-initialize if needed
            // (No scene-dependent references in current code)
        }

        private void Update()
        {
            // Process all inputs every frame in a centralized location
            ProcessInputs();
        }

        /// <summary>
        /// Processes all input in a single place to ensure consistency
        /// This is called once per frame in Update and sets all input properties
        /// </summary>
        private void ProcessInputs()
        {
            // Detect if player is using controller
            _usingController = IsControllerConnected() && HasControllerInput();

            // Check for input context permissions from centralized provider
            bool allowMovement = true;
            bool allowCameraLook = true;
            bool allowInteract = true;
            bool allowPickup = true;
            bool allowRotate = true;
            bool allowRun = true;
            bool allowHint = true;
            bool allowReset = true;
            bool allowMenu = true;

            // Get permissions from InputContextProvider if available
            var contextProvider = InputContextProvider.Instance;
            if (contextProvider != null)
            {
                allowMovement = contextProvider.CanMove;
                allowCameraLook = contextProvider.CanCameraLook;
                allowInteract = contextProvider.CanInteract;
                allowPickup = contextProvider.CanPickup;
                allowRotate = contextProvider.CanRotate;
                allowRun = contextProvider.CanRun;
                allowHint = contextProvider.CanHint;
                allowReset = contextProvider.CanReset;
                allowMenu = contextProvider.CanMenu;
            }

            // Always detect raw inputs first (for state tracking)
            bool rawInteractHeld = GetInteractButtonInternal();
            bool rawPickupHeld = GetPickupButtonInternal();
            bool rawRotateHeld = GetRotateButtonInternal();
            bool rawResetPressed = GetResetButtonDownInternal();
            bool rawMenuPressed = GetMenuButtonDownInternal();
            bool rawHintHeld = GetHintButtonInternal();
            bool rawSprintHeld = IsSprintHeldInternal();

            // Then apply permissions to determine the final input state            // Update all movement inputs (respect movement permission)
            CharacterMoveInput = allowMovement ? GetMovementInputInternal() : Vector2.zero;
            InteractableMoveInput = allowInteract ? GetMovementInputInternal() : Vector2.zero;
            MoveInputRaw = allowMovement ? GetMovementInputRawInternal() : Vector2.zero;
            IsRunning = allowRun && rawSprintHeld;

            // Handle camera movement
            if (allowCameraLook)
            {
                LookInput = GetLookInputInternal();
            }
            else
            {
                LookInput = Vector2.zero;
                PickupCameraInput = Vector2.zero;
            }

            // Update all interaction inputs (with permissions)
            InteractHeld = allowInteract && rawInteractHeld;
            InteractPressed = InteractHeld && !_prevInteractHeld;
            InteractReleased = !InteractHeld && _prevInteractHeld;
            _prevInteractHeld = InteractHeld;

            PickupHeld = allowPickup && rawPickupHeld;
            PickupPressed = PickupHeld && !_prevPickupHeld;
            PickupReleased = !PickupHeld && _prevPickupHeld;
            _prevPickupHeld = PickupHeld;

            RotateHeld = allowRotate && rawRotateHeld;
            ResetPressed = allowReset && rawResetPressed;
            MenuPressed = allowMenu && rawMenuPressed;

            HintHeld = allowHint && rawHintHeld;
            HintPressed = HintHeld && !_prevHintHeld;
            HintReleased = !HintHeld && _prevHintHeld;
            _prevHintHeld = HintHeld;
        }

        /// <summary>
        /// Detects if a controller is connected
        /// </summary>
        public static bool IsControllerConnected()
        {
            return Input.GetJoystickNames().Length > 0
                && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]);
        }

        /// <summary>
        /// Checks for any controller input
        /// </summary>
        private bool HasControllerInput()
        {
            // Only consider right stick movement as controller input
            return Mathf.Abs(Input.GetAxis("RightStickHorizontal")) > joystickDeadzone
                || Mathf.Abs(Input.GetAxis("RightStickVertical")) > joystickDeadzone;
        }

        #region Movement Input Internals
        private static Vector2 GetMovementInputInternal()
        {
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        private static Vector2 GetMovementInputRawInternal()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private static bool IsSprintHeldInternal()
        {
            return Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton2); // Xbox X, PS Square, Switch Y
        }
        #endregion

        #region Camera/Look Input Internals
        private Vector2 GetLookInputInternal()
        {
            // Always allow camera movement regardless of other actions
            if (_usingController)
            {
                float x = Input.GetAxis("RightStickHorizontal") * rightStickSensitivity;
                float y = Input.GetAxis("RightStickVertical") * rightStickSensitivity;

                if (invertRightStickY)
                {
                    y = -y;
                }

                return new Vector2(x, y);
            }
            else
            {
                return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }
        }

        /// <summary>
        /// Gets look input specifically for pickup/interaction scenarios.
        /// Uses slightly enhanced sensitivity values for better camera control.
        /// </summary>
        private Vector2 GetPickupCameraInputInternal()
        {
            // When using controller during pickup
            if (_usingController)
            {
                // Use RAW axis values for maximum responsiveness
                float x = Input.GetAxisRaw("RightStickHorizontal");
                float y = Input.GetAxisRaw("RightStickVertical");

                // Use higher sensitivity during pickup for better feel
                x *= rightStickSensitivity * 1.5f;
                y *= rightStickSensitivity * 1.5f;

                if (invertRightStickY)
                {
                    y = -y;
                }

                return new Vector2(x, y);
            }
            // When using mouse during pickup
            else
            {
                // For mouse, use a slightly higher sensitivity during pickup for better control
                float mouseX = Input.GetAxis("Mouse X") * 1.5f;
                float mouseY = Input.GetAxis("Mouse Y") * 1.5f;

                return new Vector2(mouseX, mouseY);
            }
        }
        #endregion

        #region Interaction Input Internals

        private static bool AnyKeyDown(KeyCode[] keys) => keys.Any(Input.GetKeyDown);

        private static bool AnyKey(KeyCode[] keys) => keys.Any(Input.GetKey);

        private static bool AnyKeyUp(KeyCode[] keys) => keys.Any(Input.GetKeyUp);

        private static bool GetInteractButtonDownInternal() => AnyKeyDown(interactButtons);

        private static bool GetInteractButtonInternal() => AnyKey(interactButtons);

        private static bool GetInteractButtonUpInternal() => AnyKeyUp(interactButtons);

        private bool GetPickupButtonDownInternal() => AnyKeyDown(pickupButtons);

        private bool GetPickupButtonInternal() => AnyKey(pickupButtons);

        private bool GetPickupButtonUpInternal() => AnyKeyUp(pickupButtons);

        private bool GetRotateButtonInternal() => AnyKey(rotateButtons);

        private bool GetResetButtonDownInternal() => AnyKeyDown(resetButtons);
        private bool GetMenuButtonDownInternal() => AnyKeyDown(menuButtons);

        private bool GetHintButtonDownInternal() => AnyKeyDown(hintButtons);

        private bool GetHintButtonInternal() => AnyKey(hintButtons);

        private bool GetHintButtonUpInternal() => AnyKeyUp(hintButtons);

        #endregion
    }
}
