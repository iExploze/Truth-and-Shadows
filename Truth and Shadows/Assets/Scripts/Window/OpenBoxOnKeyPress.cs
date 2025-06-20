using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlideWallOnKeyPress : MonoBehaviour
{
    [Header("Trigger Settings")]
    public Transform triggerZone;
    public float triggerRadius = 2f;
    public KeyCode interactionKey = KeyCode.F;
    public TextMeshProUGUI promptTMP;

    [Header("Movement Settings")]
    public Vector3 localMoveDirection = Vector3.right; // default: slide right
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    private bool isOpening = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool hasMoved = false;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + transform.TransformDirection(localMoveDirection.normalized) * moveDistance;

        if (promptTMP != null)
            promptTMP.gameObject.SetActive(false); // Hide prompt initially
    }

    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(player.transform.position, triggerZone.position);

        if (!hasMoved && distance < triggerRadius)
        {
            if (promptTMP != null)
                promptTMP.gameObject.SetActive(true);

            if (Input.GetKeyDown(interactionKey))
            {
                isOpening = true;
                hasMoved = true;
                if (promptTMP != null)
                    promptTMP.gameObject.SetActive(false); // Hide after pressing
            }
        }
        else if (!isOpening && promptTMP != null)
        {
            promptTMP.gameObject.SetActive(false);
        }

        if (isOpening)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition, moveSpeed * Time.deltaTime);
        }
    }
}