using System.Collections;
using Cinemachine.Examples;
using UnityEngine;

public class lightCharacterDetection : MonoBehaviour, ILightHittable
{
    private StateManager stateManager;
    private CharacterMovement characterMovement;

    private bool isInLight = false;
    private bool isSquid = false; // Tracks if we've already switched forms

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        stateManager = FindObjectOfType<StateManager>();
    }

    public void OnLightEnter(Light lightSource)
    {
        isInLight = true;
        isSquid = false; // We're in light, should be human form
        characterMovement.canMove = true;
    }

    public void OnLightStay(Light lightSource)
    {
        isInLight = true;
        // Can add more logic here if needed
    }

    public void OnLightExit(Light lightSource)
    {
        isInLight = false;
        // We'll handle switching in Update(), but you could also do it here just in case
        SwitchToSquidIfNeeded();
    }

    void Update()
    {
        if (!isInLight && !isSquid)
        {
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
