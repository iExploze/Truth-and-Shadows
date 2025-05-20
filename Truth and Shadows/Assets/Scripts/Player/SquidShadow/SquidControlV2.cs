using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SquidControlV2 : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] 
    private Transform visualRoot;

    [Header("Movement Settings")]
    [SerializeField] 
    private float moveSpeed = 5f;
    
    [SerializeField] 
    private float turnSpeed = 10f;
    
    [SerializeField] 
    private float surfaceStickForce = 200f;

    [Header("Surface Detection")]
    [SerializeField] 
    private float surfaceCheckRadius = 0.8f;
    
    [SerializeField] 
    private LayerMask surfaceLayers;
    
    [SerializeField] 
    private bool showDebug = true;

    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 currentNormal = Vector3.up;
    private Vector3 inputDirection;
    private Vector2 moveInput;
    private bool isOnWall;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        GetInput();
        UpdateVisualRoot();
    }

    private void FixedUpdate()
    {
        DetectSurface();
        Move();
        ApplySurfaceStick();
    }

    private void GetInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (isOnWall)
        {
            // When on wall, use world up/down for vertical movement
            var wallUp = Vector3.ProjectOnPlane(Vector3.up, currentNormal).normalized;
            var wallRight = Vector3.Cross(currentNormal, wallUp).normalized;
            inputDirection = wallRight * moveInput.x + wallUp * moveInput.y;
        }
        else
        {
            // Regular ground movement using camera direction
            var forward = Vector3.ProjectOnPlane(
                mainCamera.transform.forward, 
                currentNormal
            ).normalized;
            
            var right = Vector3.ProjectOnPlane(
                mainCamera.transform.right, 
                currentNormal
            ).normalized;
            
            inputDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        }
    }

    private void DetectSurface()
    {
        RaycastHit hit;
        
        Vector3[] directions = 
        {
            -transform.up,
            transform.right,
            -transform.right,
            transform.forward,
            -transform.forward
        };

        bool foundSurface = false;
        float closestDistance = float.MaxValue;
        Vector3 bestNormal = currentNormal;
        Vector3 targetPosition = transform.position;

        foreach (Vector3 dir in directions)
        {
            Debug.DrawRay(transform.position, dir * surfaceCheckRadius * 2f, Color.yellow);
            
            if (Physics.SphereCast(
                transform.position,
                surfaceCheckRadius,
                dir,
                out hit,
                surfaceCheckRadius * 2f,
                surfaceLayers
            ))
            {
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    bestNormal = hit.normal;
                    targetPosition = hit.point + hit.normal * surfaceCheckRadius;
                    foundSurface = true;
                    isOnWall = Vector3.Angle(hit.normal, Vector3.up) > 45f;
                }
            }
        }

        if (foundSurface)
        {
            transform.position = targetPosition;
            currentNormal = bestNormal;

            if (showDebug)
            {
                Debug.DrawRay(
                    transform.position,
                    currentNormal * 2f,
                    isOnWall ? Color.red : Color.green
                );
            }
        }
    }

    private void Move()
    {
        if (inputDirection.sqrMagnitude < 0.1f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 8f);
            return;
        }

        // Project movement onto surface
        Vector3 moveDirection = Vector3.ProjectOnPlane(inputDirection, currentNormal).normalized;
        rb.AddForce(moveDirection * moveSpeed, ForceMode.Acceleration);
    }

    private void ApplySurfaceStick()
    {
        // Strong stick force to surface
        rb.AddForce(-currentNormal * surfaceStickForce, ForceMode.Acceleration);

        // Cancel outward velocity
        Vector3 normalVelocity = Vector3.Project(rb.velocity, currentNormal);
        if (Vector3.Dot(normalVelocity, currentNormal) > 0)
        {
            rb.velocity -= normalVelocity;
        }
    }

    private void UpdateVisualRoot()
    {
        if (visualRoot == null) return;

        Quaternion targetRotation;
        if (inputDirection.sqrMagnitude > 0.1f)
        {
            targetRotation = Quaternion.LookRotation(inputDirection, currentNormal);
        }
        else
        {
            targetRotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(visualRoot.forward, currentNormal),
                currentNormal
            );
        }

        visualRoot.rotation = Quaternion.Slerp(
            visualRoot.rotation,
            targetRotation,
            Time.deltaTime * turnSpeed
        );
    }
}
