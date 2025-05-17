using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class lightDetection : MonoBehaviour
{
    // Find all GameObjects with the tag "Player"
    private Light spotLight;

    GameObject[] players;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Light component attached to this GameObject
        spotLight = GetComponent<Light>();

        players = GameObject.FindGameObjectsWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject player in players)
        {
            Debug.Log(isPlayerInCone(player.transform));
            // Try to get the ILightHittable component from each player
            ILightHittable playerLight = player.GetComponent<ILightHittable>();

            if (playerLight != null)
            {
                // Successfully found the component, do something with it
                Debug.Log("Found ILightHittable on player: " + player.name);
                // Example: Trigger a hit or any other logic
                
            }
            else
            {
                //Debug.Log("No ILightHittable found on player: " + player.name);
            }
        }
    }

    private bool isPlayerInCone(Transform target) 
    {
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
                // Cast a ray in the calculated direction and check for a hit
                Ray ray = new Ray(spotLight.transform.position, directionToPlayer);
                RaycastHit hit;

                // Check if the ray hits the player and is not blocked
                if (Physics.Raycast(ray, out hit, spotLight.range))
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
