using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    // Singleton instance
    public static CheckpointManager Instance { get; private set; }

    [Tooltip("Distance at which a checkpoint is considered reached")]
    [SerializeField]
    private float checkpointReachDistance = 1f;

    [Tooltip("All checkpoint transforms in the level")]
    [SerializeField]
    private List<Transform> checkpoints = new List<Transform>();

    // The currently active checkpoint (furthest one reached)
    private Transform currentCheckpoint;

    // The player transform to respawn
    [SerializeField]
    private Transform playerTransform;

    // Events
    public delegate void CheckpointReachedHandler(Transform checkpoint);
    public event CheckpointReachedHandler OnCheckpointReached;

    // Player shadow form time limit exceeded handler
    public delegate void ShadowFormTimeExceededHandler();
    public event ShadowFormTimeExceededHandler OnShadowFormTimeExceeded;

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
    }    private void Start()
    {
        // Set the initial checkpoint to the first one in the list if any exist
        if (checkpoints.Count > 0)
        {
            currentCheckpoint = checkpoints[0];
            Debug.Log("Initial spawn point set to: " + currentCheckpoint.name);

            // Move all player objects to the initial spawn point
            MovePlayerToCheckpoint(currentCheckpoint);
        }
    }

    private void Update()
    {
        CheckForCheckpointReached();
    }    private void CheckForCheckpointReached()
    {
        if (checkpoints.Count == 0)
            return;

        // Find all GameObjects with the "Player" tag
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        
        if (playerObjects.Length == 0)
            return;

        // Check distance from any player object to checkpoints
        foreach (GameObject playerObj in playerObjects)
        {
            foreach (Transform checkpoint in checkpoints)
            {
                float distance = Vector3.Distance(playerObj.transform.position, checkpoint.position);

                // If player is within reach distance of a checkpoint
                if (distance <= checkpointReachDistance)
                {
                    // Only update if this is a new checkpoint
                    if (currentCheckpoint != checkpoint)
                    {
                        // Find the index of the current and new checkpoints
                        int currentIndex = checkpoints.IndexOf(currentCheckpoint);
                        int newIndex = checkpoints.IndexOf(checkpoint); // Only update if the new checkpoint is further in the list than the current one
                        if (newIndex > currentIndex || currentCheckpoint == null)
                        {
                            currentCheckpoint = checkpoint;
                            Debug.Log("New checkpoint reached: " + checkpoint.name);

                            // Activate checkpoint visual effect if it has a Checkpoint component
                            Checkpoint checkpointComponent = checkpoint.GetComponent<Checkpoint>();
                            if (checkpointComponent != null)
                            {
                                checkpointComponent.Activate();
                            }

                            OnCheckpointReached?.Invoke(checkpoint); // Invoke the event
                            return; // Exit early once a checkpoint is reached
                        }
                    }
                }
            }
        }
    }// Helper method to move player to a checkpoint consistently
    private void MovePlayerToCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null)
            return;

        // Find all GameObjects with the "Player" tag
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        
        if (playerObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects with 'Player' tag found!");
            return;
        }

        foreach (GameObject playerObj in playerObjects)
        {
            Transform playerTransform = playerObj.transform;
            
            if (playerTransform.parent != null)
            {
                // Convert checkpoint world position to local position relative to the player's parent
                playerTransform.localPosition = playerTransform.parent.InverseTransformPoint(checkpoint.position);

                // Convert checkpoint world rotation to local rotation relative to the player's parent
                playerTransform.localRotation = Quaternion.Inverse(playerTransform.parent.rotation) * checkpoint.rotation;
            }
            else
            {
                // No parent, directly set world position
                playerTransform.position = checkpoint.position;
                playerTransform.rotation = checkpoint.rotation;
            }

            // Also update Rigidbody if present
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = playerTransform.position;
                rb.rotation = playerTransform.rotation;
            }

            Debug.Log($"Player object '{playerObj.name}' moved to checkpoint: {checkpoint.position}");
        }
    }    /// <summary>
    /// Respawns the player at the current checkpoint
    /// </summary>
    public void RespawnAtCheckpoint()
    {
        if (currentCheckpoint != null)
        {
            MovePlayerToCheckpoint(currentCheckpoint);
            Debug.Log("Player respawned at checkpoint: " + currentCheckpoint.name);
        }
        else
        {
            Debug.LogWarning("Cannot respawn player: no checkpoint set");
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
    /// Set the player transform
    /// </summary>
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    /// <summary>
    /// Call this method when the shadow form time limit is exceeded
    /// </summary>
    public void HandleShadowFormTimeout()
    {
        // Invoke the event so other systems can react
        OnShadowFormTimeExceeded?.Invoke();

        // Respawn the player at the last checkpoint
        RespawnAtCheckpoint();
    }
}
