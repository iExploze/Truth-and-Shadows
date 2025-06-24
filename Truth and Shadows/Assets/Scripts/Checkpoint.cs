using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Set this to true for this checkpoint to be auto-registered with the CheckpointManager on start")]
    [SerializeField]
    private bool autoRegister = true;

    [Tooltip("Optional visual effect to play when checkpoint is activated")]
    [SerializeField]
    private GameObject activationEffect;

    private void Start()
    {
        // Auto-register with checkpoint manager if enabled
        if (autoRegister && CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.AddCheckpoint(transform);
        }
    }

    // Called by the CheckpointManager when this checkpoint becomes the active checkpoint
    public void Activate()
    {
        if (activationEffect != null)
        {
            GameObject effect = Instantiate(activationEffect, transform.position, transform.rotation);
            Destroy(effect, 3f); // Clean up the effect after 3 seconds
        }
    }
}
