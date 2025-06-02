using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SquidControl : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Move Settings")]
    [Tooltip("Speed (units/sec) that the indicator moves in the horizontal plane.")]
    [SerializeField] private float moveSpeed = 10f;

    [Tooltip("Maximum radius from player within which the indicator can be placed.")]
    [SerializeField] private float maxRadius = 25f;

    [Header("Raycast Settings")]
    [Tooltip("How high above the indicator we start the downward ray.")]
    [SerializeField] private float raycastHeight = 10f;

    [Tooltip("LayerMask for any surface the indicator can snap to (e.g. \"Ground\").")]
    public LayerMask groundMask;

    [Header("References")]
    [Tooltip("Drag the player's root Transform here (used for clamping radius).")]
    public Transform playerRoot;

    // smoothing helper for horizontal movement
    private Vector3 velocity;

    private Rigidbody rb;

    private void Start()
    {
        // Put this indicator on the Ignore Raycast layer so its own collider never blocks our rays:
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        mainCamera = Camera.main;

        // Prevent physics from moving us
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void Update()
    {
        HandleHorizontalMovement();

        if (Input.GetMouseButton(0))
        {
            SnapVerticalDown();
        }
        else
        {
            SnapDirectlyDown();
        }
    }

    private void HandleHorizontalMovement()
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

    private void SnapVerticalDown()
    {
        rb.isKinematic = true;

        // Temporarily disable our own collider so the ray won't hit ourselves
        Collider selfCol = GetComponent<Collider>();
        bool wasEnabled = selfCol.enabled;
        selfCol.enabled = false;

        Vector3 rayStart = transform.position + Vector3.up * raycastHeight;
        float rayLength = raycastHeight + 100f;

        Debug.DrawRay(rayStart, Vector3.down * rayLength, Color.yellow);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayLength, groundMask))
        {
            transform.position = hit.point;
            Debug.DrawLine(rayStart, hit.point, Color.green);
        }
        else
        {
            Debug.DrawLine(rayStart, rayStart + Vector3.down * rayLength, Color.red);
        }

        selfCol.enabled = wasEnabled;
    }

    private void SnapDirectlyDown()
    {
        rb.isKinematic = false;

        Vector3 rayStart = transform.position;
        float rayLength = raycastHeight + 100f;

        Debug.DrawRay(rayStart, Vector3.down * rayLength, Color.yellow);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayLength, groundMask))
        {
            transform.position = hit.point;
            Debug.DrawLine(rayStart, hit.point, Color.green);
        }
        else
        {
            Debug.DrawLine(rayStart, rayStart + Vector3.down * rayLength, Color.red);
        }
    }
}