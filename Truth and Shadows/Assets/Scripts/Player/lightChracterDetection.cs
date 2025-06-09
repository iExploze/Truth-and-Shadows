using System.Collections;
using System.Collections.Generic;
using Cinemachine.Examples;
using UnityEngine;

public class lightCharacterDetection : MonoBehaviour, ILightHittable
{
    private StateManager stateManager;
    private CharacterMovement characterMovement;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        // Find the StateManager in the scene (adjust if you use a different setup)
        stateManager = FindObjectOfType<StateManager>();
    }

    public void OnLightEnter(Light lightSource)
    {
        // Optionally: do stuff when entering light
        characterMovement.canMove = true;
    }

    public void OnLightStay(Light lightSource)
    {
        // Optionally: do stuff while staying in light
    }

    public void OnLightExit(Light lightSource)
    {
        // Instantly turn into a squid/wraith when you leave light
        characterMovement.canMove = false;
        if (stateManager != null)
        {
            stateManager.SwitchToSquidForm();
        }
        else
        {
            Debug.LogWarning("StateManager not found on light exit!");
        }
    }
}
