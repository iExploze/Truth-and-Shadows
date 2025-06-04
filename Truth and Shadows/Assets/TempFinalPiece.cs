using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TempFinalPiece : MonoBehaviour
{
    [Header("Bobbing settings")]
    [Tooltip("How high above its starting point the piece will move.")]
    public float bobAmplitude = 0.5f;
    [Tooltip("How fast it bobs (cycles per second).")]
    public float bobFrequency = 1f;

    // Remember the piece's original position so we can oscillate around it
    private Vector3 startPosition;

    // The AudioSource you’ve already attached in the inspector
    private AudioSource audioSource;

    // To make sure we only play the sound once
    private bool hasActivated = false;

    void Start()
    {
        // Cache the starting position for bobbing
        startPosition = transform.position;

        // Grab (or verify) the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("TempFinalPiece: No AudioSource found! Please attach one to this GameObject.");
        }
    }

    void Update()
    {
        // Simple sine‐wave bobbing in Y axis:
        float newY = startPosition.y + Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // This requires the final piece to have a Collider with "Is Trigger" checked.
    // The player should be tagged "Player", or adjust CompareTag(...) as needed.
    private void OnTriggerEnter(Collider other)
    {
        if (!hasActivated && other.CompareTag("Player"))
        {
            hasActivated = true;

            // Play the attached AudioSource once
            if (audioSource != null)
            {
                audioSource.Play();
            }

            // (Optional) You can also disable its mesh, spawn particles, etc., here.
            // For example:
            // GetComponent<MeshRenderer>().enabled = false;
            // GetComponent<Collider>().enabled = false;
        }
    }
}
