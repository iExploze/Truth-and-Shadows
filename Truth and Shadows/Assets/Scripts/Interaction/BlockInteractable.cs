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
        private float colorChangeSpeed = 5f;

        [SerializeField]
        private float cubeMass = 1000f;

        [SerializeField]
        private float minPickupDistance = 1.25f;

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
        private Rigidbody cubeRigidbody;

        private Vector3 cubeRelativePosition;
        private bool cubeHasCalculatedRelativePosition = false;
        private Transform cubePlayerTransform;

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

            cubeRigidbody = GetComponent<Rigidbody>();
            if (cubeRigidbody == null)
            {
                cubeRigidbody = gameObject.AddComponent<Rigidbody>();
            }

            cubeRigidbody.mass = cubeMass;
            cubeRigidbody.freezeRotation = true;
            cubeRigidbody.drag = 5f;

            originalColor = cubeRenderer.material.color;
            currentColor = originalColor;
            targetColor = originalColor;

            interactionDistance = pickupDetectionRadius;

            //Debug.Log(
            //    $"GiantCubeInteractable initialized on {gameObject.name} with mass: {cubeMass}"
            //);
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
            Debug.Log(
                $"Giant cube {gameObject.name} doesn't support R-key interaction. Use F to pick up."
            );
        }

        public override void StartPickup(Transform playerTransform)
        {
            base.StartPickup(playerTransform);

            cubePlayerTransform = playerTransform;

            if (cubeRigidbody != null)
            {
                cubeRigidbody.isKinematic = true;
            }

            targetColor = pickedUpColor;
            isColorChanging = true;

            Debug.Log(
                $"Giant cube {gameObject.name} picked up - changing color and following player horizontally"
            );
        }

        public override void EndPickup()
        {
            base.EndPickup();

            if (cubeRigidbody != null)
            {
                cubeRigidbody.isKinematic = false;
            }

            cubePlayerTransform = null;
            cubeHasCalculatedRelativePosition = false;

            targetColor = originalColor;
            isColorChanging = true;

            Debug.Log(
                $"Giant cube {gameObject.name} dropped - returning to physics and original color"
            );
        }

        protected override void Update()
        {
            if (isColorChanging && cubeRenderer != null)
            {
                currentColor = Color.Lerp(
                    currentColor,
                    targetColor,
                    Time.deltaTime * colorChangeSpeed
                );
                var mats = cubeRenderer.materials;
                if (mats.Length > 0) {
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
                    if (mats.Length > 0) {
                        mats[0].color = currentColor;
                    }
                    cubeRenderer.materials = mats;
                    isColorChanging = false;
                }
            }

            if (IsPickedUp && cubePlayerTransform != null)
            {
                UpdateHorizontalOnlyPickupPosition();
            }
            else
            {
                base.Update();
            }
        }

        private void UpdateHorizontalOnlyPickupPosition()
        {
            if (!cubeHasCalculatedRelativePosition)
            {
                Vector3 playerPos = cubePlayerTransform.position;
                Vector3 cubePos = transform.position;

                Vector3 horizontalOffset = new Vector3(
                    cubePos.x - playerPos.x,
                    0f,
                    cubePos.z - playerPos.z
                );

                float currentDistance = horizontalOffset.magnitude;
                if (currentDistance < minPickupDistance)
                {
                    if (currentDistance > 0.1f)
                    {
                        horizontalOffset = horizontalOffset.normalized * minPickupDistance;
                    }
                    else
                    {
                        horizontalOffset = cubePlayerTransform.forward * minPickupDistance;
                        horizontalOffset.y = 0f;
                    }
                }

                cubeRelativePosition = horizontalOffset;
                cubeHasCalculatedRelativePosition = true;
            }

            Vector3 targetHorizontalPosition = cubePlayerTransform.position + cubeRelativePosition;

            Vector3 finalTargetPosition = new Vector3(
                targetHorizontalPosition.x,
                transform.position.y,
                targetHorizontalPosition.z
            );

            transform.position = Vector3.Lerp(
                transform.position,
                finalTargetPosition,
                Time.deltaTime * pickupSmoothness
            );
        }
    }
}
