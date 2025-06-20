using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public CanvasGroup SettingsPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
