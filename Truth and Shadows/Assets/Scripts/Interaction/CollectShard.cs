using System.Collections;
using System.Collections.Generic;
using TMPro;
using TruthAndShadows.InputSystem;
using TruthAndShadows.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectShard : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource winSoundSource;

    [Header("UI")]
    public GameObject winMenu;

    [Header("References")]
    // Optional reference to button that should be selected first when using controller
    public GameObject firstSelectedButton;

    private InputContextProvider _inputContextProvider;

    private bool isWinMenuActive = false;
    private float activationTime;

    void Start()
    {
        // displayPoem = GetComponent<DisplayPoem>();
        winMenu.SetActive(false);
        _inputContextProvider = InputContextProvider.Instance;
    }

    void Update()
    {
        // Only check for input if the win menu is active
        if (isWinMenuActive)
        {
            // Check for keyboard/controller input to return to main menu
            // if (CheckForReturnToMenuInput())
            // {
            //     ReturnToMainMenu();
            // }

            // if (CheckForNextLevelInput())
            // {
            //     NextLevel();
            // }

            // if (CheckForReset())
            // {
            //     Reset();
            // }
        }
    }

    private bool CheckForReturnToMenuInput()
    {
        // Check via InputManager if available
        if (InputManager.Instance != null)
        {
            // Check for Menu or Interact button press
            if (InputManager.Instance.MenuPressed || InputManager.Instance.InteractPressed)
            {
                return true;
            }
        }

        // Fallback to direct input checks
        if (
            Input.GetKeyDown(KeyCode.Escape)
            || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.U)
            // || Input.GetMouseButtonDown(0)
            || Input.GetKeyDown(KeyCode.JoystickButton0)
        ) // A/Cross button on controllers
        {
            return true;
        }

        return false;
    }

    private bool CheckForReset()
    {
        if (Input.GetKeyDown(KeyCode.L) || // Keyboard
            Input.GetKeyDown(KeyCode.JoystickButton8))
        {
            return true; 
        }// Xbox Back/View, PS Share, Switch -
        return false;
    }
    
    private bool CheckForNextLevelInput()
    {
        bool down = Input.GetKeyDown(KeyCode.JoystickButton11);
        if (down)
        {
            Debug.Log("CheckForNextLevelInput");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton11)
            // || Input.GetKeyDown(KeyCode.JoystickButton0)
            || Input.GetKeyDown(KeyCode.JoystickButton1)
            || Input.GetKeyDown(KeyCode.RightArrow)
        ) // left d-pad on controller
        {
            return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (winSoundSource != null)
            {
                winSoundSource.Play();
            }

            winMenu.SetActive(true);
            isWinMenuActive = true;
            activationTime = Time.unscaledTime;

            // Pause the game
            // Time.timeScale = 0f;

            // Show cursor for menu navigation
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Update player state to UI if possible
            if (_inputContextProvider != null)
            {
                _inputContextProvider.UpdatePlayerState(PlayerState.InUI);
            }

            // Auto-select the first button if we have one and we're using a controller
            if (firstSelectedButton != null && InputManager.Instance?.UsingController == true)
            {
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(
                    firstSelectedButton
                );
            }
        }
    }

    /// <summary>
    /// Restart level
    /// </summary>
    public void Reset()
    {
        // Reset win menu state
        isWinMenuActive = false;
        winMenu.SetActive(false);

        // Reset timescale before loading a new scene
        Time.timeScale = 1f;

        // Use LevelManager if available
        if (LevelManager.Instance != null || !GameObject.Find("LevelManager").activeInHierarchy)
        {
            LevelManager.Instance.LoadScene(SceneManager.GetActiveScene().name, "CrossFade");
        }
        else
        {
            // Fallback to direct scene loading
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Reset win menu state
        isWinMenuActive = false;
        winMenu.SetActive(false);

        // Reset timescale before loading a new scene
        Time.timeScale = 1f;

        // Use LevelManager if available
        if (LevelManager.Instance != null || !GameObject.Find("LevelManager").activeInHierarchy)
        {
            LevelManager.Instance.LoadScene("MainMenu", "CrossFade");
        }
        else
        {
            // Fallback to direct scene loading
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void NextLevel()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        Debug.Log(sceneName);
        if (sceneName == "Level1" || sceneName == "winscreentesting")
        {
            // Use LevelManager if available
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadScene("Level2New", "CrossFade");
            }
            else
            {
                // Fallback to direct scene loading
                SceneManager.LoadScene("Level2New");
            }
            Time.timeScale = 1f; // unpause
        } else if (sceneName == "Level2New")
        {
            // Use LevelManager if available
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadScene("level 3", "CrossFade");
            }
            else
            {
                // Fallback to direct scene loading
                SceneManager.LoadScene("level 3");
            }
            Time.timeScale = 1f; // unpause
        }


    }

    

    /// <summary>
    /// Close the win menu without returning to the main menu
    /// </summary>
    public void Continue()
    {
        // Hide the menu
        winMenu.SetActive(false);
        isWinMenuActive = false;

        // Resume normal time
        Time.timeScale = 1f;

        // Return to gameplay state if possible
        if (_inputContextProvider != null)
        {
            _inputContextProvider.UpdatePlayerState(PlayerState.Normal);
        }

        // Hide cursor for gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
