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
        UpdateButton(ControllerDetection.InputMode.Keyboard); // open with default
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
        var gamepad = Gamepad.current;
        if (gamepad == null)
            return; // do nothing
        keyboardKey.SetActive(false);
        PSkey.SetActive(false);
        XBOXkey.SetActive(false);
        Switchkey.SetActive(false);

        if (mode == ControllerDetection.InputMode.PS)
        {
            PSkey.SetActive(true);
        }
        else if (mode == ControllerDetection.InputMode.XBOX) 
        {
            XBOXkey.SetActive(true);
        }
        else if (mode == ControllerDetection.InputMode.Switch) 
        {
            Switchkey.SetActive(true);
        }
        else // default to keyboard
        {
            keyboardKey.SetActive(true);
            PSkey.SetActive(false);
            XBOXkey.SetActive(false);
            Switchkey.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ControllerDetection.OnInputModeChanged -= UpdateButton;
    }

}
