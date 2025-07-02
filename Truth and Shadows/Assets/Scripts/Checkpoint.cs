using UnityEngine;

namespace TruthAndShadows.CheckpointSystem
{
    public class Checkpoint : MonoBehaviour
    {
        [Tooltip(
            "Set this to true for this checkpoint to be auto-registered with the CheckpointManager on start"
        )]
        [SerializeField]
        private bool autoRegister = false;

        [Tooltip("Distance at which a checkpoint is considered reached")]
        [SerializeField]
        private float checkpointReachDistance = 3f;

        [Tooltip("Optional visual effect to play when checkpoint is activated")]
        [SerializeField]
        private GameObject activationEffect;

        [Tooltip("Optional secondary visual effect to play when checkpoint is activated")]
        [SerializeField]
        private GameObject secondaryActivationEffect;

        [Header("Checkpoint Visuals & Audio")]
        public Color farColor = Color.white;
        public Color activatedColor = Color.blue;
        public float glowIntensity = 2f;
        public float rotationSpeed = 30f; // Degrees per second

        [Tooltip("Optional sound to play when checkpoint is activated")]
        [SerializeField]
        private AudioClip activationSound;

        [Tooltip("AudioSource to use for playing the activation sound (optional)")]
        [SerializeField]
        private AudioSource audioSource;

        [Tooltip(
            "If true, this checkpoint is used as the initial spawn and disables effects on spawn."
        )]
        [SerializeField]
        private bool isSpawnCheckpoint = false;
        private bool effectsDisabledForSpawn = false;

        private Material runeMat;
        private bool isActivated = false;
        private Color currentColor;
        private Transform player;
        private bool isLerpingToActivated = false;
        private readonly float lerpSpeed = 5f;

        // Make effectsDisabledForSpawn public for checkpoint manager access
        public bool EffectsDisabledForSpawn => effectsDisabledForSpawn;

        void Start()
        {
            Debug.Log($"[Checkpoint] Start: {gameObject.name}");
            // Snap checkpoint to ground using a raycast
            RaycastHit hit;
            if (
                Physics.Raycast(
                    transform.position + Vector3.up * 2f,
                    Vector3.down,
                    out hit,
                    100f,
                    ~0,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                transform.position = hit.point + Vector3.up * 0.0001f; // Slightly above ground
                Debug.Log($"[Checkpoint] {gameObject.name} snapped to ground at {hit.point}");
            }
            else
            {
                Debug.LogWarning($"[Checkpoint] {gameObject.name} could not find ground below");
            }
            // Auto-register with checkpoint manager if enabled
            if (autoRegister && CheckpointManager.Instance != null)
            {
                Debug.Log(
                    $"[Checkpoint] Auto-registering {gameObject.name} with CheckpointManager"
                );
                CheckpointManager.Instance.AddCheckpoint(transform);
            }
            runeMat = GetComponent<Renderer>().material;
            runeMat.EnableKeyword("_EMISSION");
            currentColor = farColor;
            UpdateGlow(currentColor);
            Debug.Log($"[Checkpoint] Initial color set to {farColor}");
            // Find player by tag
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning($"[Checkpoint] Player not found in scene for {gameObject.name}");
        }

        void Update()
        {
            // Constant rotation
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            if (isLerpingToActivated)
            {
                currentColor = Color.Lerp(currentColor, activatedColor, Time.deltaTime * lerpSpeed);
                UpdateGlow(currentColor);
                Debug.Log(
                    $"[Checkpoint] {gameObject.name} lerping to activatedColor: {activatedColor}"
                );
                if (Vector4.Distance(currentColor, activatedColor) < 0.01f)
                {
                    currentColor = activatedColor;
                    UpdateGlow(currentColor);
                    isLerpingToActivated = false;
                    Debug.Log($"[Checkpoint] {gameObject.name} finished lerping to activatedColor");
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (
                !isActivated
                && other.CompareTag("Player")
                && CheckpointManager.Instance != null
                && CheckpointManager.Instance.GetCurrentCheckpoint() != transform
            )
            {
                Debug.Log(
                    $"[Checkpoint] Notifying CheckpointManager of activation for {gameObject.name} via trigger"
                );
                CheckpointManager.Instance.SetCheckpoint(transform);
            }
        }

        // Called when this checkpoint becomes the active checkpoint
        public void Activate()
        {
            if (isActivated && !effectsDisabledForSpawn)
            {
                Debug.Log($"[Checkpoint] {gameObject.name} already activated, skipping");
                return;
            }
            isActivated = true;
            isLerpingToActivated = true;
            effectsDisabledForSpawn = false;
            Debug.Log(
                $"[Checkpoint] {gameObject.name} activated! Starting lerp to {activatedColor}"
            );

            PlayEffect(activationEffect, "activation");
            PlayEffect(secondaryActivationEffect, "secondary activation");
            PlayActivationSound();
        }

        public void ResetActivation()
        {
            isActivated = false;
        }

        private void PlayEffect(GameObject effectObj, string effectName)
        {
            if (effectObj != null)
            {
                Debug.Log($"[Checkpoint] Playing {effectName} effect for {gameObject.name}");
                var ps = effectObj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                }
                else
                {
                    Debug.LogWarning(
                        $"[Checkpoint] {effectName}Effect on {gameObject.name} does not have a ParticleSystem component."
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[Checkpoint] No {effectName}Effect assigned for {gameObject.name}"
                );
            }
        }

        private void PlayActivationSound()
        {
            if (activationSound != null)
            {
                Debug.Log($"[Checkpoint] Playing activation sound for {gameObject.name}");
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(activationSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(activationSound, transform.position);
                }
            }
            else
            {
                Debug.LogWarning($"[Checkpoint] No activationSound assigned for {gameObject.name}");
            }
        }

        public void DisableEffectsForSpawn()
        {
            effectsDisabledForSpawn = true;
            isActivated = true;
            isLerpingToActivated = false;

            if (runeMat != null)
            {
                currentColor = activatedColor;
                UpdateGlow(currentColor);
            }

            if (activationEffect != null)
            {
                var ps = activationEffect.GetComponent<ParticleSystem>();
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (secondaryActivationEffect != null)
            {
                var ps2 = secondaryActivationEffect.GetComponent<ParticleSystem>();
                if (ps2 != null)
                    ps2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            // Disable the renderer for the spawn checkpoint
            var rend = GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = false;
            Debug.Log($"[Checkpoint] {gameObject.name} effects and renderer disabled for spawn");
        }

        void UpdateGlow(Color glowColor)
        {
            runeMat.SetColor("_EmissionColor", glowColor * glowIntensity);
            DynamicGI.SetEmissive(GetComponent<Renderer>(), glowColor * glowIntensity);
            Debug.Log(
                $"[Checkpoint] {gameObject.name} emission color updated to {glowColor * glowIntensity}"
            );
        }
    }
}
