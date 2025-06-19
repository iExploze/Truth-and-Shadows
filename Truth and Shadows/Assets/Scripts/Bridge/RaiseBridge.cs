using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RaiseBridge : MonoBehaviour
{
    public Transform player;
    public Transform bridge; // The bridge to move
    public float triggerRadius = 2f;
    public KeyCode interactionKey = KeyCode.F;
    public float raiseAmount = 3f; // How high the bridge moves
    public float moveSpeed = 2f;
    public TextMeshProUGUI promptTMP;

    private Vector3 startBridgePos;
    private Vector3 targetBridgePos;
    private bool isRaising = false;
    private bool activated = false;

    void Start()
    {
        if (bridge != null)
        {
            startBridgePos = bridge.position;
            targetBridgePos = startBridgePos + Vector3.up * raiseAmount;
        }

        if (promptTMP != null)
            promptTMP.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null || bridge == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (!activated && distance < triggerRadius)
        {
            if (promptTMP != null)
                promptTMP.gameObject.SetActive(true);

            if (Input.GetKeyDown(interactionKey))
            {
                activated = true;
                isRaising = true;
                promptTMP.gameObject.SetActive(false);
            }
        }
        else if (!activated && promptTMP != null)
        {
            promptTMP.gameObject.SetActive(false);
        }

        if (isRaising)
        {
            bridge.position = Vector3.MoveTowards(bridge.position, targetBridgePos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(bridge.position, targetBridgePos) < 0.01f)
            {
                bridge.position = targetBridgePos;
                isRaising = false;
            }
        }
    }
}
