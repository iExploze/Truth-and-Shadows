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

    [Tooltip("LayerMask for any surface the indicator can snap to (e.g. \"Ground\").")]
    public LayerMask groundMask;

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
    }


    private void SnapToSurfaceBelow()
    {

    }

    

}
