using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class ControllerDetection : MonoBehaviour
{
    private GameObject xboxButtons;
    private GameObject psButtons;
    private GameObject switchButtons;
    private GameObject keyboardButtons;

    private Gamepad gamepad;
    private bool _usingController = false;
    public bool UsingController => _usingController;
    // Start is called before the first frame update
    void Start()
    {
        gamepad = Gamepad.current;
        Debug.Log("AAAAAA");
        
        
        InputUser.onChange.AddListener(EditButtons);
    }

    // Update is called once per frame
    void Update()
    {
        
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
