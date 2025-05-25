using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightDetection : MonoBehaviour
{
    // Reference to the spotlight attached to this GameObject
    private Light spotLight;

    // Track current player with "Player" tag
    private GameObject[] currentPlayers;
    private Dictionary<GameObject, bool> playerLightStates = new Dictionary<GameObject, bool>();

    void Start()
    {
        Debug.Log("BBBBBBBB");
        // Get the Light component attached to this GameObject
        spotLight = GetComponent<Light>();

        if (spotLight == null || spotLight.type != LightType.Spot)
        {
            Debug.LogError("This script requires a Spot Light assigned.");
        }
    }

    void Update()
    {
        currentPlayers = GameObject.FindGameObjectsWithTag("Player");

        // Track which players are still present
        HashSet<GameObject> stillPresent = new HashSet<GameObject>(currentPlayers);

        // Remove entries for players that no longer exist
        var oldPlayers = new List<GameObject>(playerLightStates.Keys);
        foreach (var player in oldPlayers)
        {
            if (!stillPresent.Contains(player))
            {
                playerLightStates.Remove(player);
            }
        }

        foreach (GameObject playerObj in currentPlayers)
        {
            if (!playerObj.activeInHierarchy)
                continue;

            bool isInLight = isPlayerInCone(playerObj.transform);
            ILightHittable playerLight = playerObj.GetComponent<ILightHittable>();

            if (playerLight == null)
            {
                Debug.LogWarning("No ILightHittable component on " + playerObj.name);
                continue;
            }

            bool wasInLight = playerLightStates.ContainsKey(playerObj) && playerLightStates[playerObj];

            if (isInLight && !wasInLight)
            {
                Debug.Log("player entered light");
                playerLight.OnLightEnter(spotLight);
                playerLightStates[playerObj] = true;
            }
            else if (isInLight && wasInLight)
            {
                Debug.Log("player stay light");
                playerLight.OnLightStay(spotLight);
            }
            else if (!isInLight && wasInLight)
            {
                playerLight.OnLightExit(spotLight);
                playerLightStates[playerObj] = false;
            }
            // else: !isInLight && !wasInLight → do nothing
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
