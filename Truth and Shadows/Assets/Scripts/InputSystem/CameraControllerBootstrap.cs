using UnityEngine;

namespace TruthAndShadows.InputSystem
{
    /// <summary>
    /// This class ensures that the CameraControllerConfig component is always available in the scene.
    /// It automatically creates a GameObject with the component if one doesn't already exist.
    /// </summary>
    [DefaultExecutionOrder(-150)] // Run before most scripts to ensure camera config is available early
    public class CameraControllerBootstrap : MonoBehaviour
    {
        private static CameraControllerBootstrap _instance;
        public static CameraControllerBootstrap Instance => _instance;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            EnsureCameraControllerExists();
        }

        /// <summary>
        /// Creates a CameraControllerConfig if one doesn't already exist in the scene
        /// </summary>
        public void EnsureCameraControllerExists()
        {
            var existingAltConfig = FindObjectOfType<CameraControllerConfig>();

            if (existingAltConfig == null)
            {
                GameObject cameraConfigObject = new GameObject("CameraControllerConfig");
                var config = cameraConfigObject.AddComponent<CameraControllerConfig>();

                // Set as a child of this bootstrap to keep hierarchy clean
                cameraConfigObject.transform.SetParent(transform);

                // Configure all cameras immediately
                config.ConfigureAllCameras();
            }
        }
    }
}
