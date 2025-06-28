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

        if (!wasInLight && currentLights.Count > 0)
        {
            // Just entered *any* light
            isSquid = false;
            characterMovement.canMove = true;
            //Debug.Log("Entered light");
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
