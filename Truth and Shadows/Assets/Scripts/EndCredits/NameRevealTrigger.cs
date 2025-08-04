using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameRevealTrigger : MonoBehaviour
{
    // The name object we want to reveal
    public GameObject nameToReveal;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is the player
        // Make sure your player object has the tag "Player"
        if (other.CompareTag("Player"))
        {
            // Activate the name object, making it visible
            if (nameToReveal != null)
            {
                nameToReveal.SetActive(true);
            }
        }
    }
}
