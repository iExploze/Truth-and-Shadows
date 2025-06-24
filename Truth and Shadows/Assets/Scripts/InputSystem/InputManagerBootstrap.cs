using UnityEngine;
using Input = UnityEngine.Input;

namespace TruthAndShadows.InputSystem
{
    /// <summary>
    /// This MonoBehaviour creates an InputManager instance when the game starts.
    /// Add this script to a GameObject in your startup scene to ensure the InputManager is available.
    /// </summary>
    public class InputManagerBootstrap : MonoBehaviour
    {
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        private void Awake()
        {
            if (InputManager.Instance == null)
            {
                GameObject inputManagerObject = new GameObject("InputManager");
                inputManagerObject.AddComponent<InputManager>();

                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(inputManagerObject);
                }

                Debug.Log("InputManager created by bootstrap");
            }
        }
    }
}
