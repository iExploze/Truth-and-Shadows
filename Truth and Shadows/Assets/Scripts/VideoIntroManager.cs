using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoIntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject continueButton;

    // Start is called before the first frame update
    void Start()
    {
        continueButton.SetActive(false); // Hide button initially
        videoPlayer.loopPointReached += OnVideoFinished; // Register callback
    }
    void OnVideoFinished(VideoPlayer vp)
    {
        continueButton.SetActive(true); // Show button when video ends
    }
    public void OnContinueButtonClicked()
    {
        // Load the next level or scene here
        SceneManager.LoadScene("Level1");
    }
}
