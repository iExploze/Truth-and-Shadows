using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    // Singleton instance
    public static CheckpointManager Instance { get; private set; }

    [Tooltip("Distance at which a checkpoint is considered reached")]
    [SerializeField]
    private float checkpointReachDistance = 3f;

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
        playerObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        // Set the initial checkpoint to the first one in the list if any exist
        if (checkpoints.Count > 0)
        {
            currentCheckpoint = checkpoints[0];
            Debug.Log("Initial spawn point set to: " + currentCheckpoint.name);

            // Move all players to the initial spawn point
            MoveAllPlayersToCheckpoint(currentCheckpoint);
        }
    }

    private void Update()
    {
        CheckForCheckpointReached();
    }

    private void CheckForCheckpointReached()
    {
        if (playerObjects.Count == 0)
            return;

        int currentIndex = checkpoints.IndexOf(currentCheckpoint);

        // Check distance from any player object to checkpoints
        foreach (GameObject playerObj in playerObjects)
        {
            foreach (Transform checkpoint in checkpoints)
            {
                int newIndex = checkpoints.IndexOf(checkpoint);
                if (newIndex > currentIndex || currentCheckpoint == null)
                {
                    currentCheckpoint = checkpoint;
                    Debug.Log("New checkpoint reached: " + checkpoint.name);
                    Checkpoint checkpointComponent = checkpoint.GetComponent<Checkpoint>();
                    if (checkpointComponent != null)
                    {
                        checkpointComponent.Activate();
                    }
                    OnCheckpointReached?.Invoke(checkpoint);
                }
            }
        }
    }

    // Move all players to a checkpoint
    private void MoveAllPlayersToCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null)
            return;
        foreach (var playerObj in playerObjects)
        {
            var playerTransform = playerObj.transform;
            if (playerTransform.parent != null)
            {
                playerTransform.localPosition =
                    checkpoint.position - playerTransform.parent.position;
                playerTransform.localRotation = checkpoint.rotation;
            }
            else
            {
                playerTransform.position = checkpoint.position;
                playerTransform.rotation = checkpoint.rotation;
            }
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = playerTransform.position;
                rb.rotation = playerTransform.rotation;
            }
            Debug.Log($"Player moved to checkpoint: {checkpoint.position}");
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
}
