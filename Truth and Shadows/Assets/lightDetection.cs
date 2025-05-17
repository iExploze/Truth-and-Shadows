using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightDetection : MonoBehaviour
{
    // Find all GameObjects with the tag "Player"
    private Light spotLight;
    public Image mask;

    GameObject[] players;

    // Store the light hit state of each player
    private Dictionary<GameObject, bool> playerLightState = new Dictionary<GameObject, bool>();

    // Start is called before the first frame update
    void Start()
    {
        // Get the Light component attached to this GameObject
        spotLight = GetComponent<Light>();

        players = GameObject.FindGameObjectsWithTag("Player");

        // Initialize light state for each player
        foreach (GameObject player in players)
        {
            playerLightState[player] = false; // Assume all players start out of light
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject player in players)
        {
            bool isInLight = isPlayerInCone(player.transform);

           // Debug.Log(isPlayerInCone(player.transform));
            // Try to get the ILightHittable component from each player
            ILightHittable playerLight = player.GetComponent<ILightHittable>();

            if (playerLight != null)
            {
                // State transitions
                if (isInLight && !playerLightState[player])
                {
                    // Player just entered the light
                    Debug.Log("enter light");
                    playerLight.OnLightEnter(spotLight);
                    playerLightState[player] = true;
                }
                else if (isInLight && playerLightState[player])
                {
                    // Player stays in the light
                    Debug.Log("stay in light");
                    playerLight.OnLightStay(spotLight);
                }
                else if (!isInLight && playerLightState[player])
                {
                    // Player just exited the light
                    playerLight.OnLightExit(spotLight);
                    playerLightState[player] = false;
                }
            }
            else
            {
                Debug.Log("No ILightHittable found on player: " + player.name);
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
