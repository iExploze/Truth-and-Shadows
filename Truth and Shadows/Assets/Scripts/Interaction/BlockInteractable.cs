using TruthAndShadows.InputSystem;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// A giant cube interactable that changes color and follows the player horizontally
    /// when picked up. Heavy physics object with locked rotation and lenient pickup detection.
    /// </summary>
    public class BlockInteractable : InteractableBase
    {
        [Header("Cube Settings")]
        [SerializeField]
        private Color originalColor = Color.white;

        [SerializeField]
        private Color pickedUpColor = Color.green;

        [SerializeField]
        private Color collisionColor = Color.red;

        [SerializeField]
        private bool enableCollisionColor = true;

        [SerializeField]
        private bool enablePickupColor = true;

        [SerializeField]
        private float colorChangeSpeed = 5f;

        [SerializeField]
        private float cubeMass = 10f;

        [Header("Pickup Detection")]
        [SerializeField]
        private float pickupDetectionRadius = 4f;

        [Header("Rendering and color management")]
        [SerializeField]
        private Renderer cubeRenderer;

        [SerializeField]
        private Color currentColor;

        [SerializeField]
        private Color targetColor;

        [SerializeField]
        private bool isColorChanging = false;

        private bool isCollidingWithWall = false;

        protected override void Start()
        {
            base.Start();

            // Set movement style for base class to handle push/pull physics
            movementStyle = PickupMovementStyle.HorizontalPushPull;

            cubeRenderer = GetComponent<Renderer>();
            if (cubeRenderer == null)
            {
                cubeRenderer = GetComponentInChildren<Renderer>();
            }

            if (cubeRenderer == null)
            {
                Debug.LogError($"GiantCubeInteractable on {gameObject.name}: No Renderer found!");
                return;
            }

            // Configure the rigidbody from the base class
            if (rigidBody != null)
            {
                rigidBody.mass = cubeMass;
            }

            originalColor = cubeRenderer.material.color;
            currentColor = originalColor;
            targetColor = originalColor;

            // Use pickupDetectionRadius for interaction distance
            interactionDistance = pickupDetectionRadius;
        }

        public override bool CanInteract(Vector3 playerPosition)
        {
            float centerDistance = Vector3.Distance(transform.position, playerPosition);

            if (cubeRenderer != null)
            {
                Bounds bounds = cubeRenderer.bounds;
                Vector3 closestPoint = bounds.ClosestPoint(playerPosition);
                float boundsDistance = Vector3.Distance(playerPosition, closestPoint);

                float finalDistance = Mathf.Min(centerDistance, boundsDistance);
                return finalDistance <= pickupDetectionRadius;
            }

            return centerDistance <= pickupDetectionRadius;
        }

        public override bool CanPickup(Vector3 playerPosition)
        {
            return CanInteract(playerPosition);
        }

        public override void StartInteraction()
        {
            // Check permissions from the centralized provider
            bool canInteract = true;

            // Check for interact permission from InputContextProvider if available
            if (InputContextProvider.Instance != null)
            {
                canInteract = InputContextProvider.Instance.CanInteract;

                if (!canInteract)
                {
                    Debug.LogWarning(
                        "Block interaction attempted but permission denied by InputContextProvider"
                    );
                    return; // Don't proceed with interaction if not allowed
                }
            }

            Debug.Log(
                $"Giant cube {gameObject.name} doesn't support R-key interaction. Use F to pick up."
            );
        }

        public override void StartPickup(Transform playerTransform)
        {
            // Check permissions from the centralized provider
            bool canPickupBlock = true;

            // Get permission from InputContextProvider if available
            if (InputContextProvider.Instance != null)
            {
                canPickupBlock = InputContextProvider.Instance.CanPickup;

                if (!canPickupBlock)
                {
                    Debug.LogWarning(
                        "Block pickup attempted but permission denied by InputContextProvider"
                    );
                    return; // Don't proceed with pickup if not allowed
                }
            }

            // Call base class to handle physics setup and state changes
            base.StartPickup(playerTransform);

            // IsPickedUp is set in the base class. If it failed, don't proceed.
            if (!IsPickedUp)
                return;

            // Reset collision flags
            isCollidingWithWall = false;

            if (source != null && pickUpClip != null)
            {
                source.PlayOneShot(pickUpClip);
            }

            // Reset velocity when picked up
            if (rigidBody != null)
            {
                rigidBody.velocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;
            }

            if (enablePickupColor)
            {
                targetColor = pickedUpColor;
                isColorChanging = true;
            }

            // Check for initial wall collisions
            CheckWallCollisions();

            Debug.Log(
                $"Giant cube {gameObject.name} picked up - changing color and ready to push/pull"
            );
        }

        public override void EndPickup()
        {
            if (!IsPickedUp)
                return;

            if (enablePickupColor)
            {
                targetColor = originalColor;
                isColorChanging = true;
            }

            // Call base class to handle physics and state changes
            base.EndPickup();

            Debug.Log(
                $"Giant cube {gameObject.name} dropped - returning to normal physics and original color"
            );
        }

        protected override void Update()
        {
            // Handle the smooth color transition
            if (isColorChanging && cubeRenderer != null)
            {
                currentColor = Color.Lerp(
                    currentColor,
                    targetColor,
                    Time.deltaTime * colorChangeSpeed
                );
                var mats = cubeRenderer.materials;
                if (mats.Length > 0)
                {
                    mats[0].color = currentColor;
                }
                cubeRenderer.materials = mats;

                if (
                    Vector3.Distance(
                        new Vector3(currentColor.r, currentColor.g, currentColor.b),
                        new Vector3(targetColor.r, targetColor.g, targetColor.b)
                    ) < 0.01f
                )
                {
                    currentColor = targetColor;
                    if (mats.Length > 0)
                    {
                        mats[0].color = currentColor;
                    }
                    cubeRenderer.materials = mats;
                    isColorChanging = false;
                }
            }

            // Call base update for outline effects
            base.Update();
        }

        protected override void FixedUpdate()
        {
            // Let the base class handle the movement physics
            base.FixedUpdate();

            // If picked up, check for wall collisions to update color
            if (IsPickedUp)
            {
                // Check if pickup permission has been revoked during movement
                if (
                    InputContextProvider.Instance != null
                    && !InputContextProvider.Instance.CanPickup
                )
                {
                    Debug.LogWarning("Block pickup permissions revoked - forcing drop");
                    EndPickup();
                    return;
                }

                // Update color based on wall collision status
                bool wasColliding = isCollidingWithWall;
                CheckWallCollisions();

                if (enableCollisionColor && isCollidingWithWall && !wasColliding)
                {
                    targetColor = collisionColor;
                    isColorChanging = true;
                }
                else if (enablePickupColor && !isCollidingWithWall && wasColliding)
                {
                    targetColor = pickedUpColor;
                    isColorChanging = true;
                }
            }
        }

        private void CheckWallCollisions()
        {
            isCollidingWithWall = false;

            Vector3[] directions = new Vector3[]
            {
                Vector3.forward,
                Vector3.back,
                Vector3.right,
                Vector3.left,
            };

            foreach (Vector3 dir in directions)
            {
                if (Physics.Raycast(transform.position, dir, 0.75f, ~LayerMask.GetMask("Player")))
                {
                    isCollidingWithWall = true;
                    return;
                }
            }
        }
    }
}
