using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Examples; // Add this for CharacterMovement
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [Header("Forms")]
    public GameObject mainCharacterForm; // Drag in mainCharacterForm child (Shadow)
    public GameObject squidForm; // Drag in squidForm child (Squid)
    public GameObject shadowCharacter; // Reference to scene instance instead of prefab

    [Header("Camera Rigs")]
    [SerializeField]
    private CinemachineFreeLook mainCharacterFormCamera;

    [SerializeField]
    private CinemachineFreeLook squidFormCamera;

    [SerializeField]
    private CinemachineFreeLook shadowCharacterCamera;

    [Header("Shadow Spawn Settings")]
    [SerializeField]
    private float spawnOffset = 0.2f; // Distance to spawn shadow from player

    private enum FormState
    {
        mainCharacter,
        squid,
        shadowCharacter,
    }

    private FormState currentState = FormState.mainCharacter;
    private GameObject currentShadowCharacter;

    private CharacterMovement mainCharacterFormMovement; // Reference to main character's movement script
    private Rigidbody mainCharacterFormRigidbody; // Add this field
    private Animator mainCharacterFormAnimator; // Add this field

    // Add squid movement references
    private SquidControl squidFormMovement;
    private Rigidbody squidFormRigidbody;

    private InteractionManager interactionManager;
    private InteractionManager mainFormInteractionManager;
    private InteractionManager shadowCharacterInteractionManager;

    private void InitializeComponents()
    {
        // Get main character components
        mainCharacterFormMovement = mainCharacterForm.GetComponent<CharacterMovement>();
        mainCharacterFormRigidbody = mainCharacterForm.GetComponent<Rigidbody>();
        mainCharacterFormAnimator = mainCharacterForm.GetComponent<Animator>();
        mainFormInteractionManager = mainCharacterForm.GetComponent<InteractionManager>();

        // Get squid components
        squidFormMovement = squidForm.GetComponent<SquidControl>();
        squidFormRigidbody = squidForm.GetComponent<Rigidbody>();

        // Get shadow components
        shadowCharacterInteractionManager = shadowCharacter.GetComponent<InteractionManager>();
        
        // Get global interaction manager
        interactionManager = GetComponent<InteractionManager>();
    }

    private void SetInitialState()
    {
        mainCharacterForm.SetActive(true);
        squidForm.SetActive(false);
        shadowCharacter.SetActive(false);

        mainCharacterFormCamera.Priority = 10;
        squidFormCamera.Priority = 0;
        shadowCharacterCamera.Priority = 0;

        currentState = FormState.mainCharacter;
    }

    void Start()
    {
        InitializeComponents();
        SetInitialState();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleEInput();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleQInput();
        }
    }

    void HandleEInput()
    {
        switch (currentState)
        {
            case FormState.mainCharacter:
            case FormState.shadowCharacter:
                EnterSquidForm();
                break;
            case FormState.squid:
                SpawnShadowCharacter();
                break;
        }
    }

    void HandleQInput()
    {
        if (currentState != FormState.mainCharacter)
        {
            ReturnToNormalForm();
        }
    }

    private void DisableCurrentForm()
    {
        switch (currentState)
        {
            case FormState.mainCharacter:
                DisableMainCharacter();
                break;
            case FormState.squid:
                DisableSquid();
                break;
            case FormState.shadowCharacter:
                DisableShadowCharacter();
                break;
        }
    }

    private void DisableMainCharacter()
    {
        if (mainCharacterFormRigidbody != null)
        {
            mainCharacterFormRigidbody.velocity = Vector3.zero;
            mainCharacterFormRigidbody.angularVelocity = Vector3.zero;
            mainCharacterFormRigidbody.isKinematic = true;
        }
        if (mainCharacterFormMovement != null)
            mainCharacterFormMovement.enabled = false;
        if (mainCharacterFormAnimator != null)
        {
            mainCharacterFormAnimator.SetFloat("Speed", 0);
            mainCharacterFormAnimator.SetFloat("Direction", 0);
        }
    }

    private void DisableSquid()
    {
        if (squidFormRigidbody != null)
        {
            squidFormRigidbody.velocity = Vector3.zero;
            squidFormRigidbody.angularVelocity = Vector3.zero;
            squidFormRigidbody.isKinematic = true;
        }
        if (squidFormMovement != null)
            squidFormMovement.enabled = false;
        squidForm.SetActive(false);
    }

    private void DisableShadowCharacter()
    {
        if (shadowCharacterInteractionManager != null)
            shadowCharacterInteractionManager.enabled = false;
        shadowCharacter.SetActive(false);
    }

    void EnterSquidForm()
    {
        if (interactionManager != null)
            interactionManager.PreserveInteraction();

        GameObject sourceObject = (currentState == FormState.shadowCharacter) ? shadowCharacter : mainCharacterForm;
        DisableCurrentForm();

        Vector3 spawnPosition = CalculateSpawnPosition(sourceObject);
        EnableSquid(spawnPosition, sourceObject.transform.rotation);

        UpdateCameraPriorities(0, 10, 0);
        currentState = FormState.squid;
    }

    private Vector3 CalculateSpawnPosition(GameObject source)
    {
        Vector3 spawnPosition = source.transform.position + source.transform.forward * spawnOffset;
        if (Physics.Raycast(source.transform.position, source.transform.forward, out RaycastHit hit, spawnOffset))
        {
            spawnPosition = hit.point - source.transform.forward * 0.1f;
        }
        return spawnPosition;
    }

    private void EnableSquid(Vector3 position, Quaternion rotation)
    {
        squidForm.transform.position = position;
        squidForm.transform.rotation = rotation;
        squidForm.SetActive(true);
        if (squidFormMovement != null)
            squidFormMovement.enabled = true;
        if (squidFormRigidbody != null)
            squidFormRigidbody.isKinematic = false;
    }

    private void UpdateCameraPriorities(int main, int squid, int shadow)
    {
        mainCharacterFormCamera.Priority = main;
        squidFormCamera.Priority = squid;
        shadowCharacterCamera.Priority = shadow;
    }

    void SpawnShadowCharacter()
    {
        if (interactionManager != null)
            interactionManager.PreserveInteraction();

        DisableCurrentForm();
        
        Vector3 spawnPos = squidForm.transform.position;
        Quaternion spawnRot = squidForm.transform.rotation;
        
        squidForm.SetActive(false);

        shadowCharacter.transform.position = spawnPos;
        shadowCharacter.transform.rotation = spawnRot;
        shadowCharacter.SetActive(true);

        // Ensure interaction manager is enabled
        if (shadowCharacterInteractionManager != null)
        {
            shadowCharacterInteractionManager.enabled = true;
            Debug.Log("Enabled shadow character interaction manager");
        }

        // Update priorities
        UpdateCameraPriorities(0, 0, 10);

        currentState = FormState.shadowCharacter;
    }

    void ReturnToNormalForm()
    {
        if (currentState == FormState.shadowCharacter && interactionManager != null)
            interactionManager.EndCurrentInteraction();

        DisableCurrentForm();

        // Re-enable original character
        if (mainCharacterFormRigidbody != null)
            mainCharacterFormRigidbody.isKinematic = false;

        if (mainCharacterFormMovement != null)
            mainCharacterFormMovement.enabled = true;

        // Only update priorities
        UpdateCameraPriorities(10, 0, 0);

        currentState = FormState.mainCharacter;
    }
}
