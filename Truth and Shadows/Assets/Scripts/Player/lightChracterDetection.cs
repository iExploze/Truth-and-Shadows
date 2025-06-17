using Cinemachine.Examples;
using UnityEngine;

public class lightCharacterDetection : MonoBehaviour, ILightHittable
{
    private StateManager stateManager;
    private CharacterMovement characterMovement;

    private bool isInLight = false;
    private bool isSquid = false;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        stateManager = FindObjectOfType<StateManager>();
    }

    public void OnLightEnter(Light lightSource)
    {
        if (isInLight) return; // already in light
        isInLight = true;
        isSquid = false; // now human
        characterMovement.canMove = true;
    }

    public void OnLightStay(Light lightSource)
    {
        isInLight = true;
        // No-op, unless you want special logic
    }

    public void OnLightExit(Light lightSource)
    {
        if (isInLight) return; // already not in light
        Debug.Log("called switch to squid");
        isInLight = false;
        SwitchToSquidIfNeeded();
    }

    void Update()
    {
        if (!isInLight && !isSquid)
        {
            SwitchToSquidIfNeeded();
        }
        isInLight = false;
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
