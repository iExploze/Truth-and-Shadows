using System.Collections.Generic;
using System.Linq;
using TruthAndShadows.CheckpointSystem;
using UnityEngine;

namespace TruthAndShadows.CheckpointSystem
{
    public class CheckpointManager : MonoBehaviour
    {
        // Singleton instance
        public static CheckpointManager Instance { get; private set; }

        [Tooltip("All checkpoint transforms in the level")]
        [SerializeField]
        private List<Transform> checkpoints = new List<Transform>();

        // The currently active checkpoint (furthest one reached)
        private Transform currentCheckpoint;

        // Events
        public delegate void CheckpointReachedHandler(Transform checkpoint);
        public event CheckpointReachedHandler OnCheckpointReached;

        // Player shadow form time limit exceeded handler
        public delegate void ShadowFormTimeExceededHandler();
        public event ShadowFormTimeExceededHandler OnShadowFormTimeExceeded;

        private List<GameObject> playerObjects = new List<GameObject>();

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Cache all player-tagged objects, including inactive ones, at startup
            playerObjects = new List<GameObject>(
                GameObject.FindObjectsOfType<GameObject>(true).Where(go => go.CompareTag("Player"))
            );
            // Sort checkpoints alphabetically by name after all are registered
            SortCheckpointsAlphabetically();
            // Set the initial checkpoint to the first one in the list if any exist
            if (checkpoints.Count > 0)
            {
                currentCheckpoint = checkpoints[0];
                Debug.Log("Initial spawn point set to: " + currentCheckpoint.name);
                // Move all players to the initial spawn point
                MoveAllPlayersToCheckpoint(currentCheckpoint, true);
            }
        }

        // Move all players to a checkpoint, with optional spawn logic
        private void MoveAllPlayersToCheckpoint(Transform checkpoint, bool isSpawn = false)
        {
            if (checkpoint == null)
                return;
            foreach (var playerTransform in playerObjects.Select(playerObj => playerObj.transform))
            {
                Vector3 respawnPosition = checkpoint.position; // 1 unit above
                if (playerTransform.parent != null)
                {
                    playerTransform.localPosition =
                        respawnPosition - playerTransform.parent.position;
                    playerTransform.localRotation = Quaternion.Euler(
                        0,
                        checkpoint.rotation.eulerAngles.y,
                        0
                    );
                }
                else
                {
                    playerTransform.position = respawnPosition;
                    playerTransform.rotation = Quaternion.Euler(
                        0,
                        checkpoint.rotation.eulerAngles.y,
                        0
                    );
                }
                Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.position = playerTransform.position;
                    rb.rotation = playerTransform.rotation;
                }
                Debug.Log($"Player moved to checkpoint: {respawnPosition}");
            }
            // If this is the spawn, tell the checkpoint to disable effects
            if (isSpawn && checkpoint.TryGetComponent<Checkpoint>(out var cp))
            {
                cp.DisableEffectsForSpawn();
            }
        }

        /// <summary>
        /// Respawns all players at the current checkpoint
        /// </summary>
        public void RespawnAtCheckpoint()
        {
            if (currentCheckpoint != null)
            {
                MoveAllPlayersToCheckpoint(currentCheckpoint);
                // Play activation effects on respawn if not the initial spawn checkpoint
                if (
                    currentCheckpoint.TryGetComponent<Checkpoint>(out var cp)
                    && !cp.EffectsDisabledForSpawn
                )
                {
                    cp.ResetActivation();
                    cp.Activate();
                }
                Debug.Log("Players respawned at checkpoint: " + currentCheckpoint.name);
            }
            else
            {
                Debug.LogWarning("Cannot respawn players: no checkpoint set");
            }
        }

        /// <summary>
        /// Add a checkpoint to the list
        /// </summary>
        public void AddCheckpoint(Transform checkpointTransform)
        {
            if (!checkpoints.Contains(checkpointTransform))
            {
                checkpoints.Add(checkpointTransform);
            }
        }

        /// <summary>
        /// Call this method when the shadow form time limit is exceeded
        /// </summary>
        public void HandleShadowFormTimeout()
        {
            OnShadowFormTimeExceeded?.Invoke();
            RespawnAtCheckpoint();
        }

        // Add a public method to set the current checkpoint from Checkpoint
        public void SetCheckpoint(Transform checkpoint)
        {
            if (currentCheckpoint == checkpoint)
                return;
            currentCheckpoint = checkpoint;
            Debug.Log("Checkpoint set: " + checkpoint.name);
            Checkpoint checkpointComponent = checkpoint.GetComponent<Checkpoint>();
            if (checkpointComponent != null)
            {
                checkpointComponent.Activate();
            }
            OnCheckpointReached?.Invoke(checkpoint);
        }

        // Add a public getter for the current checkpoint
        public Transform GetCurrentCheckpoint()
        {
            return currentCheckpoint;
        }

        // Sorts the checkpoints list alphabetically by Transform name
        private void SortCheckpointsAlphabetically()
        {
            checkpoints = checkpoints.OrderBy(cp => cp.name).ToList();
            Debug.Log(
                "[CheckpointManager] Checkpoints sorted alphabetically: "
                    + string.Join(", ", checkpoints.Select(cp => cp.name))
            );
        }
    }
}
