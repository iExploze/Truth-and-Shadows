using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollectShard : MonoBehaviour

{
    public TMP_Text winnerText;
    public TMP_Text poemLine;
    public TMP_Text reset;
    public AudioSource winSoundSource;

    public GameObject winMenu;
    // Start is called before the first frame update
    void Start()
    {
        // displayPoem = GetComponent<DisplayPoem>();
        winMenu.SetActive(false);
        winnerText.text = "";
        poemLine.text = "";
        reset.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            winSoundSource.Play();
            winMenu.SetActive(true);
            winnerText.text = "LEVEL COMPLETE";
            poemLine.text = "Lorem ipsum dolor sit amet consectetur.";
            reset.text = "Press L to reset the level";   
        }
    }
}