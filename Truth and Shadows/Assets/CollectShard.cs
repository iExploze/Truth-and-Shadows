using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollectShard : MonoBehaviour

{
    public TMP_Text winnerText;
    public TMP_Text poemLine;
    // Start is called before the first frame update
    void Start()
    {
        // displayPoem = GetComponent<DisplayPoem>();
        winnerText.text = "";
        poemLine.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
            
            winnerText.text = "LEVEL COMPLETE";
            poemLine.text = "Lorem ipsum dolor sit amet consectetur.";
        }
    }
}