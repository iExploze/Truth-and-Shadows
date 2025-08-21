using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class VideoIntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject continueButton;

    private Gamepad gamepad;
    private bool videoFinished = false;

    // Start is called before the first frame update
    void Start()
    {
        continueButton.SetActive(false); // Hide button initially
        videoPlayer.loopPointReached += OnVideoFinished; // Register callback
        
        // Get the current gamepad
        gamepad = Gamepad.current;
        
        // Set up button for controller navigation when it becomes active
        if (continueButton != null)
        {
            var button = continueButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnContinueButtonClicked);
            }
        }
    }

    void Update()
    {
        // Only process controller input if video has finished and button is active
        if (videoFinished && continueButton.activeInHierarchy && gamepad != null)
        {
            // Check for ANY input on the controller to continue
            if (gamepad.wasUpdatedThisFrame)
            {
                OnContinueButtonClicked();
            }
        }
        
        // Also check for any keyboard input
        if (videoFinished && continueButton.activeInHierarchy && Input.anyKeyDown)
        {
            OnContinueButtonClicked();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        videoFinished = true;
        continueButton.SetActive(true); // Show button when video ends
        
        // Set the continue button as selected for controller navigation
        if (continueButton != null)
        {
            EventSystem.current.SetSelectedGameObject(continueButton);
        }
    }
    public void OnContinueButtonClicked()
    {
        // Load the next level using LevelManager (consistent with MainMenuController)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene("Level1", "CrossFade");
        }
        else
        {
            // Fallback to direct scene loading if LevelManager is not available
            SceneManager.LoadScene("Level1");
        }
    }
}
