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
    [SerializeField] private CinemachineFreeLook mainCharacterFormCamera;
    [SerializeField] private CinemachineFreeLook squidFormCamera;
    [SerializeField] private CinemachineFreeLook shadowCharacterCamera;

    [Header("Shadow Spawn Settings")]
    [SerializeField] private float spawnOffset = 0.2f; // Distance to spawn shadow from player

    private enum FormState 
    {
        mainCharacter,
        squid,
        shadowCharacter
    }
    
    private FormState currentState = FormState.mainCharacter;
    private GameObject currentShadowCharacter;

    private CharacterMovement mainCharacterFormMovement; // Reference to main character's movement script
    private Rigidbody mainCharacterFormRigidbody;  // Add this field
    private Animator mainCharacterFormAnimator; // Add this field

    void Start()
    {
        // Main is active at start
        mainCharacterForm.SetActive(true);
        squidForm.SetActive(false); // Shadow starts inactive
        shadowCharacter.SetActive(false); // Disable at start
        mainCharacterFormMovement = mainCharacterForm.GetComponent<CharacterMovement>();
        mainCharacterFormRigidbody = mainCharacterForm.GetComponent<Rigidbody>();  // Get rigidbody reference
        mainCharacterFormAnimator = mainCharacterForm.GetComponent<Animator>(); // Get animator reference

        mainCharacterForm.tag = "Player";
        squidForm.tag = "Untagged";
        shadowCharacter.tag = "Untagged"; // Ensure shadow character is untagged

        // Validate cameras
        if (mainCharacterFormCamera == null || squidFormCamera == null || shadowCharacterCamera == null)
        {
            Debug.LogError("Missing camera references in StateManager!");
            return;
        }

        // Set initial camera priorities only
        mainCharacterFormCamera.Priority = 10;
        squidFormCamera.Priority = 0;
        shadowCharacterCamera.Priority = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleForm();
        }
    }

    void ToggleForm()
    {
        switch (currentState)
        {
            case FormState.mainCharacter:
                // Switch to shadow form
                EnterShadowForm();
                currentState = FormState.squid;
                break;

            case FormState.squid:
                // Switch to shadow character
                SpawnShadowCharacter();
                currentState = FormState.shadowCharacter;
                break;

            case FormState.shadowCharacter:
                // Return to normal
                ReturnToNormalForm();
                currentState = FormState.mainCharacter;
                break;
        }
    }

    void EnterShadowForm()
    {
        // Stop all player movement and animation
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

        // Spawn shadow at offset
        Vector3 spawnPosition = mainCharacterForm.transform.position + mainCharacterForm.transform.forward * spawnOffset;
        RaycastHit hit;
        if (Physics.Raycast(mainCharacterForm.transform.position, mainCharacterForm.transform.forward, out hit, spawnOffset))
        {
            spawnPosition = hit.point - mainCharacterForm.transform.forward * 0.1f;
        }

        squidForm.transform.position = spawnPosition;
        squidForm.transform.rotation = mainCharacterForm.transform.rotation;
        squidForm.SetActive(true);

        if (mainCharacterFormMovement != null)
            mainCharacterFormMovement.enabled = false;

        // Only update priorities
        mainCharacterFormCamera.Priority = 0;
        squidFormCamera.Priority = 10;
        shadowCharacterCamera.Priority = 0;
    }

    void SpawnShadowCharacter()
    {
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
    }

    void ReturnToNormalForm()
    {
        // Just disable shadow character
        shadowCharacter.SetActive(false);

        // Re-enable original character
        if (mainCharacterFormRigidbody != null)
            mainCharacterFormRigidbody.isKinematic = false;

        if (mainCharacterFormMovement != null)
            mainCharacterFormMovement.enabled = true;

        // Only update priorities
        mainCharacterFormCamera.Priority = 10;
        squidFormCamera.Priority = 0;
        shadowCharacterCamera.Priority = 0;
    }
}
