using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideCubeOnApproach : MonoBehaviour
{
    public Transform player;
    public float triggerDistance = 4f;
    public float moveDistance = 2f;
    public float moveSpeed = 2f;
    public bool moveLeft = true;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool activated = false;

    void Start()
    {
        startPos = transform.position;

        // Set target position based on direction
        if (moveLeft)
            targetPos = startPos + Vector3.left * moveDistance;
        else
            targetPos = startPos + Vector3.right * moveDistance;
    }

    void Update()
    {
        if (!activated && Vector3.Distance(player.position, transform.position) <= triggerDistance)
        {
            activated = true;
        }

        if (activated)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }
}
