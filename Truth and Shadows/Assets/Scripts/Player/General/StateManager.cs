using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Examples;
using UnityEngine;
using InteractionManager = TruthAndShadows.Interaction.InteractionManager;

public class StateManager : MonoBehaviour
{
    [Header("Forms")]
    public GameObject mainCharacterForm;
    public GameObject squidForm;

    [Header("Camera Rigs")]
    [SerializeField]
    private CinemachineFreeLook mainCharacterCamera;
    [SerializeField]
    private CinemachineFreeLook squidCamera;

    [Header("Spawn Settings")]
    [SerializeField]
    private float spawnOffset = 2f;

    private CharacterMovement mainCharMovement;
    private Rigidbody mainCharRb;
    private Animator mainCharAnimator;
    private SquidControl squidMovement;
    private Rigidbody squidRb;

    private enum FormState { MainCharacter, Squid }
    private FormState currentState = FormState.MainCharacter;

    private InteractionManager mainInteractionManager;

    void Start()
    {
        mainCharMovement = mainCharacterForm.GetComponent<CharacterMovement>();
        mainCharRb = mainCharacterForm.GetComponent<Rigidbody>();
        mainCharAnimator = mainCharacterForm.GetComponent<Animator>();
        squidMovement = squidForm.GetComponent<SquidControl>();
        squidRb = squidForm.GetComponent<Rigidbody>();

        mainInteractionManager = mainCharacterForm.GetComponent<InteractionManager>();

        SetToHumanForm(); // Always start as human
    }

    // Call this to become a squid
    public void SwitchToSquidForm()
    {
        if (currentState == FormState.Squid) return;

        // Hide human, reset movement, physics
        mainInteractionManager.DropPickedUpItem();
        mainCharacterForm.SetActive(false);
        if (mainCharRb != null)
        {
            mainCharRb.velocity = Vector3.zero;
            mainCharRb.angularVelocity = Vector3.zero;
            mainCharRb.isKinematic = true;
        }
        if (mainCharMovement != null) mainCharMovement.enabled = false;
        if (mainCharAnimator != null)
        {
            mainCharAnimator.SetFloat("Speed", 0);
            mainCharAnimator.SetFloat("Direction", 0);
        }

        // Place and activate squid
        squidForm.transform.position = mainCharacterForm.transform.position;
        squidForm.transform.rotation = mainCharacterForm.transform.rotation;
        squidForm.SetActive(true);
        if (squidMovement != null) squidMovement.enabled = true;
        if (squidRb != null) squidRb.isKinematic = false;

        // Camera
        UpdateCameraPriorities(main: 0, squid: 10);

        // Play squid sound if exists
        PlayAudio(squidForm);

        SyncCameraState(mainCharacterCamera, squidCamera); // Copy current state to squid camera
        UpdateCameraPriorities(main: 0, squid: 10);

        currentState = FormState.Squid;
    }

    // Call this to become human again
    public void SwitchToHumanForm()
    {
        if (currentState == FormState.MainCharacter) return;

        // Hide squid, reset movement/physics
        squidForm.SetActive(false);
        if (squidRb != null)
        {
            squidRb.velocity = Vector3.zero;
            squidRb.angularVelocity = Vector3.zero;
            squidRb.isKinematic = true;
        }
        if (squidMovement != null) squidMovement.enabled = false;

        // Activate human, enable controls
        mainCharacterForm.transform.position = squidForm.transform.position;
        mainCharacterForm.transform.rotation = squidForm.transform.rotation;
        mainCharacterForm.SetActive(true);
        if (mainCharRb != null) mainCharRb.isKinematic = false;
        if (mainCharMovement != null) mainCharMovement.enabled = true;

        UpdateCameraPriorities(main: 10, squid: 0);

        currentState = FormState.MainCharacter;

        // Play human sound if exists
        PlayAudio(mainCharacterForm);

        SyncCameraState(squidCamera, mainCharacterCamera); // Copy current state to main camera
        UpdateCameraPriorities(main: 10, squid: 0);
    }

    // Helper: resets everything for Start()
    private void SetToHumanForm()
    {
        mainCharacterForm.SetActive(true);
        squidForm.SetActive(false);
        if (mainCharRb != null) mainCharRb.isKinematic = false;
        if (mainCharMovement != null) mainCharMovement.enabled = true;
        if (squidMovement != null) squidMovement.enabled = false;
        if (squidRb != null) squidRb.isKinematic = true;
        currentState = FormState.MainCharacter;
        UpdateCameraPriorities(main: 10, squid: 0);
    }

    private void UpdateCameraPriorities(int main, int squid)
    {
        if (mainCharacterCamera != null) mainCharacterCamera.Priority = main;
        if (squidCamera != null) squidCamera.Priority = squid;
    }

    private void PlayAudio(GameObject obj)
    {
        var audio = obj.GetComponent<AudioSource>();
        if (audio != null) audio.Play();
    }

    public bool isHumanForm()
    {
        return currentState == FormState.MainCharacter;
    }

    private void SyncCameraState(CinemachineFreeLook source, CinemachineFreeLook target)
    {
        // Copy rotation (horizontal axis & vertical axis)
        target.m_XAxis.Value = source.m_XAxis.Value;
        target.m_YAxis.Value = source.m_YAxis.Value;

        // Copy rig settings (optional, if they differ)
        for (int i = 0; i < 3; i++)
        {
            target.m_Orbits[i].m_Height = source.m_Orbits[i].m_Height;
            target.m_Orbits[i].m_Radius = source.m_Orbits[i].m_Radius;
        }
    }


}
