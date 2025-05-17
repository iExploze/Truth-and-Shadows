using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCloneController : MonoBehaviour
{
    public GameObject clonePrefab;
    public Transform cloneSpawnPoint;

    private GameObject currentClone;
    private CharacterController characterController;
    private Camera mainCamera;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentClone == null)
        {
            // Disable main character control & camera
            characterController.enabled = false;
            if (mainCamera) mainCamera.enabled = false;

            // Spawn clone
            currentClone = Instantiate(clonePrefab, cloneSpawnPoint.position, Quaternion.identity);
        }

        if (Input.GetKeyDown(KeyCode.S) && currentClone != null)
        {
            // Destroy clone
            Destroy(currentClone);
            currentClone = null;

            // Re-enable main character control & camera
            characterController.enabled = true;
            if (mainCamera) mainCamera.enabled = true;
        }
    }
}

