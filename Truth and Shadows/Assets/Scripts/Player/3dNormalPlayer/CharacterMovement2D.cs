using TruthAndShadows.InputSystem;
using UnityEngine;

namespace TruthAndShadows.Player
{
    [AddComponentMenu("")] // Don't display in add component menu
    public class CharacterMovement2D : MonoBehaviour
    {
        public float jumpVelocity = 7f;
        public float groundTolerance = 0.2f;
        public bool checkGroundForJump = true;

        float speed = 0f;
        bool isSprinting = false;
        Animator anim;
        Vector2 input;
        float velocity;
        bool headingleft = false;
        Quaternion targetrot;
        Rigidbody rigbody;

        // Use this for initialization
        void Start()
        {
            anim = GetComponent<Animator>();
            rigbody = GetComponent<Rigidbody>();
            targetrot = transform.rotation;
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        // Update is called once per frame
        void FixedUpdate()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            // Check if InputManager exists
            if (InputManager.Instance == null)
                return;

            // Do not set input = Vector2.zero here!
            // Get horizontal movement from InputManager
            input.x = InputManager.Instance.CharacterMoveInput.x;

            // Only block player movement, not input
            if (InputContextProvider.Instance.CanMove)
            {
                // Check if direction changes
                if ((input.x < 0f && !headingleft) || (input.x > 0f && headingleft))
                {
                    if (input.x < 0f)
                        targetrot = Quaternion.Euler(0, 270, 0);
                    if (input.x > 0f)
                        targetrot = Quaternion.Euler(0, 90, 0);
                    headingleft = !headingleft;
                }
                // Rotate player if direction changes
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetrot,
                    Time.deltaTime * 20f
                );
                // set speed to horizontal inputs
                speed = Mathf.Abs(input.x);
                speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref velocity, 0.1f);
                anim.SetFloat("Speed", speed);
                // set sprinting using InputManager's IsRunning property
                isSprinting = InputManager.Instance.IsRunning && input.x != 0f;
                anim.SetBool("isSprinting", isSprinting);
            } else {
                // Block only player movement, not input
                anim.SetFloat("Speed", 0f);
                anim.SetBool("isSprinting", false);
            }
#endif
        }
#endif

        private void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            // Jump - this could be expanded to use a dedicated Jump property in InputManager
            // if that's added in the future
            if (isGrounded() && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)))
            {
                rigbody.AddForce(new Vector3(0, jumpVelocity, 0), ForceMode.Impulse);
            }
#else
            InputSystemHelper.EnableBackendsWarningMessage();
#endif
        }

        public bool isGrounded()
        {
            if (checkGroundForJump)
                return Physics.Raycast(transform.position, Vector3.down, groundTolerance);
            else
                return true;
        }
    }
}
