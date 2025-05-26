using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorDestroyer : MonoBehaviour
{
    [Tooltip("The item to delete when this door's trigger is activated")]
    public GameObject itemToDelete;

    private void Reset()
    {
        // auto-set this collider to be a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (itemToDelete != null)
        {
            Destroy(itemToDelete);
            Destroy(this.gameObject);
        }
        else
        {
        }
    }
}
