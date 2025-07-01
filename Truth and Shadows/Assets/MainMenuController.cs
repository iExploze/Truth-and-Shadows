using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Input = UnityEngine.Input;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    private Gamepad gamepad;
    private bool _usingController = false;
    public bool UsingController => _usingController;

    public CanvasGroup SettingsPanel;
    public CanvasGroup LevelsPanel;
    public GameObject firstLevelButton;

    void Start()
    {
        gamepad = Gamepad.current;
    }

    void Update()
    {
        if (gamepad != null)
        {
            ProcessInputs();
        }
    }

    private void ProcessInputs()
    {
        // Gamepad: X/A to Play
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            PlayGame();
        }

        // Gamepad: Square/X to Open Settings (if none visible)
        if (gamepad.buttonWest.wasPressedThisFrame && !IsAnyPanelOpen())
        {
            Settings();
        }

        // Gamepad: Triangle/Y to Open Levels (if none visible)
        if (gamepad.buttonNorth.wasPressedThisFrame && !IsAnyPanelOpen())
        {
            Levels();
        }

        // Gamepad: Circle/B to Back or Quit
        if (gamepad.buttonEast.wasPressedThisFrame)
        {
            if (!IsAnyPanelOpen())
            {
                QuitGame();
            }
            else
            {
                Back();
            }
        }
    }

    // Helper to check if either panel is open
    private bool IsAnyPanelOpen()
    {
        return SettingsPanel.alpha > 0 || LevelsPanel.alpha > 0;
    }

    public void PlayGame()
    {
        LevelManager.Instance.LoadScene("Level Hallway", "CrossFade");
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

    public void Back()
    {
        SettingsPanel.alpha = 0;
        SettingsPanel.blocksRaycasts = false;
        SettingsPanel.interactable = false;

        LevelsPanel.alpha = 0;
        LevelsPanel.blocksRaycasts = false;
        LevelsPanel.interactable = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadLevel1()
    {
        LevelManager.Instance.LoadScene("Level Hallway", "CrossFade");
    }

    public void LoadLevel2()
    {
        LevelManager.Instance.LoadScene("LevelDesigntest", "CrossFade");
    }
}
