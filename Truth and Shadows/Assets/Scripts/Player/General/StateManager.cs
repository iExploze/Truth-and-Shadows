using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Examples; // Add this for CharacterMovement
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public GameObject mainForm; // Drag in MainForm child (Shadow)
    public GameObject altForm; // Drag in AltForm child (Squid)

    [Header("Camera Rigs")]
    [SerializeField] private CinemachineFreeLook mainFormCamera;
    [SerializeField] private CinemachineFreeLook altFormCamera;

    [Header("Shadow Spawn Settings")]
    [SerializeField] private float spawnOffset = 0.2f; // Distance to spawn shadow from player

    private bool isInAltForm = false;
    private CharacterMovement mainFormMovement; // Reference to main character's movement script
    private Rigidbody mainFormRigidbody;  // Add this field
    private Animator mainFormAnimator; // Add this field

    void Start()
    {
        // Main is active at start
        mainForm.SetActive(true);
        altForm.SetActive(false); // Shadow starts inactive
        mainFormMovement = mainForm.GetComponent<CharacterMovement>();
        mainFormRigidbody = mainForm.GetComponent<Rigidbody>();  // Get rigidbody reference
        mainFormAnimator = mainForm.GetComponent<Animator>(); // Get animator reference

        mainForm.tag = "Player";
        altForm.tag = "Untagged";

        // Set initial camera priorities
        if (mainFormCamera && altFormCamera)
        {
            mainFormCamera.Priority = 10;
            altFormCamera.Priority = 0;
        }
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
        isInAltForm = !isInAltForm;

        if (isInAltForm)
        {
            // Stop all player movement and animation
            if (mainFormRigidbody != null)
            {
                mainFormRigidbody.velocity = Vector3.zero;
                mainFormRigidbody.angularVelocity = Vector3.zero;
                mainFormRigidbody.isKinematic = true; // Prevent physics from moving player
            }

            if (mainFormAnimator != null)
            {
                mainFormAnimator.SetFloat("Speed", 0);
                mainFormAnimator.SetFloat("Direction", 0);
            }

            // Calculate spawn position in front of player
            Vector3 spawnPosition = mainForm.transform.position + mainForm.transform.forward * spawnOffset;
            
            // Ensure we're not spawning inside geometry
            RaycastHit hit;
            if (Physics.Raycast(mainForm.transform.position, mainForm.transform.forward, out hit, spawnOffset))
            {
                // If there's something in the way, spawn closer to avoid clipping
                spawnPosition = hit.point - mainForm.transform.forward * 0.1f;
            }
            
            altForm.transform.position = spawnPosition;
            altForm.transform.rotation = mainForm.transform.rotation;
            altForm.SetActive(true);

            // Disable character movement
            if (mainFormMovement != null)
                mainFormMovement.enabled = false;

            // Switch camera priorities
            mainFormCamera.Priority = 0;
            altFormCamera.Priority = 10;
        }
        else
        {
            // Simply disable shadow, don't modify character
            altForm.SetActive(false);

            // Re-enable physics when switching back
            if (mainFormRigidbody != null)
            {
                mainFormRigidbody.isKinematic = false;
            }

            // Re-enable character movement
            if (mainFormMovement != null)
                mainFormMovement.enabled = true;

            // Switch camera priorities
            mainFormCamera.Priority = 10;
            altFormCamera.Priority = 0;
        }
    }
}
