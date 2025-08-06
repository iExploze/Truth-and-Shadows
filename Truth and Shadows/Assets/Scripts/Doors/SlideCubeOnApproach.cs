using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideDoorOnApproach : MonoBehaviour
{
    public Transform player;
    public float triggerDistance = 4f;
    public float moveDistance = 2f;
    public float moveSpeed = 2f;
    public bool moveLeft = true;
    public float requiredTimeInRange = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool activated = false;
    private float timeInRange = 0f;

    void Start()
    {
        startPos = transform.position;
        Vector3 localDir = moveLeft ? -transform.right : transform.right;
        targetPos = startPos + localDir * moveDistance;
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (!activated)
        {
            if (distance <= triggerDistance)
            {
                timeInRange += Time.deltaTime;

                if (timeInRange >= requiredTimeInRange)
                {
                    activated = true;
                }
            }
            else
            {
                timeInRange = 0f;
            }
        }

        if (activated)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }
}