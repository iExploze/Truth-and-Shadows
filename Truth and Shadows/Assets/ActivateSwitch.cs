using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ActivateSwitch : MonoBehaviour
{
    public GameObject bridgeObject;
    public GameObject anotherObject;
    public AudioSource audioSource;  // Drag your AudioSource here in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("box"))
        {
            if (bridgeObject != null)
                bridgeObject.SetActive(true);

            if (anotherObject != null)
                anotherObject.SetActive(true);

            if (audioSource != null)
                audioSource.Play();
        }
    }
}
