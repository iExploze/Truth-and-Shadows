using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Tooltip("Where the player will be teleported to")]
    public Transform teleportDestination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = teleportDestination.position;
            // Optional: reset velocity if using Rigidbody
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = Vector3.zero;
        }
    }
}
