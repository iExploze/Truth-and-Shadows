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

public class ControllerDetection : MonoBehaviour
{
    private GameObject xboxButtons;
    private GameObject psButtons;
    private GameObject switchButtons;
    private GameObject keyboardButtons;

    private Gamepad gamepad;
    // private bool _usingController = false;
    // public bool UsingController => _usingController;

    public enum InputMode
    {
        Gamepad,
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
        _inputType = InputMode.Gamepad;
    }

    // Update is called once per frame
    void Update()
    {
        _inputType = GetInputMode();
        if (_inputType != _inputModeLastFrame)
        {
            OnInputModeChanged?.Invoke(_inputType);
        }
        _inputModeLastFrame = GetInputMode();

    }

    private InputMode GetInputMode()
    {
        if (Input.GetJoystickNames().Length == 0)
        {
            // if no controllers plugged in then use keyboard
            return InputMode.Keyboard;
        }

        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                return InputMode.Gamepad;
            }else if (Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton4))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton6))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton8))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton9))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton10))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton11))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton12))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton13))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton14))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton15))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton16))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton17))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton18))
            {
                return InputMode.Gamepad;
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton19))
            {
                return InputMode.Gamepad;
            }
            else
            {
                return InputMode.Keyboard;
            }
        }
        return _inputType;
    }

    public void EditButtons()
    {
       
        
        xboxButtons = GameObject.Find("WinScreen/Panel/XBOX buttons");
        psButtons = GameObject.Find("WinScreen/Panel/PS buttons");
        switchButtons = GameObject.Find("WinScreen/Panel/switch buttons");
        keyboardButtons = GameObject.Find("WinScreen/Panel/keyboard");
        
        
        // Debug.Log(xboxButtons == null);
        keyboardButtons.SetActive(false);
        xboxButtons.SetActive(false);
        psButtons.SetActive(false);
        switchButtons.SetActive(false);

        if (gamepad == null)
        {
            // ProcessInputs();
            Debug.Log("Gamepad is not null");
            xboxButtons.SetActive(true);
            psButtons.SetActive(true);
            switchButtons.SetActive(true);
            // test = GameObject.Find("WinScreen/Panel/keyboard");
        }
        else
        { // default to keyboard
            Debug.Log("Gamepad is null");
            // test = GameObject.Find("WinScreen/Panel/PS buttons");
            keyboardButtons.SetActive(true);

        }

        
    }
}
