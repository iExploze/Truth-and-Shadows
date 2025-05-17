using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCrawlerMovement : MonoBehaviour
{
    public float moveSpeed = 0.1f;
    public float surfaceStickDistance = 0.6f;

    private Vector3 surfaceNormal = Vector3.up;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = transform;
    }

    void Update()
    {
        // Input
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        // Calculate right and forward vectors relative to surface normal
        Vector3 right = Vector3.Cross(Vector3.up, surfaceNormal).normalized;
        Vector3 forward = Vector3.Cross(surfaceNormal, right).normalized;

        // Move on surface plane
        Vector3 moveDir = (forward * vInput + right * hInput).normalized;

        playerTransform.position += moveDir * moveSpeed;

        // Raycast towards the surface to detect and stick
        RaycastHit hit;
        if (Physics.Raycast(playerTransform.position, -surfaceNormal, out hit, surfaceStickDistance))
        {
            surfaceNormal = hit.normal;

            // Align character "up" with the surface normal smoothly
            Quaternion targetRotation = Quaternion.FromToRotation(playerTransform.up, surfaceNormal) * playerTransform.rotation;
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        else
        {
            // If no surface, apply a simple downward move (gravity-like)
            playerTransform.position += Vector3.down * moveSpeed * Time.deltaTime;
        }
    }
}


