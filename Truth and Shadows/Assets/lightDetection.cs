using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        
    }
}
