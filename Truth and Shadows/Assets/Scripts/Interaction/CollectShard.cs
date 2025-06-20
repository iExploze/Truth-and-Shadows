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
        winnerText.text = "Level Complete";
        poemLine.text = "You";
        reset.text = "Reset Level";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            winSoundSource.Play();
            winMenu.SetActive(true);
            winnerText.text = "LEVEL 1 COMPLETE";
            poemLine.text = "He left me\r\nAnd I will never believe\r\nIt had to be that way";
            reset.text = "Press L to restart the level";   
        }
    }
}