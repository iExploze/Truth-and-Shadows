using System.Linq;
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

        public override void StartInteraction()
        {
            Debug.Log($"Block interactables can only be picked up, not interacted with directly.");
        }

        public override void StartPickup(Transform playerTransform)
        {
            // Call base class to handle physics setup and state changes
            base.StartPickup(playerTransform);

            // IsPickedUp is set in the base class. If it failed, don't proceed.
            if (!IsPickedUp)
                return;

            isCollidingWithWall = false;
            if (enablePickupColor)
            {
                targetColor = pickedUpColor;
                isColorChanging = true;
            }
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
            // // Handle the smooth color transition
            // if (isColorChanging && cubeRenderer != null)
            // {
            //     currentColor = Color.Lerp(
            //         currentColor,
            //         targetColor,
            //         Time.deltaTime * colorChangeSpeed
            //     );
                
            //     // Modify the material color without replacing the materials array
            //     // This preserves the outline materials that were added by the Outline component
            //     var mats = cubeRenderer.materials;
            //     if (mats.Length > 0 && mats[0].color != currentColor)
            //     {
            //         // Create a new material instance to avoid modifying the shared material
            //         mats[0] = new Material(mats[0]) { color = currentColor };
            //         cubeRenderer.materials = mats;
            //     }

            //     if (
            //         Vector3.Distance(
            //             new Vector3(currentColor.r, currentColor.g, currentColor.b),
            //             new Vector3(targetColor.r, targetColor.g, targetColor.b)
            //         ) < 0.01f
            //     )
            //     {
            //         currentColor = targetColor;
            //         var finalMats = cubeRenderer.materials;
            //         if (finalMats.Length > 0)
            //         {
            //             finalMats[0] = new Material(finalMats[0]) { color = currentColor };
            //             cubeRenderer.materials = finalMats;
            //         }
            //         isColorChanging = false;
            //     }
            // }

            // // Call base update for outline effects
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
            Vector3[] directions = new Vector3[]
            {
                Vector3.forward,
                Vector3.back,
                Vector3.right,
                Vector3.left,
            };

            isCollidingWithWall = directions.Any(dir => 
                Physics.Raycast(transform.position, dir, 0.75f, ~LayerMask.GetMask("Player")));
        }
    }
}
