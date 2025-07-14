using TruthAndShadows.Player;
using UnityEngine;
using System.Collections.Generic;

public class lightCharacterDetection : MonoBehaviour, ILightHittable
{
    private StateManager stateManager;
    private CharacterMovement characterMovement;

    // Track all current lights
    private HashSet<Light> currentLights = new HashSet<Light>();
    private bool isSquid = false;

    //Audio
    public AudioSource intoLightSound;

    //particles
    [SerializeField] private ParticleSystem dissolveEffect;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        stateManager = FindObjectOfType<StateManager>();
        isSquid = false;
    }

    public void OnLightEnter(Light lightSource)
    {
        // Add this light
        bool wasInLight = currentLights.Count > 0;
        currentLights.Add(lightSource);

        if (!wasInLight && !intoLightSound.isPlaying && currentLights.Count > 0)
        {
            // Just entered *any* light
            isSquid = false;
            characterMovement.canMove = true;
            intoLightSound.Play();
            //Debug.Log("Entered light");
        }
        else if (intoLightSound.isPlaying )
        {
            intoLightSound.Stop();
        }
    }

    public void OnLightStay(Light lightSource)
    {
        // Optional: refresh logic (no-op if not needed)
        isSquid = false; // now human
        characterMovement.canMove = true;
    }

    public void OnLightExit(Light lightSource)
    {
        dissolveEffect.Play();
        currentLights.Remove(lightSource);
    }

    void Update()
    {
        if (currentLights.Count == 0)
        {
            isSquid = true;
            // Truly out of *all* lights
            //Debug.Log("Exited all lights");
            SwitchToSquidIfNeeded();
        }
    }

    private void SwitchToSquidIfNeeded()
    {
        characterMovement.canMove = false;
        if (stateManager != null)
        {
            stateManager.SwitchToSquidForm();
        }
        else
        {
            Debug.LogWarning("StateManager not found on light exit!");
        }
        isSquid = true;
    }
}
