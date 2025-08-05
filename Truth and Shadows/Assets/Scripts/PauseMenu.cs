using System.Collections;
using System.Collections.Generic;
using TruthAndShadows.InputSystem;
using TruthAndShadows.Player;
using TruthAndShadows.CheckpointSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;

    [Header("UI Navigation")]
    [SerializeField]
    private GameObject firstSelectedButton; // Reference to the first button to select when menu opens

    [Header("Controller Support")]
    [SerializeField]
    private float navigationDelay = 0.3f; // Delay before controller navigation becomes active

    private bool _usingController = false;
    private float lastMenuActionTime;
    private PlayerState _previousPlayerState = PlayerState.Normal; // Store previous state for restoration

    private InputContextProvider _inputContextProvider;
    private InputManager _inputManager;

    // Start is called before the first frame update
    void Start()
    {
        if (pauseMenu == null)
        {
            pauseMenu = GameObject.Find("PauseMenu");
        }
        pauseMenu.SetActive(false);
        isPaused = false;
        lastMenuActionTime = Time.unscaledTime;
        _inputContextProvider = InputContextProvider.Instance;
        _inputManager = InputManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if we can access the menu based on permissions and player state
        bool canAccessMenu = false;

        // Get input permissions if available
        if (_inputContextProvider != null)
        {
            // We can access the menu if we have permission AND
            // we're either already in UI (isPaused) or in a state that allows opening the menu
            PlayerState currentState = _inputContextProvider.CurrentPlayerState;
            canAccessMenu =
                _inputContextProvider.CanMenu && (isPaused || IsGameplayState(currentState));
        }

        // Check for input through the InputManager
        if (canAccessMenu)
        {
            // Use InputManager to check for menu button press
            bool menuButtonPressed = false;

            // Get from InputManager if available
            if (_inputManager != null)
            {
                menuButtonPressed = _inputManager.MenuPressed;
                // Update controller detection from InputManager
                _usingController = _inputManager.UsingController;
            }
            // Fallback to direct input check
            else
            {
                menuButtonPressed = Input.GetKeyUp(KeyCode.Escape);
            }

            // Handle menu toggle
            if (menuButtonPressed)
            {
                lastMenuActionTime = Time.unscaledTime;

                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        // Check if we can pause based on player state
        if (_inputContextProvider == null)
        {
            Debug.LogWarning("Cannot pause - no InputContextProvider found (not in a level)");
            return;
        }

        PlayerState currentState = _inputContextProvider.CurrentPlayerState;

        // Only allow pausing if we're not already in UI and in a valid state
        if (currentState == PlayerState.InUI || currentState == PlayerState.Cutscene)
        {
            Debug.LogWarning("Cannot pause - no InputContextProvider found (not in a level)");
            return;
        }
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // Set the first button as selected for controller navigation
        if (firstSelectedButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }

        // Show cursor for mouse navigation
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Save the time when we paused (for delaying controller input)
        lastMenuActionTime = Time.unscaledTime;

        Debug.Log("Game paused - Player state set to InUI");
    }

    public void ResumeGame()
    {
        // Don't resume if not actually paused
        if (!isPaused)
            return;

        if (pauseMenu == null)
        {
            pauseMenu = GameObject.Find("PauseMenu");
        }
        
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        if (_inputContextProvider != null)
        {
            // If the previous state was valid (not UI, cutscene, or disabled), restore it
            if (
                _previousPlayerState
                is not (PlayerState.InUI or PlayerState.Cutscene or PlayerState.Disabled)
            )
            {
                // Return to the previous state if it's valid
                _inputContextProvider.UpdatePlayerState(_previousPlayerState);
                Debug.Log($"Game resumed - Player state restored to {_previousPlayerState}");
            }
            else
            {
                // Fallback to normal state
                _inputContextProvider.UpdatePlayerState(PlayerState.Normal);
                Debug.Log("Game resumed - Player state reset to Normal");
            }
        }
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        // Soft reset: respawn at the most recent checkpoint instead of quitting
        if (CheckpointManager.Instance != null)
        {
            Debug.Log("Soft reset: Resetting interactables and respawning at checkpoint");
            
            // First, reset all interactables to their spawn positions
            CheckpointManager.Instance.ResetAllInteractablesToSpawn();
            
            // Then respawn players at the checkpoint
            CheckpointManager.Instance.RespawnAtCheckpoint();
            
            // Resume the game after respawning
            ResumeGame();
        }
        else
        {
            Debug.LogWarning("CheckpointManager not found! Cannot perform soft reset.");
            // Fallback to quitting if no checkpoint system
            Application.Quit();
        }
    }

    void OnEnable()
    {
        lastMenuActionTime = Time.unscaledTime;
    }

    private bool IsGameplayState(PlayerState state)
    {
        return state == PlayerState.Normal
            || state == PlayerState.Aiming
            || state == PlayerState.Pickup
            || state == PlayerState.Interacting;
    }
}
