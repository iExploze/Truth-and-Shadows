using TruthAndShadows.Player;
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
#if ENABLE_LEGACY_INPUT_MANAGER
        // Check for InputManager existence
        if (InputManager.Instance == null)
            return;
            
        // Get movement input from the InputManager's property
        Vector2 input = InputManager.Instance.CharacterMoveInput;

        InputContextProvider.Instance.LogPermissions();

        // Only block player movement, not input
        if (InputContextProvider.Instance.CanMove)
        {
            // Handle forward/backward movement
            var speed = input.y;
            speed = Mathf.Clamp(speed, -1f, 1f);
            speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref currentVelocity.y, Damping);
            anim.SetFloat("Speed", speed);
            anim.SetFloat("Direction", speed);

            // Set sprinting state using InputManager's IsRunning property
            isSprinting = InputManager.Instance.IsRunning && speed > 0;
            anim.SetBool("isSprinting", isSprinting);

            // Handle strafing (left/right movement)
            currentStrafeSpeed = Mathf.SmoothDamp(
                currentStrafeSpeed,
                input.x * StrafeSpeed,
                ref currentVelocity.x,
                Damping
            );
            transform.position += transform.TransformDirection(Vector3.right) * currentStrafeSpeed;
        } else {
            // Block only player movement, not input
            anim.SetFloat("Speed", 0f);
            anim.SetBool("isSprinting", false);
        }

        // Get camera look input from InputManager's property
        Vector2 rotInput = InputManager.Instance.LookInput;
        
        // During pickup, use the special pickup camera input to prevent input blocking
        if (InputManager.Instance.PickupHeld)
        {
            rotInput = InputManager.Instance.PickupCameraInput;
        }
        
        // Apply horizontal rotation (turning)
        var rot = transform.eulerAngles;
        rot.y += rotInput.x * TurnSpeed;
        transform.rotation = Quaternion.Euler(rot);

        // Apply vertical rotation (looking up/down) to the camera pivot
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
