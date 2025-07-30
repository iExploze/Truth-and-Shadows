using System;
using TruthAndShadows.InputSystem;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class SquidControl : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody rb;

    [Header("Move Settings")]
    [Tooltip("Speed (units/sec) that the indicator moves in the horizontal plane.")]
    [SerializeField]
    private float moveSpeed = 10f;

    [Tooltip("Maximum radius from player within which the indicator can be placed.")]
    [SerializeField]
    private float maxRadius = 25f;

    [Tooltip("LayerMask for climbable walls.")]
    public LayerMask groundMask;

    [Header("References")]
    [Tooltip("Drag the player's root Transform here (used for clamping radius).")]
    public Transform playerRoot;

    [Header("Wall-Climb Settings")]
    [Tooltip("How fast you climb up/down the wall.")]
    [SerializeField]
    private float climbSpeed = 5f;

    [Tooltip("Max distance to detect a climbable wall.")]
    [SerializeField]
    private float climbCheckDistance = 0.2f;

    public AudioSource squidMoveSound;

    private void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();

        // Fully dynamic, gravity ON
        rb.useGravity = true;
        rb.isKinematic = false;

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void FixedUpdate()
    {
        // 1) Read input & build flat dir
        Vector2 moveInput = Vector2.zero;
        if (InputManager.Instance != null)
        {
            moveInput = InputManager.Instance.CharacterMoveInput; // x = horizontal, y = vertical
        }
        else
        {
            return;
        }

        float h = moveInput.x;
        float v = moveInput.y;

        Vector3 camF = mainCamera.transform.forward;
        camF.y = 0f;
        camF.Normalize();
        Vector3 camR = mainCamera.transform.right;
        camR.y = 0f;
        camR.Normalize();
        Vector3 inputDir = (camF * v + camR * h).normalized;

        // 2) Wall check only when pushing forward
        bool againstWall =
            inputDir.sqrMagnitude > 0f
            && Physics.Raycast(transform.position, inputDir, out _, climbCheckDistance, groundMask);

        // 3) Compose new velocity
        Vector3 newVel = rb.velocity;

        // horizontal movement
        newVel.x = inputDir.x * moveSpeed;
        newVel.z = inputDir.z * moveSpeed;

        // vertical: only override when climbing, else let gravity do its job
        if (againstWall)
        {
            newVel.y = climbSpeed;
        }
        // else leave newVel.y untouched so gravity pulls you down

        // 4) Apply velocity
        rb.velocity = newVel;

        //Rashai Was Here
        if (!squidMoveSound.isPlaying)
        {
            squidMoveSound.Play();
        }
    }
}
