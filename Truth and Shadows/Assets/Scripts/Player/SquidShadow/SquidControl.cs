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
    public LayerMask wallMask;

    [Header("References")]
    [Tooltip("Drag the player's root Transform here (used for clamping radius).")]
    public Transform playerRoot;

    [Tooltip("The speed the player must be lower than to toggle wall climbing")]
    public float wallSensitivity = 0.1f;

    private Rigidbody rb;

    private Vector3 surfaceNormal = Vector3.up; // default to up

    public float currentSpeed; // in units/sec

    private void Awake()
    {
        // Hide the hardware cursor:
        Cursor.visible = false;

        // (Optional) Lock it to the center if you don't want it wandering
        Cursor.lockState = CursorLockMode.Locked;
    }

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
    private bool useVerticalSnap = false;
    private float snapTimer = 0f;
    [SerializeField] private float snapHoldDuration = 0.7f; // seconds

    private void Update()
    {
        // 1) Compute movement direction
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 inputDir = (camForward * v + camRight * h).normalized;

        // 2) Check if indicator is “against” a wall right now
        bool againstWall = false;
        if (inputDir.sqrMagnitude > 0f)
        {
            Vector3 origin = transform.position;
            Vector3 dir = new Vector3(inputDir.x, 0f, inputDir.z).normalized;
            float checkDist = 0.1f;

            Debug.DrawRay(origin, dir * checkDist, Color.red);
            if (Physics.Raycast(origin, dir, out RaycastHit hit, checkDist, groundMask))
            {
                againstWall = true;
            }
        }

        // 3) If we just hit a wall, start/refresh the 0.5s snap timer
        if (againstWall)
        {
            useVerticalSnap = true;
            snapTimer = snapHoldDuration;
        }

        // 4) If vertical‐snap mode is active, call SnapVerticalDown() and count down
        if (useVerticalSnap)
        {
            SnapVerticalDown();
            snapTimer -= Time.deltaTime;
            if (snapTimer <= 0f)
            {
                useVerticalSnap = false;
            }
        }
        else
        {
            // 5) Otherwise, do the normal ground snap
            SnapDirectlyDown();
        }

        HandleHorizontalMovement();
    }

    private void HandleHorizontalMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Math.Abs(h) < 0.01f && Math.Abs(v) < 0.01f)
            return;

        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDir = camForward * v + camRight * h;
        inputDir.Normalize();

        float distance = moveSpeed * Time.deltaTime;
        Vector3 targetPos = transform.position + inputDir * distance;

        // Raycast in the movement direction to check for wall
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, inputDir, out hit, distance + 0.1f, wallMask))
        {
            // No wall, move freely
            transform.position = targetPos;
        }
        else
        {
            // Wall detected, stop right before the wall
            transform.position = hit.point - inputDir * 0.05f;
        }
    }


    private void SnapVerticalDown()
    {
        // Always kinematic—never re-enable physics:
        rb.isKinematic = true;

        // Temporarily disable our collider so we don’t hit ourselves:
        Collider selfCol = GetComponent<Collider>();
        bool wasEnabled = selfCol.enabled;
        selfCol.enabled = false;

        Vector3 rayStart = transform.position + Vector3.up * 1000 * raycastHeight ;
        float rayLength = raycastHeight + 10000f;

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

        Vector3 rayStart = transform.position + Vector3.up * 1f;
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
