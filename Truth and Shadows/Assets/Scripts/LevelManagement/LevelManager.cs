using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GameObject transitionsContainer;
    private SceneTransition[] transitions;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
    }

    public void LoadScene(string sceneName, string transitionName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }
    
    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        Debug.Log(GameObject.Find("CheckpointManager"));
        //GameObject.Find("CheckpointManager").SetActive(false);
        if (GameObject.Find("CheckpointManager") != null)
        {
            // Debug.Log("AAAAAA");
            GameObject.Find("CheckpointManager").SetActive(false);
        }
        // 1 line for loop. loop thru transitions until name matches transitionName
        SceneTransition transition = transitions.First(t => t.name == transitionName);

        AsyncOperation blackScreen = SceneManager.LoadSceneAsync("Loading");
        
        
        blackScreen.allowSceneActivation = false;
        // wait for the scene to load
        while (!blackScreen.isDone)
        {
            if (blackScreen.progress >= 0.9f)
            {
                break;
            }

            yield return null;
        }
        
 
        blackScreen.allowSceneActivation = true;
        // yield return new WaitForSeconds(1f);
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        
        // SceneManager.UnloadScene("Loading");
        scene.allowSceneActivation = true;
        while (!scene.isDone)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        
        // Debug.Log(GameObject.Find("CheckpointManager"));
        // //GameObject.Find("CheckpointManager").SetActive(false);
        // if (GameObject.Find("CheckpointManager") != null)
        // {
        //     Debug.Log("AAAAAA");
        //     GameObject.Find("CheckpointManager").SetActive(false);
        // }
    }
}
