using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Input = UnityEngine.Input;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance;
    private Gamepad gamepad;
    private bool _usingController = false;
    public bool UsingController => _usingController;

    public CanvasGroup SettingsPanel;
    public CanvasGroup LevelsPanel;
    public GameObject firstLevelButton;
    public GameObject firstMenuButton; // Added field for the first menu button
    
    [SerializeField]
    private Slider _sliderBrightness;
    [SerializeField]
    private Image _blackOverlay;

    public static float brightness = 1;
    
    // Start is called before the 
    void Start()
    {
        gamepad = Gamepad.current;
        // Set initial selected button for joystick navigation
        if (firstMenuButton != null && !IsAnyPanelOpen())
        {
            EventSystem.current.SetSelectedGameObject(firstMenuButton);
        }

        brightness = 1;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }

    
    void Update()
    {
        if (gamepad != null)
        {
            ProcessInputs();
            //
            // if (gamepad is DualShockGamepad)
            // {
            //     print("Playstation gamepad");
            // }
        }
    }

    private void ProcessInputs()
    {
        // // Gamepad: X/A to Play
        // if (gamepad.buttonSouth.wasPressedThisFrame)
        // {
        //     PlayGame();
        // }

        // // Gamepad: Square/X to Open Settings (if none visible)
        // if (gamepad.buttonWest.wasPressedThisFrame && !IsAnyPanelOpen())
        // {
        //     Settings();
        // }

        // // Gamepad: Triangle/Y to Open Levels (if none visible)
        // if (gamepad.buttonNorth.wasPressedThisFrame && !IsAnyPanelOpen())
        // {
        //     Levels();
        // }

        // // Gamepad: Circle/B to Back or Quit
        if (gamepad.buttonEast.wasPressedThisFrame)
        {
            if (IsAnyPanelOpen())
            {
                Back();
            }
            // else
            // {
            //     Back();
            // }
        }
    }

    // Helper to check if either panel is open
    private bool IsAnyPanelOpen()
    {
        return SettingsPanel.alpha > 0 || LevelsPanel.alpha > 0;
    }

    public void PlayGame()
    {
        LevelManager.Instance.LoadScene("Level1", "CrossFade");
    }

    public void Settings()
    {
        SettingsPanel.alpha = 1;
        SettingsPanel.blocksRaycasts = true;
        SettingsPanel.interactable = true;
    }

    public void Levels()
    {
        LevelsPanel.alpha = 1;
        LevelsPanel.blocksRaycasts = true;
        LevelsPanel.interactable = true;

        EventSystem.current.SetSelectedGameObject(firstLevelButton);
    }

    public void AdjustBrightness(float value)
    {
        brightness = value;
        // var tempColor = _blackOverlay.color;
        // tempColor.a = _sliderBrightness.value;
        // _blackOverlay.color = tempColor;
    }

    public void Back()
    {
        SettingsPanel.alpha = 0;
        SettingsPanel.blocksRaycasts = false;
        SettingsPanel.interactable = false;

        LevelsPanel.alpha = 0;
        LevelsPanel.blocksRaycasts = false;
        LevelsPanel.interactable = false;

        // After closing panels, set main menu button as selected for controller
        if (firstMenuButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstMenuButton);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadLevel1()
    {
        LevelManager.Instance.LoadScene("Level1", "CrossFade");
    }

    public void LoadLevel2()
    {
        LevelManager.Instance.LoadScene("Level2New", "CrossFade");
    }

    public void LoadLevel3()
    {
        LevelManager.Instance.LoadScene("level 3", "CrossFade");
    }
}
