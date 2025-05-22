using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightDetection : MonoBehaviour
{
    // Reference to the spotlight attached to this GameObject
    private Light spotLight;

    // Track current player with "Player" tag
    private GameObject currentPlayer;

    // Keep track of whether the player is currently in the light
    private bool isPlayerInLight = false;

    void Start()
    {
        // Get the Light component attached to this GameObject
        spotLight = GetComponent<Light>();

        if (spotLight == null || spotLight.type != LightType.Spot)
        {
            Debug.LogError("This script requires a Spot Light assigned.");
        }
    }

    void Update()
    {
        // Dynamically find the currently active player with the "Player" tag
        currentPlayer = GameObject.FindGameObjectWithTag("Player");

        // find the script that interacts with lights in that gameobject


        if (currentPlayer == null || !currentPlayer.activeInHierarchy)
            return;

        // Determine if the current player is in the light cone
        bool isInLight = isPlayerInCone(currentPlayer.transform);

        // Try to get the ILightHittable component from the active player
        ILightHittable playerLight = currentPlayer.GetComponent<ILightHittable>();

        if (playerLight == null)
        {
            Debug.LogWarning("No ILightHittable component on current Player.");
            return;
        }

        // State transitions
        if (isInLight && !isPlayerInLight)
        {
            // Player just entered the light
            playerLight.OnLightEnter(spotLight);
            isPlayerInLight = true;
        }
        else if (isInLight && isPlayerInLight)
        {
            // Player stays in the light
            playerLight.OnLightStay(spotLight);
        }
        else if (!isInLight && isPlayerInLight)
        {
            // Player just exited the light
            playerLight.OnLightExit(spotLight);
            isPlayerInLight = false;
        }
    }

    // Helper function to check if a target Transform is within the spotlight's cone
    private bool isPlayerInCone(Transform target)
    {
        // Safety check in case the light is missing or not a spotlight
        if (spotLight == null || spotLight.type != LightType.Spot)
        {
            Debug.LogError("This script requires a Spot Light assigned.");
            return false;
        }

        // Get the direction from spotlight to player
        Vector3 directionToPlayer = (target.position - spotLight.transform.position).normalized;

        // Calculate the angle between the spotlight's forward direction and the direction to the player
        float angleToPlayer = Vector3.Angle(spotLight.transform.forward, directionToPlayer);

        // Check if the player is within the light cone's angle
        if (angleToPlayer <= spotLight.spotAngle / 2)
        {
            // Calculate the distance between the spotlight and the player
            float distanceToPlayer = Vector3.Distance(spotLight.transform.position, target.position);

            // Check if the player is within the spotlight's range
            if (distanceToPlayer <= spotLight.range)
            {
                int layerMask = ~LayerMask.GetMask("IgnoreLightRaycast");

                // Cast a ray in the calculated direction and check for a hit
                Ray ray = new Ray(spotLight.transform.position, directionToPlayer);
                RaycastHit hit;

                // Check if the ray hits the player and is not blocked
                if (Physics.Raycast(ray, out hit, spotLight.range, layerMask))
                {

                    if (hit.transform.CompareTag("Player"))
                    {
                        Debug.DrawLine(spotLight.transform.position, hit.point, Color.green);  // Visualize the hit
                        return true;
                    }
                    else
                    {
                        Debug.DrawLine(spotLight.transform.position, hit.point, Color.red);  // Visualize the blocked hit
                    }
                }
            }
        }

        return false;
    }
}
