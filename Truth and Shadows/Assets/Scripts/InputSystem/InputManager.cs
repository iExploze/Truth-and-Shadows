using System.Collections;
using UnityEngine;
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
        public Vector2 MoveInput { get; private set; }
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
        // Interaction button (left bumper/shoulder button - matches R key functionality)
        private readonly KeyCode[] interactButtons = new KeyCode[]
        {
            KeyCode.JoystickButton4, // Xbox LB, PS L1, Switch L
        };

        // Pickup button (right bumper/shoulder button - matches F key functionality)W
        private readonly KeyCode[] pickupButtons = new KeyCode[]
        {
            KeyCode.JoystickButton5, // Xbox RB, PS R1, Switch R
        };

        // This should match interactButtons for consistency
        private readonly KeyCode[] rotateButtons = new KeyCode[]
        {
            KeyCode.JoystickButton4, // Xbox LB, PS L1, Switch L
        };

        private readonly KeyCode[] resetButtons = new KeyCode[]
        {
            KeyCode.JoystickButton6, // Xbox Back/View, PS Share, Switch -
        };

        // Hint button (rightmost face button - matches K key functionality)
        private readonly KeyCode[] hintButtons = new KeyCode[]
        {
            KeyCode.JoystickButton0, // Xbox B, PS Circle, Switch A
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
            DontDestroyOnLoad(gameObject);
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
            }

            // Always detect raw inputs first (for state tracking)
            bool rawInteractHeld = GetInteractButtonInternal();
            bool rawPickupHeld = GetPickupButtonInternal();
            bool rawRotateHeld = GetRotateButtonInternal();
            bool rawResetPressed = GetResetButtonDownInternal();
            bool rawHintHeld = GetHintButtonInternal();
            bool rawSprintHeld = IsSprintHeldInternal();

            // Then apply permissions to determine the final input state            // Update all movement inputs (respect movement permission)
            MoveInput = allowMovement ? GetMovementInputInternal() : Vector2.zero;
            MoveInputRaw = allowMovement ? GetMovementInputRawInternal() : Vector2.zero;
            IsRunning = allowRun && rawSprintHeld;

            // Handle camera movement
            if (allowCameraLook)
            {
                // Get regular look input
                LookInput = GetLookInputInternal();

                // During pickup/interaction, use specialized camera input
                PickupCameraInput = GetPickupCameraInputInternal();
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

            HintHeld = allowHint && rawHintHeld;
            HintPressed = HintHeld && !_prevHintHeld;
            HintReleased = !HintHeld && _prevHintHeld;
            _prevHintHeld = HintHeld;

            // Force Unity to process all input axes to prevent potential input blocking
            // This helps ensure multiple inputs can be processed simultaneously
            ForceProcessAllInputAxes();
        }

        /// <summary>
        /// Detects if a controller is connected
        /// </summary>
        public bool IsControllerConnected()
        {
            return Input.GetJoystickNames().Length > 0
                && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]);
        }

        /// <summary>
        /// Checks for any controller input
        /// </summary>
        private bool HasControllerInput()
        {
            // Check joystick axes
            if (
                Mathf.Abs(Input.GetAxis("Horizontal")) > joystickDeadzone
                || Mathf.Abs(Input.GetAxis("Vertical")) > joystickDeadzone
                || Mathf.Abs(Input.GetAxis("RightStickHorizontal")) > joystickDeadzone
                || Mathf.Abs(Input.GetAxis("RightStickVertical")) > joystickDeadzone
            )
            {
                return true;
            }

            // Check any joystick buttons
            for (int i = 0; i < 20; i++)
            {
                if (Input.GetKey((KeyCode)(KeyCode.JoystickButton0 + i)))
                {
                    return true;
                }
            }

            return false;
        }

        #region Movement Input Internals
        private Vector2 GetMovementInputInternal()
        {
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        private Vector2 GetMovementInputRawInternal()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private bool IsSprintHeldInternal()
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
        }        /// <summary>
        /// Makes sure Unity processes important input axes to prevent input blocking issues.
        /// </summary>
        private void ForceProcessAllInputAxes()
        {
            // Force Unity to read these axes to ensure they're active
            // The values need to be read but don't need to be stored
            Input.GetAxis("Mouse X");
            Input.GetAxis("Mouse Y");
            Input.GetAxis("Horizontal");
            Input.GetAxis("Vertical");
        }
        #endregion

        #region Interaction Input Internals
        private bool GetInteractButtonDownInternal()
        {
            // Simply report if the interact button is pressed, regardless of other inputs
            if (Input.GetKeyDown(KeyCode.R))
                return true;

            foreach (KeyCode key in interactButtons)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }

            return false;
        }

        private bool GetInteractButtonInternal()
        {
            // Simply report if the interact button is held, regardless of other inputs
            if (Input.GetKey(KeyCode.R))
                return true;

            foreach (KeyCode key in interactButtons)
            {
                if (Input.GetKey(key))
                    return true;
            }

            return false;
        }

        private bool GetInteractButtonUpInternal()
        {
            // Simply report if the interact button is released, regardless of other inputs
            if (Input.GetKeyUp(KeyCode.R))
                return true;

            foreach (KeyCode key in interactButtons)
            {
                if (Input.GetKeyUp(key))
                    return true;
            }

            return false;
        }

        private bool GetPickupButtonDownInternal()
        {
            // Simply report if the pickup button is pressed, regardless of other inputs
            if (Input.GetKeyDown(KeyCode.F))
                return true;

            foreach (KeyCode key in pickupButtons)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }

            return false;
        }

        private bool GetPickupButtonInternal()
        {
            // Simply report if the pickup button is held, regardless of other inputs
            if (Input.GetKey(KeyCode.F))
                return true;

            foreach (KeyCode key in pickupButtons)
            {
                if (Input.GetKey(key))
                    return true;
            }

            return false;
        }

        private bool GetPickupButtonUpInternal()
        {
            // Simply report if the pickup button is released, regardless of other inputs
            if (Input.GetKeyUp(KeyCode.F))
                return true;

            foreach (KeyCode key in pickupButtons)
            {
                if (Input.GetKeyUp(key))
                    return true;
            }

            return false;
        }

        private bool GetRotateButtonInternal()
        {
            // Simply report if the rotate button is held, regardless of other inputs
            if (Input.GetKey(KeyCode.R))
                return true;

            foreach (KeyCode key in rotateButtons)
            {
                if (Input.GetKey(key))
                    return true;
            }

            return false;
        }

        private bool GetResetButtonDownInternal()
        {
            // Simply report if the reset button is pressed
            if (Input.GetKeyDown(KeyCode.L))
                return true;

            foreach (KeyCode key in resetButtons)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }

            return false;
        }

        private bool GetHintButtonDownInternal()
        {
            // Simply report if the hint button is pressed
            if (Input.GetKeyDown(KeyCode.K))
                return true;

            foreach (KeyCode key in hintButtons)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }

            return false;
        }

        private bool GetHintButtonInternal()
        {
            // Simply report if the hint button is held
            if (Input.GetKey(KeyCode.K))
                return true;

            foreach (KeyCode key in hintButtons)
            {
                if (Input.GetKey(key))
                    return true;
            }

            return false;
        }

        private bool GetHintButtonUpInternal()
        {
            // Simply report if the hint button is released
            if (Input.GetKeyUp(KeyCode.K))
                return true;

            foreach (KeyCode key in hintButtons)
            {
                if (Input.GetKeyUp(key))
                    return true;
            }

            return false;
        }
        #endregion

        #region Public API - Legacy Functions for Backwards Compatibility
        // These public methods are maintained for compatibility with existing code

        /// <summary>
        /// Gets the current movement vector.
        /// </summary>
        public Vector2 GetMovementInput() => MoveInput;

        /// <summary>
        /// Gets the raw (non-smoothed) movement vector.
        /// </summary>
        public Vector2 GetMovementInputRaw() => MoveInputRaw;

        /// <summary>
        /// Returns true if the sprint button is held.
        /// </summary>
        public bool IsSprintHeld() => IsRunning;

        /// <summary>
        /// Gets the look/camera rotation input (Mouse or Right Stick)
        /// </summary>
        public Vector2 GetLookInput() => LookInput;

        /// <summary>
        /// Gets look input that bypasses any potential input blocking during pickup.
        /// This method ensures camera control works properly during pickup actions.
        /// </summary>
        public Vector2 GetPickupCameraInput() => PickupCameraInput;

        /// <summary>
        /// Returns true during the frame the interact button is pressed
        /// </summary>
        public bool GetInteractButtonDown() => InteractPressed;

        /// <summary>
        /// Returns true while the interact button is held
        /// </summary>
        public bool GetInteractButton() => InteractHeld;

        /// <summary>
        /// Returns true during the frame the interact button is released
        /// </summary>
        public bool GetInteractButtonUp() => InteractReleased;

        /// <summary>
        /// Returns true during the frame the pickup button is pressed
        /// </summary>
        public bool GetPickupButtonDown() => PickupPressed;

        /// <summary>
        /// Returns true while the pickup button is held
        /// </summary>
        public bool GetPickupButton() => PickupHeld;

        /// <summary>
        /// Returns true during the frame the pickup button is released
        /// </summary>
        /// <summary>
        /// Returns true during the frame the reset button is pressed
        /// </summary>
        public bool GetResetButtonDown() => ResetPressed;

        /// <summary>
        /// Returns true during the frame the hint button is pressed
        /// </summary>
        public bool GetHintButtonDown() => HintPressed;

        /// <summary>
        /// Returns true while the hint button is held
        /// </summary>
        public bool GetHintButton() => HintHeld;

        /// <summary>
        /// Returns true during the frame the hint button is released
        /// </summary>
        public bool GetHintButtonUp() => HintReleased;

        /// <summary>
        /// Returns true while the rotate button is held        /// </summary>
        public bool GetRotateButton() => RotateHeld;
        #endregion
    }
}
