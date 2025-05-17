using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject characterPrefab; // Assign this in the inspector with your character prefab
    public Vector3 cloneScale = new Vector3(1f, 0.05f, 1f); // smaller clone scale

    private bool hasSpawned = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !hasSpawned)
        {
            // Stop original character movement
            var movement = GetComponent<MoveShadow>();
            if (movement != null)
                movement.enabled = false;

            // Store the original camera world position
            Transform originalCamera = GetComponentInChildren<Camera>().transform;
            Vector3 originalCameraPosition = originalCamera.position;

            // Spawn clone lower so it sits on ground
            Vector3 spawnPosition = transform.position;
            spawnPosition.y -= 0.9f;

            GameObject clone = Instantiate(characterPrefab, spawnPosition, transform.rotation);
            clone.transform.localScale = new Vector3(1f, 0.1f, 1f);

            // Move clone's camera to match the original camera position
            Camera cloneCamera = clone.GetComponentInChildren<Camera>();
            if (cloneCamera != null)
            {
                cloneCamera.transform.position = originalCameraPosition;
            }

            hasSpawned = true;
        }
    }
}

