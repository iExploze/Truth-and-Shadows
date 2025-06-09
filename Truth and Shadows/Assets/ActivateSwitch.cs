using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ActivateSwitch : MonoBehaviour
{
    [Header("Assign the block you want to move when this switch is triggered")]
    public Transform blockToMove;

    [Header("How far (in world units) should the block go down when the switch is pressed?")]
    public float moveDistance = 2f;

    [Header("How fast should the block slide down? (units per second)")]
    public float moveSpeed = 2f;

    // Optional: only allow the switch to activate once
    private bool hasActivated = false;

    // Cache the starting position so we know where to come from
    private Vector3 originalBlockPos;

    // The AudioSource on this switch (make sure to attach one in the Inspector)
    private AudioSource audioSource;

    void Start()
    {
        if (blockToMove == null)
        {
            Debug.LogError("ActivateSwitch: No block assigned! Please assign blockToMove in the inspector.");
            enabled = false;
            return;
        }

        originalBlockPos = blockToMove.position;

        // Grab the AudioSource component (RequireComponent ensures it exists)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("ActivateSwitch: No AudioSource found! Please add one to this GameObject.");
            enabled = false;
            return;
        }

        // Make sure Play On Awake is turned OFF in the Inspector, so it only plays when triggered.
    }

    // When something enters the trigger of this switch...
    private void OnTriggerEnter(Collider other)
    {
        // You can change this tag check to whatever your player is tagged as
        if (hasActivated == false && other.CompareTag("Player"))
        {
            // Prevent multiple activations
            hasActivated = true;

            // Play the switch sound once
            audioSource.Play();

            // Start moving the block down
            StartCoroutine(MoveBlockDown());
        }
    }

    private IEnumerator MoveBlockDown()
    {
        // Compute the target position by moving down in world‐space
        Vector3 targetPos = originalBlockPos + Vector3.down * moveDistance;

        // While we haven't reached (or overshot) the target position...
        while (blockToMove.position.y > targetPos.y + 0.001f)
        {
            // Move down smoothly at moveSpeed
            blockToMove.position += Vector3.down * moveSpeed * Time.deltaTime;
            yield return null;
        }

        // Snap exactly to the target in case of small overshoot
        blockToMove.position = targetPos;
    }
}
