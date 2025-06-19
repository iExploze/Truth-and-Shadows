using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonApproach : MonoBehaviour
{
    public Transform player;
    public TextMeshProUGUI OpenButton;
    public float triggerRadius = 2f;

    void Start()
    {
        if (OpenButton != null)
            OpenButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null || OpenButton == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < triggerRadius)
        {
            OpenButton.gameObject.SetActive(true);
        }
        else
        {
            OpenButton.gameObject.SetActive(false);
        }
    }
}
