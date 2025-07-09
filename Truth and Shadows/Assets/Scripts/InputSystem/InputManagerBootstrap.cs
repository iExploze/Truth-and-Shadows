using UnityEngine;
using Input = UnityEngine.Input;

namespace TruthAndShadows.InputSystem
{
    /// <summary>
    /// Ensures an InputManager and InputContextProvider exist in the scene.
    /// Add this script to a GameObject in your startup scene.
    /// </summary>
    [DefaultExecutionOrder(-200)] // Run early
    public class InputManagerBootstrap : MonoBehaviour
    {
        private static InputManagerBootstrap _instance;
        public static InputManagerBootstrap Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            EnsureInputManagerExists();
        }

        public void EnsureInputManagerExists()
        {
            if (InputManager.Instance == null)
            {
                var inputManagerObject = new GameObject("InputManager");
                inputManagerObject.AddComponent<InputManager>();
            }

            if (InputContextProvider.Instance == null)
            {
                var inputContextProviderObject = new GameObject("InputContextProvider");
                inputContextProviderObject.AddComponent<InputContextProvider>();
            }
        }
    }
}