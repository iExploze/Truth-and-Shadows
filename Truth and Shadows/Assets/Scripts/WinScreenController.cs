using System.Collections;
using System.Collections.Generic;
using TruthAndShadows.CheckpointSystem;
using TruthAndShadows.InputSystem;
using TruthAndShadows.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreenController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winScreen;
    public GameObject firstSelectedButton; // Reference to the first button to select when screen opens

    [Header("Button References")]
    public Button mainMenuButton;
    public Button resetButton;
    public Button nextLevelButton;

    [Header("Audio")]
    public AudioSource winSoundSource;

    [Header("Controller Support")]
    [SerializeField]
    private float navigationDelay = 0.3f; // Delay before controller navigation becomes active

    [Header("Level Management")]
    [SerializeField]
    private string nextLevelName = ""; // Set this in inspector or via code

    [Header("Level List System")]
    [SerializeField]
    private List<string> levelSceneNames = new List<string>
    {
        "MainMenu", // Index 0 - Main Menu
        "Intro Scene", // Index 1 - Level 1
        "Level2New", // Index 2 - Level 2
        "level 3", // Index 3 - Level 3
    };

    [SerializeField]
    [Tooltip("Current level index (0 = Main Menu, 1 = Level 1, etc.)")]
    private int currentLevelIndex = 1; // Default to Level 1

    private bool _usingController = false;
    private bool isWinScreenActive = false;
    private float activationTime;

    private InputContextProvider _inputContextProvider;
    private InputManager _inputManager;
    private Gamepad gamepad;

    void Start()
    {
        // Initialize references
        _inputContextProvider = InputContextProvider.Instance;
        _inputManager = InputManager.Instance;
        gamepad = Gamepad.current;

        // Hide win screen initially
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

        isWinScreenActive = false;

        // Setup button listeners
        SetupButtonListeners();

        // Auto-detect current level index based on active scene
        AutoDetectCurrentLevelIndex();
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    void Update()
    {
        // Update controller detection
        if (_inputManager != null)
        {
            _usingController = _inputManager.UsingController;
        }

        // Update gamepad reference
        gamepad = Gamepad.current;

        if (isWinScreenActive)
        {
            // Handle controller input for UI navigation
            HandleControllerNavigation();
            
            // Check for controller input shortcuts
            if (CheckForMainMenuInput())
            {
                ReturnToMainMenu();
            }

            if (CheckForResetInput())
            {
                ResetLevel();
            }

            if (CheckForNextLevelInput())
            {
                LoadNextLevel();
            }
            
            // Handle escape key to close win screen
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseWinScreen();
            }
        }
    }

    private void SetupButtonListeners()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetLevel);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        }
    }

    /// <summary>
    /// Shows the win screen and handles input state changes
    /// </summary>
    public void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        isWinScreenActive = true;
        activationTime = Time.unscaledTime;

        // Play win sound
        if (winSoundSource != null)
        {
            winSoundSource.Play();
        }

        // Update player state to UI
        if (_inputContextProvider != null)
        {
            _inputContextProvider.UpdatePlayerState(PlayerState.InUI);
        }

        // Set first button as selected for controller navigation
        if (firstSelectedButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }

        // Show cursor for mouse navigation
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Win screen activated - Player state set to InUI");
    }

    /// <summary>
    /// Hides the win screen (if needed for some reason)
    /// </summary>
    public void HideWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

        isWinScreenActive = false;

        // Restore normal player state
        if (_inputContextProvider != null)
        {
            _inputContextProvider.UpdatePlayerState(PlayerState.Normal);
        }
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu...");

        // Reset time scale in case it was modified
        Time.timeScale = 1f;

        string mainMenuScene = GetLevelSceneName(0); // Index 0 is always main menu

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene(mainMenuScene, "CrossFade");
        }
        else
        {
            // Fallback to direct scene loading
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    /// <summary>
    /// Reset/restart the current level
    /// </summary>
    public void ResetLevel()
    {
        Debug.Log("Resetting level...");

        // Reset time scale in case it was modified
        Time.timeScale = 1f;

        // Use checkpoint system if available
        if (CheckpointManager.Instance != null)
        {
            Debug.Log("Using CheckpointManager to reset level");

            // Reset all interactables to their spawn positions
            CheckpointManager.Instance.ResetAllInteractablesToSpawn();

            // Respawn players at the checkpoint
            CheckpointManager.Instance.RespawnAtCheckpoint();

            // Hide win screen after respawning
            HideWinScreen();
        }
        else
        {
            // Fallback: reload the current scene
            Debug.Log("No CheckpointManager found, reloading scene");

            // Try to use level management system first
            string currentScene = GetCurrentLevelSceneName();
            string activeSceneName = SceneManager.GetActiveScene().name;

            // Verify the level management system matches the current scene
            if (!string.IsNullOrEmpty(currentScene) && currentScene == activeSceneName)
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.LoadScene(currentScene, "CrossFade");
                }
                else
                {
                    SceneManager.LoadScene(currentScene);
                }
            }
            else
            {
                // Fallback to active scene if level management doesn't match
                Debug.LogWarning(
                    $"Level management mismatch! Current index points to '{currentScene}' but active scene is '{activeSceneName}'. Using active scene."
                );

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.LoadScene(activeSceneName, "CrossFade");
                }
                else
                {
                    SceneManager.LoadScene(activeSceneName);
                }
            }
        }
    }

    /// <summary>
    /// Load the next level
    /// </summary>
    public void LoadNextLevel()
    {
        Debug.Log("Loading next level...");

        // Reset time scale in case it was modified
        Time.timeScale = 1f;

        string levelToLoad = "";

        // Use the specified next level name if provided
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            levelToLoad = nextLevelName;
        }
        else
        {
            // Use level list system to get next level
            int nextLevelIndex = GetNextLevelIndex();
            if (nextLevelIndex >= 0)
            {
                levelToLoad = GetLevelSceneName(nextLevelIndex);
                Debug.Log($"Loading next level from list: {levelToLoad} (index {nextLevelIndex})");
            }
            else
            {
                // Try the old automatic determination method as fallback
                levelToLoad = GetNextLevelNameLegacy();
                Debug.Log($"Using legacy level detection: {levelToLoad}");
            }
        }

        if (!string.IsNullOrEmpty(levelToLoad))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadScene(levelToLoad, "CrossFade");
            }
            else
            {
                // Fallback to direct scene loading
                SceneManager.LoadScene(levelToLoad);
            }
        }
        else
        {
            Debug.LogWarning("No next level found! Returning to main menu.");
            // Fallback: return to main menu
            ReturnToMainMenu();
        }
    }

    /// <summary>
    /// Get level scene name by index from the level list
    /// </summary>
    /// <param name="levelIndex">Level index (0 = Main Menu, 1 = Level 1, etc.)</param>
    /// <returns>Scene name for the specified level index</returns>
    private string GetLevelSceneName(int levelIndex)
    {
        if (levelSceneNames != null && levelIndex >= 0 && levelIndex < levelSceneNames.Count)
        {
            return levelSceneNames[levelIndex];
        }

        Debug.LogWarning($"Invalid level index {levelIndex}! Using fallback.");

        // Fallback for common levels
        switch (levelIndex)
        {
            case 0:
                return "MainMenu";
            case 1:
                return "Intro Scene";
            case 2:
                return "Level2New";
            case 3:
                return "level 3";
            default:
                return "MainMenu"; // Fallback to main menu
        }
    }

    /// <summary>
    /// Get the next level index based on current level index
    /// </summary>
    /// <returns>Next level index, or -1 if no next level exists</returns>
    private int GetNextLevelIndex()
    {
        int nextIndex = currentLevelIndex + 1;

        // Check if next level exists in our list
        if (levelSceneNames != null && nextIndex < levelSceneNames.Count)
        {
            return nextIndex;
        }

        return -1; // No next level
    }

    /// <summary>
    /// Automatically detect the current level index based on the active scene
    /// </summary>
    private void AutoDetectCurrentLevelIndex()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;

        if (levelSceneNames != null)
        {
            for (int i = 0; i < levelSceneNames.Count; i++)
            {
                if (levelSceneNames[i] == activeSceneName)
                {
                    currentLevelIndex = i;
                    Debug.Log(
                        $"Auto-detected current level index: {currentLevelIndex} (scene: {activeSceneName})"
                    );
                    return;
                }
            }
        }

        // Fallback: try to guess based on scene name patterns
        if (activeSceneName.ToLower().Contains("mainmenu"))
        {
            currentLevelIndex = 0;
        }
        else if (
            activeSceneName.ToLower().Contains("intro")
            || activeSceneName.ToLower().Contains("level1")
        )
        {
            currentLevelIndex = 1;
        }
        else if (activeSceneName.ToLower().Contains("level2"))
        {
            currentLevelIndex = 2;
        }
        else if (activeSceneName.ToLower().Contains("level3"))
        {
            currentLevelIndex = 3;
        }
        else
        {
            Debug.LogWarning(
                $"Could not auto-detect level index for scene '{activeSceneName}'. Using default value {currentLevelIndex}."
            );
        }

        Debug.Log(
            $"Auto-detected current level index: {currentLevelIndex} (scene: {activeSceneName})"
        );
    }

    /// <summary>
    /// Set the current level index
    /// </summary>
    /// <param name="levelIndex">Level index to set</param>
    public void SetCurrentLevelIndex(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        Debug.Log($"Current level index set to: {currentLevelIndex}");
    }

    /// <summary>
    /// Legacy method: Automatically determine the next level name based on current scene
    /// </summary>
    private string GetNextLevelNameLegacy()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Simple logic to determine next level - customize as needed
        if (currentSceneName.ToLower().Contains("level1"))
        {
            return "Level2";
        }
        else if (currentSceneName.ToLower().Contains("level2"))
        {
            return "Level3";
        }
        else if (currentSceneName.ToLower().Contains("level3"))
        {
            return "Level4";
        }
        // Add more levels as needed

        return ""; // Return empty if no next level found
    }

    /// <summary>
    /// Get the current level scene name
    /// </summary>
    /// <returns>Current level scene name</returns>
    public string GetCurrentLevelSceneName()
    {
        return GetLevelSceneName(currentLevelIndex);
    }

    /// <summary>
    /// Check if there is a next level available
    /// </summary>
    /// <returns>True if next level exists, false otherwise</returns>
    public bool HasNextLevel()
    {
        return GetNextLevelIndex() >= 0;
    }

    /// <summary>
    /// Get the name of the next level (if available)
    /// </summary>
    /// <returns>Next level scene name, or empty string if no next level</returns>
    public string GetNextLevelSceneName()
    {
        int nextIndex = GetNextLevelIndex();
        return nextIndex >= 0 ? GetLevelSceneName(nextIndex) : "";
    }

    /// <summary>
    /// Get the total number of levels (including main menu)
    /// </summary>
    /// <returns>Total level count</returns>
    public int GetTotalLevelCount()
    {
        return levelSceneNames != null ? levelSceneNames.Count : 0;
    }

    /// <summary>
    /// Set the next level name programmatically
    /// </summary>
    public void SetNextLevelName(string levelName)
    {
        nextLevelName = levelName;
    }

    /// <summary>
    /// Print level management debug info
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugPrintLevelInfo()
    {
        Debug.Log("=== WinScreenController Level Management Debug Info ===");
        Debug.Log($"Current Level Index: {currentLevelIndex}");
        Debug.Log($"Current Level Scene: {GetCurrentLevelSceneName()}");
        Debug.Log($"Active Scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"Has Next Level: {HasNextLevel()}");
        Debug.Log($"Next Level Scene: {GetNextLevelSceneName()}");
        Debug.Log($"Total Level Count: {GetTotalLevelCount()}");
        Debug.Log(
            $"Next Level Name Override: {(string.IsNullOrEmpty(nextLevelName) ? "None" : nextLevelName)}"
        );

        if (levelSceneNames != null)
        {
            Debug.Log("Level List:");
            for (int i = 0; i < levelSceneNames.Count; i++)
            {
                Debug.Log(
                    $"  [{i}] {levelSceneNames[i]}" + (i == currentLevelIndex ? " <- CURRENT" : "")
                );
            }
        }
        Debug.Log("=== End Debug Info ===");
    }

    #region Input Detection Methods

    private bool CheckForMainMenuInput()
    {
        // Prevent input immediately after activation
        if (Time.unscaledTime - activationTime < navigationDelay)
            return false;

        // Check InputManager first
        if (_inputManager != null && _inputManager.MenuPressed)
        {
            return true;
        }

        if (
            Input.GetKeyDown(KeyCode.Escape)
            || Input.GetKeyDown(KeyCode.U)
            || Input.GetKeyDown(KeyCode.JoystickButton7)
        )
        {
            return true;
        }

        // Check gamepad specifically
        if (gamepad != null)
        {
            if (gamepad.startButton.wasPressedThisFrame || gamepad.selectButton.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckForResetInput()
    {
        // Prevent input immediately after activation
        if (Time.unscaledTime - activationTime < navigationDelay)
            return false;

        // Check InputManager first
        if (_inputManager != null && _inputManager.ResetPressed)
        {
            return true;
        }

        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.JoystickButton8))
        {
            return true;
        }

        // Check gamepad specifically
        if (gamepad != null)
        {
            if (gamepad.selectButton.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckForNextLevelInput()
    {
        // Prevent input immediately after activation
        if (Time.unscaledTime - activationTime < navigationDelay)
            return false;

        if (
            Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.RightArrow)
            || Input.GetKeyDown(KeyCode.JoystickButton1)
            || Input.GetKeyDown(KeyCode.JoystickButton11)
        )
        {
            return true;
        }

        // Check gamepad specifically
        if (gamepad != null)
        {
            if (
                gamepad.buttonSouth.wasPressedThisFrame
                || gamepad.rightShoulder.wasPressedThisFrame
                || gamepad.dpad.right.wasPressedThisFrame
            )
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    /// <summary>
    /// Handle controller navigation through UI buttons
    /// </summary>
    private void HandleControllerNavigation()
    {
        if (gamepad == null || !gamepad.wasUpdatedThisFrame) return;

        // Navigate through buttons using D-pad or left stick
        if (gamepad.dpad.up.wasPressedThisFrame || gamepad.leftStick.up.wasPressedThisFrame)
        {
            NavigateButtons(-1); // Move up
        }
        else if (gamepad.dpad.down.wasPressedThisFrame || gamepad.leftStick.down.wasPressedThisFrame)
        {
            NavigateButtons(1); // Move down
        }

        // Activate selected button
        if (gamepad.buttonSouth.wasPressedThisFrame) // A button
        {
            ActivateSelectedButton();
        }

        // Close win screen with B button
        if (gamepad.buttonEast.wasPressedThisFrame) // B button
        {
            CloseWinScreen();
        }
    }

    /// <summary>
    /// Navigate through the UI buttons
    /// </summary>
    /// <param name="direction">-1 for up, 1 for down</param>
    private void NavigateButtons(int direction)
    {
        Button[] buttons = { nextLevelButton, resetButton, mainMenuButton };
        
        // Find currently selected button
        int currentIndex = -1;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && EventSystem.current.currentSelectedGameObject == buttons[i].gameObject)
            {
                currentIndex = i;
                break;
            }
        }

        // If no button is selected, select the first one
        if (currentIndex == -1)
        {
            if (buttons[0] != null && buttons[0].interactable)
            {
                EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            }
            return;
        }

        // Navigate to next/previous button
        int nextIndex = (currentIndex + direction) % buttons.Length;
        if (nextIndex < 0) nextIndex = buttons.Length - 1;

        // Find next interactable button
        int attempts = 0;
        while (attempts < buttons.Length)
        {
            if (buttons[nextIndex] != null && buttons[nextIndex].interactable)
            {
                EventSystem.current.SetSelectedGameObject(buttons[nextIndex].gameObject);
                break;
            }
            
            nextIndex = (nextIndex + direction) % buttons.Length;
            if (nextIndex < 0) nextIndex = buttons.Length - 1;
            attempts++;
        }
    }

    /// <summary>
    /// Activate the currently selected button
    /// </summary>
    private void ActivateSelectedButton()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            if (selectedButton != null && selectedButton.interactable)
            {
                selectedButton.onClick.Invoke();
            }
        }
    }

    /// <summary>
    /// Close/hide the win screen and return to gameplay
    /// </summary>
    public void CloseWinScreen()
    {
        HideWinScreen();
    }

    /// <summary>
    /// Call this method when the player wins (e.g., from CollectShard or other win conditions)
    /// </summary>
    public void TriggerWin()
    {
        ShowWinScreen();
    }

    void OnEnable()
    {
        activationTime = Time.unscaledTime;
    }

    void OnDisable()
    {
        // Clean up any ongoing processes
        isWinScreenActive = false;
    }
}
