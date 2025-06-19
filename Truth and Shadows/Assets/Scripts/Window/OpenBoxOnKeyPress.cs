using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OpenBoxOnKeyPress : MonoBehaviour
{
    public Transform triggerZone; 
    public float triggerRadius = 2f;
    public KeyCode interactionKey = KeyCode.F;
    public float openHeight = 3f;
    public float openSpeed = 2f;
    public TextMeshProUGUI promptTMP; 

    private bool isOpening = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;

        if (promptTMP != null)
            promptTMP.gameObject.SetActive(false); // Hide prompt at start
    }

    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(player.transform.position, triggerZone.position);

        if (!isOpening && distance < triggerRadius)
        {
            if (promptTMP != null)
                promptTMP.gameObject.SetActive(true); // Show prompt

            if (Input.GetKeyDown(interactionKey))
                isOpening = true;
        }
        else
        {
            if (promptTMP != null)
                promptTMP.gameObject.SetActive(false); // Hide prompt
        }

        if (isOpening)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition, openSpeed * Time.deltaTime);
        }
    }
}