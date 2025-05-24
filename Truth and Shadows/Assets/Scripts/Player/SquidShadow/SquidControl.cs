using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
public class SquidControl : MonoBehaviour
{
    private float moveSpeed = 3f;
    private float gravityForce = 1f; // Increased to keep stuck better
    private float cohesionStrength = 10f; // Increased for better damping
    private float drag = 1f;
    private float angularDrag = 0f;

    private float surfaceNormalSmoothSpeed = 10f; // How fast surfaceNormal updates
    private float airGravityMultiplier = 10f; // How much stronger gravity is in air
    private float detectRadius = 0.2f; // How far to check for surfaces
    private float airRotationSpeed = 200f; // Faster rotation in air
    private float approachDampening = 0.5f; // Slow down when approaching surface

    private float edgeTransitionSpeed = 205f;  // Speed of rotation when transitioning around edges
    private float stabilityThreshold = 0.95f; // How closely we need to align before allowing movement
    private bool isStable = true;

    private Rigidbody rb;
    private Vector3 surfaceNormal = Vector3.up;
    private Vector3 targetSurfaceNormal = Vector3.up; // Smoothed target normal
    private Vector3 inputDirection;
    private Vector3 nearestSurfaceNormal = Vector3.up;
    private float currentRotationSpeed;

    private bool onFloor = false;
    private bool onWall = false;
    private bool isAirborne = false;

    private Vector3 initialForward;

    [Header("Movement Settings")]
    [SerializeField] private bool useCharacterForward = false;
    [SerializeField] private bool lockToCameraForward = false;
    [SerializeField] private float turnSpeed = 10f;
    
    private Camera mainCamera;
    private Vector2 input;
    private Vector3 targetDirection;
    private float turnSpeedMultiplier;
    private float speed;

    private enum SurfaceType
    {
        XZ, // Floor-like
        YX, // Wall-like (left/right)
        YZ, // Wall-like (front/back)
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.constraints = RigidbodyConstraints.None; // We'll handle rotation constraints manually
        initialForward = transform.forward;
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Get input relative to camera
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        
        UpdateTargetDirection();
    }

    void FixedUpdate()
    {
        isAirborne = !onFloor && !onWall;
        currentRotationSpeed = isAirborne ? airRotationSpeed : 10f;

        if (isAirborne)
        {
            FindNearestSurface();
            surfaceNormal = nearestSurfaceNormal;

            // Dampen velocity when approaching surface to prevent bouncing
            Vector3 towardsSurface = Vector3.Project(rb.velocity, surfaceNormal);
            if (Vector3.Dot(towardsSurface, surfaceNormal) < 0)
            {
                rb.velocity *= approachDampening;
            }
        }
        else
        {
            // Smoothly update the surface normal
            surfaceNormal = Vector3
                .Slerp(
                    surfaceNormal,
                    targetSurfaceNormal,
                    Time.fixedDeltaTime * surfaceNormalSmoothSpeed
                )
                .normalized;
        }

        ApplySurfaceCohesion();
        MoveOnSurface();
        SmoothAlignRotation();
        LimitVelocityAwayFromSurface();
    }

    void UpdateTargetDirection()
    {
        // Check if we're on a vertical surface
        bool isVertical = Vector3.Dot(surfaceNormal, Vector3.up) < 0.1f;
        
        // Invert vertical input on vertical surfaces
        float verticalInput = isVertical ? -input.y : input.y;

        if (!useCharacterForward)
        {
            turnSpeedMultiplier = 1f;
            var forward = mainCamera.transform.TransformDirection(Vector3.forward);
            var right = mainCamera.transform.TransformDirection(Vector3.right);
            
            forward = Vector3.ProjectOnPlane(forward, surfaceNormal).normalized;
            right = Vector3.ProjectOnPlane(right, surfaceNormal).normalized;
            
            targetDirection = input.x * right + verticalInput * forward;
        }
        else
        {
            turnSpeedMultiplier = 0.2f;
            var forward = transform.TransformDirection(Vector3.forward);
            var right = transform.TransformDirection(Vector3.right);
            
            // Project character directions onto surface plane
            forward = Vector3.ProjectOnPlane(forward, surfaceNormal).normalized;
            right = Vector3.ProjectOnPlane(right, surfaceNormal).normalized;
            
            targetDirection = input.x * right + Mathf.Abs(input.y) * forward;
        }

        // Convert to movement direction
        inputDirection = targetDirection.normalized;
    }

    void ApplySurfaceCohesion()
    {
        float currentGravity = gravityForce * (isAirborne ? airGravityMultiplier : 1f);
        Vector3 forceDirection = -surfaceNormal * currentGravity;
        rb.AddForce(forceDirection, ForceMode.Acceleration);

        if (!isAirborne)
        {
            Vector3 cohesionForce = -Vector3.Project(rb.velocity, surfaceNormal) * cohesionStrength;
            rb.AddForce(cohesionForce, ForceMode.Acceleration);
        }
    }

    void MoveOnSurface()
    {
        // Only allow movement when stable
        if (!isStable || inputDirection.sqrMagnitude < 0.01f)
            return;

        // Project movement direction onto surface plane
        Vector3 moveDir = Vector3.ProjectOnPlane(inputDirection, surfaceNormal).normalized;

        // Apply movement force
        rb.AddForce(moveDir * moveSpeed, ForceMode.Acceleration);

        // Handle rotation
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                Time.deltaTime * turnSpeed * turnSpeedMultiplier);
        }
    }

    void SmoothAlignRotation()
    {
        Vector3 rotationAxis = Vector3.Cross(transform.up, surfaceNormal).normalized;
        float rotationAngle = Vector3.Angle(transform.up, surfaceNormal);

        isStable = Mathf.Abs(Vector3.Dot(transform.up, surfaceNormal)) > stabilityThreshold;

        if (rotationAngle > 0.15f)
        {
            float currentSpeed = isStable ? currentRotationSpeed : edgeTransitionSpeed;
            
            Quaternion targetRotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation * transform.rotation,
                Time.deltaTime * currentSpeed
            );
        }

        // Only allow forward alignment when stable
        if (isStable)
        {
            // Ensure forward direction stays aligned with initial forward
            Vector3 currentForward = transform.forward;
            Vector3 desiredForward = Vector3.ProjectOnPlane(initialForward, surfaceNormal).normalized;

            if (Vector3.Dot(currentForward, desiredForward) < 0.95f)
            {
                Quaternion forwardCorrection = Quaternion.FromToRotation(
                    currentForward,
                    desiredForward
                );
                transform.rotation = forwardCorrection * transform.rotation;
            }
        }

        rb.angularVelocity = Vector3.zero;
    }

    void LimitVelocityAwayFromSurface()
    {
        // Prevent velocity shooting away from surface, limit it smoothly
        Vector3 velocityAway = Vector3.Project(rb.velocity, surfaceNormal);
        float maxAwaySpeed = 0.25f; // tweak this if needed

        if (velocityAway.magnitude > maxAwaySpeed)
        {
            Vector3 limitedAwayVelocity = velocityAway.normalized * maxAwaySpeed;
            Vector3 tangentialVelocity = rb.velocity - velocityAway;
            rb.velocity = tangentialVelocity + limitedAwayVelocity;
        }
    }

    // Collision handlers update targetSurfaceNormal smoothly
    void OnCollisionEnter(Collision collision)
    {
        UpdateSurfaceNormalFromCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        UpdateSurfaceNormalFromCollision(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Floor"))
        {
            onFloor = false;
        }
        else if (collision.collider.CompareTag("Wall"))
        {
            onWall = false;
        }
    }

    void UpdateSurfaceNormalFromCollision(Collision collision)
    {
        Vector3 newNormal = collision.contacts[0].normal;
        bool wasAirborne = isAirborne;

        // Smooth out edge transitions by blending normals
        if (!wasAirborne && !isAirborne)
        {
            targetSurfaceNormal = Vector3.Lerp(targetSurfaceNormal, newNormal, Time.deltaTime * surfaceNormalSmoothSpeed);
        }
        else
        {
            targetSurfaceNormal = newNormal;
        }

        if (collision.collider.CompareTag("Floor"))
        {
            onFloor = true;
            onWall = false;
            targetSurfaceNormal = newNormal;
        }
        else if (collision.collider.CompareTag("Wall"))
        {
            onWall = true;
            onFloor = false;
            targetSurfaceNormal = newNormal;
        }

        // Immediate alignment when transitioning from air to surface
        if (wasAirborne)
        {
            surfaceNormal = targetSurfaceNormal;
            transform.up = surfaceNormal;
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.ProjectOnPlane(rb.velocity, surfaceNormal);
        }
    }

    void FindNearestSurface()
    {
        float nearestDistance = float.MaxValue;
        Vector3[] directions = new Vector3[]
        {
            Vector3.down,
            Vector3.up,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back,
            (-Vector3.one).normalized,
            (new Vector3(-1, -1, 1)).normalized,
            (new Vector3(1, -1, -1)).normalized,
            (new Vector3(1, -1, 1)).normalized,
        };

        foreach (Vector3 direction in directions)
        {
            RaycastHit[] hits = Physics.SphereCastAll(
                transform.position,
                0.1f,
                direction,
                detectRadius
            );

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Floor") || hit.collider.CompareTag("Wall"))
                {
                    float distance = Vector3.Distance(transform.position, hit.point);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestSurfaceNormal = hit.normal;
                    }
                }
            }
        }

        // If no surface found, default to previous normal
        if (nearestDistance == float.MaxValue)
        {
            nearestSurfaceNormal = surfaceNormal;
        }
    }
}
