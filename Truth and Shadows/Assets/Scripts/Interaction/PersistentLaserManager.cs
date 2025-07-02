using UnityEngine;
using UnityEngine.SceneManagement;

namespace TruthAndShadows.Interaction
{
    public class PersistentLaserManager : MonoBehaviour
    {
        private static PersistentLaserManager _instance;

        private LaserBetweenPoints persistentLaserInstance;
        private Transform persistentLaserEndPointDummy;
        private GameObject persistentLaserStartGO;
        private bool isInitialized = false;
        public static PersistentLaserManager Instance
        {
            get { return _instance; }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            Initialize();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isInitialized = false;
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized)
                return;

            LaserBetweenPoints laserPrefab = null;
            GameObject ultraHandObj = GameObject.Find("UltraHand");
            if (ultraHandObj != null)
            {
                laserPrefab = ultraHandObj.GetComponent<LaserBetweenPoints>();
            }

            if (laserPrefab != null)
            {
                persistentLaserInstance = Instantiate(laserPrefab);
                persistentLaserInstance.transform.SetParent(transform, true);
                persistentLaserInstance.SetLaserActive(false);

                GameObject dummy = new GameObject("PersistentLaserEndPoint");
                persistentLaserEndPointDummy = dummy.transform;
                persistentLaserEndPointDummy.SetParent(transform, true);

                persistentLaserStartGO = GameObject.Find("finger_1_r_1_joint");
                if (persistentLaserStartGO == null)
                {
                    var playerObj = GameObject.FindWithTag("Player");
                    if (playerObj != null)
                        persistentLaserStartGO = playerObj;
                }

                if (persistentLaserStartGO != null)
                {
                    persistentLaserInstance.UpdateLaserGameObjects(
                        persistentLaserStartGO,
                        persistentLaserEndPointDummy.gameObject,
                        false
                    );
                }
            }
            isInitialized = true;
        }

        void Update()
        {
            if (!isInitialized || persistentLaserInstance == null)
                return;

            var heldInteractable = InteractableBase.CurrentlyHeldInteractable;

            if (heldInteractable != null && heldInteractable.IsPickedUp)
            {
                UpdateLaserForInteractable(heldInteractable);
            }
            else
            {
                persistentLaserInstance.SetLaserActive(false);
            }
        }

        private void UpdateLaserForInteractable(InteractableBase interactable)
        {
            if (persistentLaserEndPointDummy == null || interactable.InteractableCollider == null)
            {
                persistentLaserInstance.SetLaserActive(false);
                return;
            }

            // Start point: player's hand (persistentLaserStartGO)
            // End point: center of interactable's transform (not player's hand)
            Vector3 rayOrigin =
                persistentLaserStartGO != null
                    ? persistentLaserStartGO.transform.position
                    : interactable.transform.position;
            Vector3 rayDir =
                persistentLaserStartGO != null
                    ? (interactable.transform.position - persistentLaserStartGO.transform.position).normalized
                    : interactable.transform.forward;
            float maxLaserDist = 10f;

            RaycastHit hit;
            bool hitSomething = Physics.Raycast(rayOrigin, rayDir, out hit, maxLaserDist);

            Vector3 endPoint;
            if (hitSomething)
            {
                endPoint = hit.point;
            }
            else
            {
                endPoint = interactable.transform.position; // Always point to the interactable's center if no hit
            }

            persistentLaserEndPointDummy.position = endPoint;

            persistentLaserInstance.UpdateLaserGameObjects(
                persistentLaserStartGO,
                persistentLaserEndPointDummy.gameObject,
                false
            );

            bool isOccluded = hitSomething && hit.collider != interactable.InteractableCollider;
            persistentLaserInstance.SetLaserActive(!isOccluded);
        }
    }
}
