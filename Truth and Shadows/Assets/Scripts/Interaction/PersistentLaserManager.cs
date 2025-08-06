using System.Collections;
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
            StartCoroutine(DelayedInitialize());
        }

        private IEnumerator DelayedInitialize()
        {
            yield return new WaitForSeconds(0.1f); // Wait for scene to fully load
            Initialize();
            
            // Retry if failed
            if (!isInitialized)
            {
                yield return new WaitForSeconds(0.5f);
                Initialize();
            }
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

                // Find the player's finger joint specifically, not any finger joint
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    // Look for the finger joint within the player hierarchy
                    Transform fingerJoint = playerObj.transform.Find("finger_1_r_1_joint");
                    if (fingerJoint == null)
                    {
                        // If not a direct child, search recursively
                        fingerJoint = FindChildRecursive(playerObj.transform, "finger_1_r_1_joint");
                    }
                    
                    if (fingerJoint != null)
                    {
                        persistentLaserStartGO = fingerJoint.gameObject;
                    }
                    else
                    {
                        // Fallback to player object itself
                        persistentLaserStartGO = playerObj;
                    }
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

        // Helper method to search recursively through child transforms
        private Transform FindChildRecursive(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;
                
                Transform found = FindChildRecursive(child, childName);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
