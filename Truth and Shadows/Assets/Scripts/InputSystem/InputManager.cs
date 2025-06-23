using UnityEngine;
using Input = UnityEngine.Input;

namespace TruthAndShadows.InputSystem
{
    /// <summary>
    /// Centralized input manager that handles both keyboard/mouse and controller inputs.
    /// Provides consistent input methods regardless of input device.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        public static InputManager Instance => _instance;

        [Header("Controller Settings")]
        [SerializeField]
        private float joystickDeadzone = 0.2f;

        [SerializeField]
        private float rightStickSensitivity = 1.0f;

        [SerializeField]
        private bool invertRightStickY = false; // Controller button mappings (used internally)

        // Interaction button (left bumper/shoulder button - matches R key functionality)
        private readonly KeyCode[] interactButtons = new KeyCode[]
        {
            KeyCode.JoystickButton4, // Xbox LB, PS L1, Switch L
        };

        // Pickup button (right bumper/shoulder button - matches F key functionality)
        private readonly KeyCode[] pickupButtons = new KeyCode[]
        {
            KeyCode.JoystickButton5, // Xbox RB, PS R1, Switch R
        }; // Left bumper is used for spotlight aiming (same as R key)

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

        private bool _usingController = false;
        public bool UsingController => _usingController;

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
            // Detect if player is using controller
            _usingController = IsControllerConnected() && HasControllerInput();
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

        #region Movement Input

        public Vector2 GetMovementInput()
        {
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        public Vector2 GetMovementInputRaw()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        public bool IsSprintHeld()
        {
            return Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton2); // Xbox X, PS Square, Switch Y
        }

        #endregion

        #region Camera/Look Input        /// <summary>
        /// Gets the look/camera rotation input (Mouse or Right Stick)
        /// Always returns camera input regardless of other actions
        /// </summary>
        public Vector2 GetLookInput()
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

        #endregion

        #region Interaction Input        
        /// <summary>
        /// Returns true during the frame the interact button is pressed
        /// </summary>
        public bool GetInteractButtonDown()
        {
            bool pickupHeld = Input.GetKey(KeyCode.F);
            if (!pickupHeld)
            {
                foreach (KeyCode key in pickupButtons)
                {
                    if (Input.GetKey(key))
                    {
                        pickupHeld = true;
                        break;
                    }
                }
            }

            // Don't allow interaction if pickup button is held
            if (pickupHeld)
                return false;

            if (Input.GetKeyDown(KeyCode.R))
                return true;

            foreach (KeyCode key in interactButtons)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true while the interact button is held
        /// </summary>
        public bool GetInteractButton()
        {
            // Check direct input for pickup buttons to avoid circular reference
            bool pickupHeld = Input.GetKey(KeyCode.F);
            if (!pickupHeld)
            {
                foreach (KeyCode key in pickupButtons)
                {
                    if (Input.GetKey(key))
                    {
                        pickupHeld = true;
                        break;
                    }
                }
            }

            // Don't allow interaction if pickup button is held
            if (pickupHeld)
                return false;

            if (Input.GetKey(KeyCode.R))
                return true;

            foreach (KeyCode key in interactButtons)
            {
                if (Input.GetKey(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true during the frame the interact button is released
        /// </summary>
        public bool GetInteractButtonUp()
        {
            // Still allow button up events even if pickup is held
            // This ensures we don't miss the release event
            if (Input.GetKeyUp(KeyCode.R))
                return true;

            foreach (KeyCode key in interactButtons)
            {
                if (Input.GetKeyUp(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true during the frame the pickup button is pressed
        /// </summary>
        public bool GetPickupButtonDown()
        {
            // Check direct input for interact buttons to avoid circular reference
            bool interactHeld = Input.GetKey(KeyCode.R);
            if (!interactHeld)
            {
                foreach (KeyCode key in interactButtons)
                {
                    if (Input.GetKey(key))
                    {
                        interactHeld = true;
                        break;
                    }
                }
            }

            // Don't allow pickup if interact button is held
            if (interactHeld)
                return false;

            if (Input.GetKeyDown(KeyCode.F))
                return true;

            foreach (KeyCode key in pickupButtons)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true while the pickup button is held
        /// </summary>
        public bool GetPickupButton()
        {
            // We check the raw input here to avoid circular reference with GetInteractButton
            bool interactHeld = Input.GetKey(KeyCode.R);
            if (!interactHeld)
            {
                foreach (KeyCode key in interactButtons)
                {
                    if (Input.GetKey(key))
                    {
                        interactHeld = true;
                        break;
                    }
                }
            }

            // Don't allow pickup if interact button is held
            if (interactHeld)
                return false;

            if (Input.GetKey(KeyCode.F))
                return true;

            foreach (KeyCode key in pickupButtons)
            {
                if (Input.GetKey(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true during the frame the pickup button is released
        /// </summary>
        public bool GetPickupButtonUp()
        {
            if (Input.GetKeyUp(KeyCode.F))
                return true;

            foreach (KeyCode key in pickupButtons)
            {
                if (Input.GetKeyUp(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true while the rotate/aim button is held (R key or left bumper)
        /// Used for aiming the spotlight, ensures mutual exclusivity with pickup
        /// </summary>
        public bool GetRotateButton()
        {
            // Check direct input for pickup buttons to avoid circular reference
            bool pickupHeld = Input.GetKey(KeyCode.F);
            if (!pickupHeld)
            {
                foreach (KeyCode key in pickupButtons)
                {
                    if (Input.GetKey(key))
                    {
                        pickupHeld = true;
                        break;
                    }
                }
            }

            // Don't allow rotation/aiming if pickup button is held
            if (pickupHeld)
                return false;

            // Check R key or left bumper (uses same buttons as interact)
            if (Input.GetKey(KeyCode.R))
                return true;

            foreach (KeyCode key in rotateButtons)
            {
                if (Input.GetKey(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true during the frame the reset button is pressed
        /// </summary>
        public bool GetResetButtonDown()
        {
            if (Input.GetKeyDown(KeyCode.L))
                return true;

            foreach (KeyCode key in resetButtons)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }

            return false;
        }        /// <summary>
        /// Returns true during the frame the hint button is pressed (K key or B button on Xbox, Circle on PS, or A on Switch Pro)
        /// </summary>
        public bool GetHintButtonDown()
        {
            if (Input.GetKeyDown(KeyCode.K))
                return true;

            foreach (KeyCode key in hintButtons)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }

            return false;
        }        /// <summary>
        /// Returns true while the hint button is held (K key or B button on Xbox, Circle on PS, or A on Switch Pro)
        /// </summary>
        public bool GetHintButton()
        {
            if (Input.GetKey(KeyCode.K))
                return true;

            foreach (KeyCode key in hintButtons)
            {
                if (Input.GetKey(key))
                    return true;
            }

            return false;
        }        /// <summary>
        /// Returns true during the frame the hint button is released (K key or B button on Xbox, Circle on PS, or A on Switch Pro)
        /// </summary>
        public bool GetHintButtonUp()
        {
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
    }
}
