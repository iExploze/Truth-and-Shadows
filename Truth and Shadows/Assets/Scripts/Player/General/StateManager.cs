using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TruthAndShadows.Player;
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
    //public CinemachineFreeLook MainCharacterCamera => mainCharacterCamera;
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

    private RagdollOnOff playerRagdoll;

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

        playerRagdoll = GetComponent<RagdollOnOff>();

        SetToHumanForm(); // Always start as human

        // Align camera after everything is initialized
        StartCoroutine(AlignCameraBehindPlayer(mainCharacterCamera));
    }

    public void SwitchToSquidForm()
    {
        if (currentState == FormState.Squid) return;

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

        squidForm.transform.position = mainCharacterForm.transform.position;
        squidForm.transform.rotation = mainCharacterForm.transform.rotation;
        squidForm.SetActive(true);
        if (squidMovement != null) squidMovement.enabled = true;
        if (squidRb != null) squidRb.isKinematic = false;

        UpdateCameraPriorities(main: 0, squid: 10);
        PlayAudio(squidForm);

        SyncCameraDirectionOnly(mainCharacterCamera, squidCamera);
        UpdateCameraPriorities(main: 0, squid: 10);

        //StartCoroutine(AlignCameraBehindPlayer(squidCamera));

        currentState = FormState.Squid;
    }

    public void SwitchToHumanForm()
    {
        if (currentState == FormState.MainCharacter) return;

        squidForm.SetActive(false);
        if (squidRb != null)
        {
            squidRb.velocity = Vector3.zero;
            squidRb.angularVelocity = Vector3.zero;
            squidRb.isKinematic = true;
        }
        if (squidMovement != null) squidMovement.enabled = false;

        mainCharacterForm.transform.position = squidForm.transform.position;
        mainCharacterForm.transform.rotation = squidForm.transform.rotation;
        mainCharacterForm.SetActive(true);
        if (mainCharRb != null) mainCharRb.isKinematic = false;
        if (mainCharMovement != null) mainCharMovement.enabled = true;

        UpdateCameraPriorities(main: 10, squid: 0);

        currentState = FormState.MainCharacter;

        // Play human sound if exists
        // PlayAudio(mainCharacterForm);

        SyncCameraDirectionOnly(squidCamera, mainCharacterCamera);
        UpdateCameraPriorities(main: 10, squid: 0);

        Debug.Log("We got here");

        if (playerRagdoll != null)
        {
            playerRagdoll.RagdollModeOff(); // now using proper public method
            Debug.Log("Ragdoll reset called");
        }

        //StartCoroutine(AlignCameraBehindPlayer(mainCharacterCamera));
    }

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

        if (playerRagdoll != null)
        {
            playerRagdoll.RagdollModeOff(); // now using proper public method
            Debug.Log("Ragdoll reset called 1");
        }
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

    private void SyncCameraDirectionOnly(CinemachineFreeLook source, CinemachineFreeLook target)
    {
        target.m_XAxis.Value = source.m_XAxis.Value;
        target.m_YAxis.Value = source.m_YAxis.Value;
    }

    public IEnumerator AlignCameraBehindPlayer(CinemachineFreeLook cam)
    {
        yield return null;

        if (cam != null)
        {
            cam.m_XAxis.Value = -90f;
        }
    }

    public void OnRespawn()
    {
        // Only adjust if in main character form
        if (currentState == FormState.MainCharacter)
        {
            StartCoroutine(AlignCameraBehindPlayer(mainCharacterCamera));
        }
        else if (currentState == FormState.Squid)
        {
            StartCoroutine(AlignCameraBehindPlayer(squidCamera));
        }

        if (playerRagdoll != null)
        {
            playerRagdoll.RagdollModeOff(); // now using proper public method
        }
    }
}