using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Input = UnityEngine.Input;
using UnityEngine.InputSystem;
public class MainMenuController : MonoBehaviour
{
    private Gamepad gamepad;
    private bool _usingController = false;
    public bool UsingController => _usingController;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(GameObject.Find("CheckpointManager"));
        //GameObject.Find("CheckpointManager").SetActive(false);
        if (GameObject.Find("CheckpointManager") != null)
        {
            Debug.Log("AAAAAA");
            GameObject.Find("CheckpointManager").SetActive(false);
        }
        gamepad = Gamepad.current;
    }

    // Update is called once per frame
    void Update()
    {
        if (gamepad != null)
        {
            ProcessInputs();
            // Debug.Log("AAAAAA");
        }

        
    }
    
    private void ProcessInputs()
    {
        // PS X, XBOX A
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            PlayGame();
        }

        // PS SQUARE, XBOX X
        if (gamepad.buttonWest.wasPressedThisFrame && SettingsPanel.alpha == 0)
        {
            Settings();
        }

        // PS CIRCLE, XBOX B
        if (gamepad.buttonEast.wasPressedThisFrame)
        {
            if (SettingsPanel.alpha == 0)
            {
                QuitGame();
            }
            else
            {
                Back();
            }
        }



    }
    
    // private bool IsControllerConnected()
    // {
    //     return Gamepad.all.Count > 0;
    // }

    public CanvasGroup SettingsPanel;

    public void PlayGame()
    {
        LevelManager.Instance.LoadScene("Level Hallway", "CrossFade");
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Settings()
    {
        SettingsPanel.alpha = 1;
        SettingsPanel.blocksRaycasts = true;
    }

    public void Back()
    {
        SettingsPanel.alpha = 0;
        SettingsPanel.blocksRaycasts = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
