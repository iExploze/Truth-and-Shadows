using Cinemachine.Examples;
using TruthAndShadows.InputSystem;
using UnityEngine;

// WASD to move, Space to sprint
public class CharacterMovementNoCamera : MonoBehaviour
{
    public Transform InvisibleCameraOrigin;

    public float StrafeSpeed = 0.1f;
    public float TurnSpeed = 3;
    public float Damping = 0.2f;
    public float VerticalRotMin = -80;
    public float VerticalRotMax = 80;
    public KeyCode sprintJoystick = KeyCode.JoystickButton2;
    public KeyCode sprintKeyboard = KeyCode.E;

    private bool isSprinting;
    private Animator anim;
    private float currentStrafeSpeed;
    private Vector2 currentVelocity;

    void OnEnable()
    {
        anim = GetComponent<Animator>();
        currentVelocity = Vector2.zero;
        currentStrafeSpeed = 0;
        isSprinting = false;
    }

    void FixedUpdate()
    {
#if ENABLE_LEGACY_INPUT_MANAGER        // Get consistent movement input across devices (WASD or left stick)
        var input = InputManager.Instance.GetMovementInput();

        // Check if spotlight aiming - block movement if rotating spotlight
        if (InputManager.Instance.GetRotateButton())
        {
            // Zero out input to prevent movement during spotlight aiming
            input = Vector2.zero;
        }

        var speed = input.y;
        speed = Mathf.Clamp(speed, -1f, 1f);
        speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref currentVelocity.y, Damping);
        anim.SetFloat("Speed", speed);
        anim.SetFloat("Direction", speed);

        // set sprinting
        isSprinting = InputManager.Instance.IsSprintHeld() && speed > 0;
        anim.SetBool("isSprinting", isSprinting);

        // strafing
        currentStrafeSpeed = Mathf.SmoothDamp(
            currentStrafeSpeed,
            input.x * StrafeSpeed,
            ref currentVelocity.x,
            Damping
        );
        transform.position += transform.TransformDirection(Vector3.right) * currentStrafeSpeed;

        // Get consistent camera input across devices (mouse or right stick)
        var rotInput = InputManager.Instance.GetLookInput();
        var rot = transform.eulerAngles;
        rot.y += rotInput.x * TurnSpeed;
        transform.rotation = Quaternion.Euler(rot);

        if (InvisibleCameraOrigin != null)
        {
            rot = InvisibleCameraOrigin.localRotation.eulerAngles;
            rot.x -= rotInput.y * TurnSpeed;
            if (rot.x > 180)
                rot.x -= 360;
            rot.x = Mathf.Clamp(rot.x, VerticalRotMin, VerticalRotMax);
            InvisibleCameraOrigin.localRotation = Quaternion.Euler(rot);
        }
#else
        InputSystemHelper.EnableBackendsWarningMessage();
#endif
    }
}
