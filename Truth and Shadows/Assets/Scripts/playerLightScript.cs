using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerLightScript : MonoBehaviour, ILightHittable
{
    private AudioSource audioSource;

    [SerializeField] private AudioSource test;
    void Start() 
    {
        // Get the audio source component attached to the same GameObject
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true; // Set the audio to loop
        audioSource.playOnAwake = false; // Don't play automatically
    }
    public void OnLightEnter(Light lightSource)
    {
        Debug.Log("enter light");

        // Start playing the sound when entering light
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void OnLightExit(Light lightSource)
    {
        Debug.Log("exit lights");

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        PlaySoundOnce();
    }

    public void OnLightStay(Light lightSource)
    {
        Debug.Log("in lights");

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void PlaySoundOnce()
    {
        if (test != null)
        {
            test.PlayOneShot(test.clip);
            Debug.Log("Sound played once");
        }
        else
        {
            Debug.LogWarning("AudioSource not assigned.");
        }
    }
}
