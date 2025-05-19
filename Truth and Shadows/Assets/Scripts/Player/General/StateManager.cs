using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Examples; // Add this for CharacterMovement
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [Header("Forms")]
    public GameObject mainForm; // Drag in MainForm child (Shadow)
    public GameObject altForm; // Drag in AltForm child (Squid)
    public GameObject shadowCharacter; // Reference to scene instance instead of prefab

    [Header("Camera Rigs")]
    [SerializeField] private CinemachineFreeLook mainFormCamera;
    [SerializeField] private CinemachineFreeLook altFormCamera;
    [SerializeField] private CinemachineFreeLook shadowCharacterCamera;

    [Header("Shadow Spawn Settings")]
    [SerializeField] private float spawnOffset = 0.2f; // Distance to spawn shadow from player

    private enum FormState 
    {
        Normal,
        Shadow,
        ShadowCharacter
    }
    
    private FormState currentState = FormState.Normal;
    private GameObject currentShadowCharacter;

    private CharacterMovement mainFormMovement; // Reference to main character's movement script
    private Rigidbody mainFormRigidbody;  // Add this field
    private Animator mainFormAnimator; // Add this field

    void Start()
    {
        // Main is active at start
        mainForm.SetActive(true);
        altForm.SetActive(false); // Shadow starts inactive
        shadowCharacter.SetActive(false); // Disable at start
        mainFormMovement = mainForm.GetComponent<CharacterMovement>();
        mainFormRigidbody = mainForm.GetComponent<Rigidbody>();  // Get rigidbody reference
        mainFormAnimator = mainForm.GetComponent<Animator>(); // Get animator reference

        mainForm.tag = "Player";
        altForm.tag = "Untagged";
        shadowCharacter.tag = "Untagged"; // Ensure shadow character is untagged

        // Validate cameras
        if (mainFormCamera == null || altFormCamera == null || shadowCharacterCamera == null)
        {
            Debug.LogError("Missing camera references in StateManager!");
            return;
        }

        // Set initial camera priorities only
        mainFormCamera.Priority = 10;
        altFormCamera.Priority = 0;
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
            case FormState.Normal:
                // Switch to shadow form
                EnterShadowForm();
                currentState = FormState.Shadow;
                break;

            case FormState.Shadow:
                // Switch to shadow character
                SpawnShadowCharacter();
                currentState = FormState.ShadowCharacter;
                break;

            case FormState.ShadowCharacter:
                // Return to normal
                ReturnToNormalForm();
                currentState = FormState.Normal;
                break;
        }
    }

    void EnterShadowForm()
    {
        // Stop all player movement and animation
        if (mainFormRigidbody != null)
        {
            mainFormRigidbody.velocity = Vector3.zero;
            mainFormRigidbody.angularVelocity = Vector3.zero;
            mainFormRigidbody.isKinematic = true;
        }

        if (mainFormAnimator != null)
        {
            mainFormAnimator.SetFloat("Speed", 0);
            mainFormAnimator.SetFloat("Direction", 0);
        }

        // Spawn shadow at offset
        Vector3 spawnPosition = mainForm.transform.position + mainForm.transform.forward * spawnOffset;
        RaycastHit hit;
        if (Physics.Raycast(mainForm.transform.position, mainForm.transform.forward, out hit, spawnOffset))
        {
            spawnPosition = hit.point - mainForm.transform.forward * 0.1f;
        }

        altForm.transform.position = spawnPosition;
        altForm.transform.rotation = mainForm.transform.rotation;
        altForm.SetActive(true);

        if (mainFormMovement != null)
            mainFormMovement.enabled = false;

        // Only update priorities
        mainFormCamera.Priority = 0;
        altFormCamera.Priority = 10;
        shadowCharacterCamera.Priority = 0;
    }

    void SpawnShadowCharacter()
    {
        // Get position from current shadow
        Vector3 spawnPos = altForm.transform.position;
        Quaternion spawnRot = altForm.transform.rotation;
        
        // Disable shadow
        altForm.SetActive(false);

        // Enable and position shadow character
        shadowCharacter.transform.position = spawnPos;
        shadowCharacter.transform.rotation = spawnRot;
        shadowCharacter.SetActive(true);
        
        // Update priorities
        mainFormCamera.Priority = 0;
        altFormCamera.Priority = 0;
        shadowCharacterCamera.Priority = 10;
    }

    void ReturnToNormalForm()
    {
        // Just disable shadow character
        shadowCharacter.SetActive(false);

        // Re-enable original character
        if (mainFormRigidbody != null)
            mainFormRigidbody.isKinematic = false;

        if (mainFormMovement != null)
            mainFormMovement.enabled = true;

        // Only update priorities
        mainFormCamera.Priority = 10;
        altFormCamera.Priority = 0;
        shadowCharacterCamera.Priority = 0;
    }
}
