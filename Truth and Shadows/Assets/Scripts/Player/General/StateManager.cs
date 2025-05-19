using System.Collections;
using System.Collections.Generic;
using Cinemachine; // Add this for FreeLook camera
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public GameObject mainForm; // Drag in MainForm child (Shadow)
    public GameObject altForm; // Drag in AltForm child (Squid)

    [Header("Camera Rigs")]
    [SerializeField] private CinemachineFreeLook mainFormCamera;
    [SerializeField] private CinemachineFreeLook altFormCamera;

    private bool isInAltForm = false;

    void Start()
    {
        // Main is active at start
        mainForm.SetActive(true);
        altForm.SetActive(false);

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
            // Move Squid to Shadow's position
            altForm.transform.position = mainForm.transform.position;
            altForm.transform.rotation = mainForm.transform.rotation;

            mainForm.SetActive(false);
            altForm.SetActive(true);

            // Switch camera priorities
            mainFormCamera.Priority = 0;
            altFormCamera.Priority = 10;
        }
        else
        {
            // Move Shadow to Squid's position
            mainForm.transform.position = altForm.transform.position;
            mainForm.transform.rotation = altForm.transform.rotation;

            altForm.SetActive(false);
            mainForm.SetActive(true);

            // Switch camera priorities
            mainFormCamera.Priority = 10;
            altFormCamera.Priority = 0;
        }
    }
}
