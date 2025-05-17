using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject characterPrefab; // Assign this in Inspector
    public Vector3 cloneScale = new Vector3(1f, 0.1f, 1f); // Flat disk scale

    private GameObject currentClone;
    private Camera originalCamera;
    private MoveShadow movement;

    private bool hasSpawned = false;

    void Start()
    {
        movement = GetComponent<MoveShadow>();
        originalCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        // Spawn clone on left-click
        if (Input.GetMouseButtonDown(0) && !hasSpawned)
        {
            if (movement != null)
                movement.enabled = false;

            Vector3 originalCameraPosition = originalCamera.transform.position;

            Vector3 spawnPosition = transform.position;
            spawnPosition.y -= 0.9f; // lower it to sit on ground

            currentClone = Instantiate(characterPrefab, spawnPosition, transform.rotation);
            currentClone.transform.localScale = cloneScale;

            //var mainMovement = currentClone.GetComponent<MoveShadow>();
            //if (mainMovement != null)
            //    Destroy(mainMovement);

            currentClone.AddComponent<WallCrawlerMovement>();


            Camera cloneCamera = currentClone.GetComponentInChildren<Camera>();
            if (cloneCamera != null)
                cloneCamera.transform.position = originalCameraPosition;

            originalCamera.enabled = false;
            hasSpawned = true;
        }

        // Return to main character on 'S'
        if (Input.GetKeyDown(KeyCode.E) && hasSpawned && currentClone != null)
        {
            Destroy(currentClone);
            currentClone = null;

            if (movement != null)
                movement.enabled = true;

            if (originalCamera != null)
                originalCamera.enabled = true;

            hasSpawned = false;
        }
    }
}

