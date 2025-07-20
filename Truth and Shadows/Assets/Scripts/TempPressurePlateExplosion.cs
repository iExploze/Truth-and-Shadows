using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TempPressurePlateExplosion : MonoBehaviour
{
    public GameObject objectToEnable;         // Assign the parent GameObject with children
    public GameObject objectToDisable;         // Assign the parent GameObject with children
    public AudioClip explosionSound;          // Assign explosion or rumble clip
    public float shakeIntensity = 2f;
    public float shakeDuration = 0.5f;

    private AudioSource audioSource;
    private CinemachineImpulseSource impulseSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Enable the whole object with children
            if (objectToEnable != null)
                objectToEnable.SetActive(true);

            if (objectToDisable != null)
                objectToDisable.SetActive(false);

            // Play sound
            if (audioSource && explosionSound)
                audioSource.PlayOneShot(explosionSound);

            // Screen shake
            if (impulseSource)
                impulseSource.GenerateImpulse(shakeIntensity);
        }
    }
}
