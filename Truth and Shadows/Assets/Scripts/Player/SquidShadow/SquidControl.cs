using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SquidControl : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Move Settings")]
    [Tooltip("Speed (units/sec) that the indicator moves in the horizontal plane.")]
    public float moveSpeed = 10f;

    [Tooltip("Maximum radius from player within which the indicator can be placed.")]
    public float maxRadius = 25f;

    [Header("Raycast Settings")]
    [Tooltip("How high above the indicator we start the downward ray.")]
    public float raycastHeight = 10f;

    [Header("References")]
    [Tooltip("Drag the player's root Transform here (used for clamping radius).")]
    public Transform playerRoot;

    private Camera mainCam;
    private Vector3 velocity; // for SmoothDamp

    private void Start()
    {
        mainCamera = Camera.main;

        // Optional: freeze rotation so physics won't spin it. 
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true; // we'll move this transform manually
    }

    void Update()
    {
        HandleHorizontalMovement();
        SnapToSurfaceBelow();
    }
    void HandleHorizontalMovement()
    {
        float h = Input.GetAxis("Horizontal"); 
        float v = Input.GetAxis("Vertical");

        if (Math.Abs(h) < 0.01f && Math.Abs(v) < 0.01f) 
        {
            return;
        }

        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDir = camForward * v + camRight * h;
        inputDir.Normalize();

        Vector3 rawTarget = transform.position + inputDir * moveSpeed * Time.deltaTime;

        Vector3 offsetFromPlayer = rawTarget - playerRoot.position;
        if (offsetFromPlayer.magnitude > maxRadius)
        {
            offsetFromPlayer = offsetFromPlayer.normalized * maxRadius;
            rawTarget = playerRoot.position + offsetFromPlayer;
        }

        Vector3 destination = new Vector3(rawTarget.x, transform.position.y, rawTarget.z);
        Vector3 smoothPos = Vector3.SmoothDamp(transform.position, destination, ref velocity, 0.05f);
        transform.position = smoothPos;
    }


    private void SnapToSurfaceBelow()
    {
        Vector3 rayStart = new Vector3(transform.position.x, transform.position.y + raycastHeight, transform.position.z);
        Ray ray = new Ray(rayStart, Vector3.down);
        RaycastHit hit;

        Debug.DrawRay(
            rayStart,
            Vector3.down * (raycastHeight + 0.1f),
            Color.yellow
        );

        if (Physics.Raycast(ray, out hit, raycastHeight + 100f))
        {
            // 3) Snap directly onto the hit point
            transform.position = hit.point;
                        Debug.DrawLine(
                rayStart,
                hit.point,
                Color.green
            );
        }
    }

    

}
