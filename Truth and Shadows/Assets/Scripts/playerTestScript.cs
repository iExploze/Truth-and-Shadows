using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class playerTestScript : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    [SerializeField] private float jumpSpeed = 8.0f;
    private float gravity = 20.0f;
    private Camera playerCamera;
    private float lookSpeed = 2.0f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Check if running (W + LeftShift)
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W);

        // Get movement input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Normalize diagonal movement to prevent faster speed
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Calculate speed based on movement type
        float speed = isRunning ? runningSpeed : walkingSpeed;

        // Apply movement direction with speed
        Vector3 move = (forward * inputDirection.z + right * inputDirection.x) * speed;

        // Gravity and Jump Handling
        if (characterController.isGrounded && canMove)
        {
            moveDirection.y = -gravity * Time.deltaTime;

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Combine movement and gravity
        moveDirection = new Vector3(move.x, moveDirection.y, move.z);

        // Move the player
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            // Horizontal rotation (Player object Y-axis)
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            transform.Rotate(0, mouseX, 0);

            // Vertical rotation (Camera orbit around player)
            float mouseY = -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX + mouseY, -10, 80);

            // Update camera position and rotation
            Vector3 cameraOffset = new Vector3(0, 0, -5.0f);
            Quaternion rotation = Quaternion.Euler(rotationX, transform.eulerAngles.y, 0);
            playerCamera.transform.position = transform.position + rotation * cameraOffset;
            playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f); // Adjust to look at player head height
        }
    }
}