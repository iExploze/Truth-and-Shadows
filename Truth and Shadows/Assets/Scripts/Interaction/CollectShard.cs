using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollectShard : MonoBehaviour

{
    public AudioSource winSoundSource;

    public GameObject winMenu;
    // Start is called before the first frame update
    void Start()
    {
        // displayPoem = GetComponent<DisplayPoem>();
        winMenu.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            winSoundSource.Play();
            winMenu.SetActive(true);
        }
    }
}