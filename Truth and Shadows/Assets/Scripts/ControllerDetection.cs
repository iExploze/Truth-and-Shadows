using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System;
using Input = UnityEngine.Input;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.Switch;

public class ControllerDetection : MonoBehaviour
{
    // [SerializeField] private GameObject xboxButtons;
    // [SerializeField] private GameObject psButtons;
    // [SerializeField] private GameObject switchButtons;
    // [SerializeField] private GameObject keyboardButtons;

    private Gamepad gamepad;
    // private bool _usingController = false;
    // public bool UsingController => _usingController;

    // public enum InputMode
    // {
    //     Gamepad,
    //     Keyboard
    // }
    public enum InputMode
    {
        PS,
        XBOX,
        Switch,
        Keyboard
    }
    
    private InputMode _inputType;
    private InputMode _inputModeLastFrame;

    public static Action<InputMode> OnInputModeChanged;

    // Start is called before the first frame update
    void Start()
    {
        // _inputType = InputType.Gamepad;
        // gamepad = Gamepad.current;
        // Debug.Log("AAAAAA");
        //
        
        // InputUser.onChange.AddListener(EditButtons);
        _inputType = InputMode.Keyboard;
    }

    // Update is called once per frame
    void Update()
    {
        _inputType = GetInputMode();
        if (_inputType != _inputModeLastFrame)
        { // if changed
            OnInputModeChanged?.Invoke(_inputType);
            // EditButtons(_inputType);
        }
        _inputModeLastFrame = GetInputMode();

    }

    private InputMode GetInputMode()
    {
        if (Input.GetJoystickNames().Length == 0)
        {
            // EditButtons();
            // if no controllers plugged in then use keyboard
            return InputMode.Keyboard;
        }

        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1)
              || Input.GetKeyDown(KeyCode.JoystickButton2) ||Input.GetKeyDown(KeyCode.JoystickButton3) 
              ||Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.JoystickButton5) 
              ||Input.GetKeyDown(KeyCode.JoystickButton6) || Input.GetKeyDown(KeyCode.JoystickButton7)
                ||Input.GetKeyDown(KeyCode.JoystickButton8) || Input.GetKeyDown(KeyCode.JoystickButton9)
                ||Input.GetKeyDown(KeyCode.JoystickButton10) || Input.GetKeyDown(KeyCode.JoystickButton11)
                ||Input.GetKeyDown(KeyCode.JoystickButton12) || Input.GetKeyDown(KeyCode.JoystickButton13)
                ||Input.GetKeyDown(KeyCode.JoystickButton14) || Input.GetKeyDown(KeyCode.JoystickButton15)
                ||Input.GetKeyDown(KeyCode.JoystickButton16) || Input.GetKeyDown(KeyCode.JoystickButton17)
                ||Input.GetKeyDown(KeyCode.JoystickButton18) || Input.GetKeyDown(KeyCode.JoystickButton19))
            {
                var gamepad = Gamepad.current;
                if (gamepad == null)
                    return InputMode.Keyboard;
                if (gamepad is DualShockGamepad)
                {
                    return InputMode.PS;
                }
                else if (gamepad is XInputController) 
                {
                    return InputMode.XBOX;
                }
                else if (gamepad is SwitchProControllerHID)
                {
                    return InputMode.Switch;
                }
                else // default to keyboard
                {
                    return InputMode.Keyboard;
                }
            }
            else
            {
                return InputMode.Keyboard;
            }
            
        }

        if (Input.anyKey)
        {
            // unity only recognizes input.anykey for keyboard presses
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                return InputMode.Keyboard;
            }
        }
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal2") != 0 || Input.GetAxisRaw("Vertical2") != 0
            || Input.GetAxisRaw("HorizontalD") != 0 || Input.GetAxisRaw("VerticalD") != 0 
            // || Input.GetAxisRaw("LeftTrigger") < 0 || Input.GetAxisRaw("RightTrigger") < 0
            )
        {
            var gamepad = Gamepad.current;
            if (gamepad == null)
                return InputMode.Keyboard;
            if (gamepad is DualShockGamepad)
            {
                return InputMode.PS;
            }
            else if (gamepad is XInputController) 
            {
                return InputMode.XBOX;
            }
            else if (gamepad is SwitchProControllerHID)
            {
                return InputMode.Switch;
            }
            else // default to keyboard
            {
                return InputMode.Keyboard;
            }
        }
        return _inputType;
    }

    // public void EditButtons(ControllerDetection.InputMode mode)
    // {
    //    
    //     
    //     // xboxButtons = GameObject.Find("/WinScreen/Panel/XBOX buttons");
    //     // psButtons = GameObject.Find("/WinScreen/Panel/PS buttons");
    //     // switchButtons = GameObject.Find("/WinScreen/Panel/switch buttons");
    //     // keyboardButtons = GameObject.Find("/WinScreen/Panel/keyboard");
    //     //
    //     
    //     // Debug.Log(xboxButtons == null);
    //     keyboardButtons.SetActive(false);
    //     xboxButtons.SetActive(false);
    //     psButtons.SetActive(false);
    //     switchButtons.SetActive(false);
    //
    //     if (mode == InputMode.XBOX)
    //     {
    //         xboxButtons.SetActive(true);
    //     }
    //     else if (mode == InputMode.PS)
    //     {
    //         psButtons.SetActive(true);
    //     }
    //     else if (mode == InputMode.Switch)
    //     {
    //         switchButtons.SetActive(true);
    //     }
    //     else
    //     { // default to keyboard
    //         Debug.Log("Gamepad is null");
    //         // test = GameObject.Find("WinScreen/Panel/PS buttons");
    //         keyboardButtons.SetActive(true);
    //
    //     }
    //
    //     
    // }
}
