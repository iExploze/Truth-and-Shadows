using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameRevealTrigger : MonoBehaviour
{
    public GameObject nameToReveal;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (nameToReveal != null)
            {
                nameToReveal.SetActive(true);
            }
        }
    }
}
