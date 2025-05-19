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

    void Start()
    {
        // Main is active at start
        mainCharacterForm.SetActive(true);
        squidForm.SetActive(false); // Shadow starts inactive
        shadowCharacter.SetActive(false); // Disable at start
        mainCharacterFormMovement = mainCharacterForm.GetComponent<CharacterMovement>();
        mainCharacterFormRigidbody = mainCharacterForm.GetComponent<Rigidbody>(); // Get rigidbody reference
        mainCharacterFormAnimator = mainCharacterForm.GetComponent<Animator>(); // Get animator reference

        // Get squid components
        squidFormMovement = squidForm.GetComponent<SquidControl>();
        squidFormRigidbody = squidForm.GetComponent<Rigidbody>();

        mainCharacterForm.tag = "Player";
        squidForm.tag = "Untagged";
        shadowCharacter.tag = "Untagged"; // Ensure shadow character is untagged

        // Validate cameras
        if (
            mainCharacterFormCamera == null
            || squidFormCamera == null
            || shadowCharacterCamera == null
        )
        {
            Debug.LogError("Missing camera references in StateManager!");
            return;
        }

        // Set initial camera priorities only
        mainCharacterFormCamera.Priority = 10;
        squidFormCamera.Priority = 0;
        shadowCharacterCamera.Priority = 0;

        interactionManager = GetComponent<InteractionManager>();
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

    void EnterSquidForm()
    {
        if (interactionManager != null)
            interactionManager.PreserveInteraction();

        // Get the source object to spawn from
        GameObject sourceObject =
            (currentState == FormState.shadowCharacter) ? shadowCharacter : mainCharacterForm;

        // Disable current form if needed
        if (currentState == FormState.shadowCharacter)
        {
            shadowCharacter.SetActive(false);
        }

        StopCharacterMovement();

        // Spawn squid at offset from the correct source
        Vector3 spawnPosition =
            sourceObject.transform.position + sourceObject.transform.forward * spawnOffset;
        RaycastHit hit;
        if (
            Physics.Raycast(
                sourceObject.transform.position,
                sourceObject.transform.forward,
                out hit,
                spawnOffset
            )
        )
        {
            spawnPosition = hit.point - sourceObject.transform.forward * 0.1f;
        }

        squidForm.transform.position = spawnPosition;
        squidForm.transform.rotation = sourceObject.transform.rotation;
        squidForm.SetActive(true);

        mainCharacterFormCamera.Priority = 0;
        squidFormCamera.Priority = 10;
        shadowCharacterCamera.Priority = 0;

        currentState = FormState.squid;
    }

    void StopCharacterMovement()
    {
        // Stop main character movement if needed
        if (currentState == FormState.mainCharacter)
        {
            if (mainCharacterFormRigidbody != null)
            {
                mainCharacterFormRigidbody.velocity = Vector3.zero;
                mainCharacterFormRigidbody.angularVelocity = Vector3.zero;
                mainCharacterFormRigidbody.isKinematic = true;
            }

            if (mainCharacterFormAnimator != null)
            {
                mainCharacterFormAnimator.SetFloat("Speed", 0);
                mainCharacterFormAnimator.SetFloat("Direction", 0);
            }

            if (mainCharacterFormMovement != null)
                mainCharacterFormMovement.enabled = false;
        }

        // Stop squid movement if needed
        if (currentState == FormState.squid && squidFormRigidbody != null)
        {
            squidFormRigidbody.velocity = Vector3.zero;
            squidFormRigidbody.angularVelocity = Vector3.zero;

            if (squidFormMovement != null)
                squidFormMovement.enabled = false;
        }
    }

    void SpawnShadowCharacter()
    {
        if (interactionManager != null)
            interactionManager.PreserveInteraction();

        // Get position from current shadow
        Vector3 spawnPos = squidForm.transform.position;
        Quaternion spawnRot = squidForm.transform.rotation;

        // Disable shadow
        squidForm.SetActive(false);

        // Enable and position shadow character
        shadowCharacter.transform.position = spawnPos;
        shadowCharacter.transform.rotation = spawnRot;
        shadowCharacter.SetActive(true);

        // Update priorities
        mainCharacterFormCamera.Priority = 0;
        squidFormCamera.Priority = 0;
        shadowCharacterCamera.Priority = 10;

        currentState = FormState.shadowCharacter;
    }

    void ReturnToNormalForm()
    {
        if (currentState == FormState.shadowCharacter && interactionManager != null)
            interactionManager.EndCurrentInteraction();

        // Disable current forms
        if (currentState == FormState.shadowCharacter)
        {
            shadowCharacter.SetActive(false);
        }
        else if (currentState == FormState.squid)
        {
            squidForm.SetActive(false);
        }

        // Re-enable original character
        if (mainCharacterFormRigidbody != null)
            mainCharacterFormRigidbody.isKinematic = false;

        if (mainCharacterFormMovement != null)
            mainCharacterFormMovement.enabled = true;

        // Only update priorities
        mainCharacterFormCamera.Priority = 10;
        squidFormCamera.Priority = 0;
        shadowCharacterCamera.Priority = 0;

        currentState = FormState.mainCharacter;
    }
}
