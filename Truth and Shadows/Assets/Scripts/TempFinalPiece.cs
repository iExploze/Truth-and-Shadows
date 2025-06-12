using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TempFinalPiece : MonoBehaviour
{
    [Header("Bobbing settings")]
    public float bobAmplitude = 0.5f;
    public float bobFrequency = 1f;

    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool hasActivated = false;

    void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("TempFinalPiece: No AudioSource found! Please attach one to this GameObject.");
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasActivated && other.CompareTag("Player"))
        {
            hasActivated = true;

            // Play sound
            if (audioSource != null)
                audioSource.Play();

            // Disable visuals and collider immediately (optional)
            if (TryGetComponent<MeshRenderer>(out var mesh)) mesh.enabled = false;
            if (TryGetComponent<Collider>(out var collider)) collider.enabled = false;

            // Destroy after the audio finishes
            StartCoroutine(DestroyAfterSound());
        }
    }

    private IEnumerator DestroyAfterSound()
    {
        // Wait for the sound to finish (or 0.5 sec if none)
        float waitTime = audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0.5f;
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }
}
