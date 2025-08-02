using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem;
using Input = UnityEngine.Input;



public class DisplayButton : MonoBehaviour
{
    [SerializeField] private GameObject keyboardKey;

    //public Image controllerKey;
    [SerializeField] private GameObject PSkey;
    [SerializeField] private GameObject XBOXkey;
    [SerializeField] private GameObject Switchkey;
    

    void Start()
    {
        keyboardKey.SetActive(false);
        PSkey.SetActive(false);
        XBOXkey.SetActive(false);
        Switchkey.SetActive(false);
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        ControllerDetection.OnInputModeChanged += UpdateButton;
    }

    private void UpdateButton(ControllerDetection.InputMode mode)
    {
        
        if (mode == ControllerDetection.InputMode.Keyboard)
        {
            keyboardKey.SetActive(true);
            PSkey.SetActive(false);
            XBOXkey.SetActive(false);
            Switchkey.SetActive(false);
        }

        if (mode == ControllerDetection.InputMode.Gamepad)
        {
            var gamepad = Gamepad.current;
            if (gamepad == null)
                return; // do nothing
            keyboardKey.SetActive(false);
            PSkey.SetActive(false);
            XBOXkey.SetActive(false);
            Switchkey.SetActive(false);
            if (gamepad is DualShockGamepad)
            {
                PSkey.SetActive(true);
            }
            else if (gamepad is XInputController) 
            {
                XBOXkey.SetActive(true);
            }
            else if (gamepad is SwitchProControllerHID)
            {
                Switchkey.SetActive(true);
            }
        }
    }

    private void OnDisable()
    {
        ControllerDetection.OnInputModeChanged -= UpdateButton;
    }

}
