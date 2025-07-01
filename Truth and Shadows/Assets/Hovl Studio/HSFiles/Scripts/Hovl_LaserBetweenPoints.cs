using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Hovl_LaserBetweenPoints : MonoBehaviour
{
    [Header("Laser Points")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Laser Appearance")]
    public float minLaserWidth = 2f;
    public float maxLaserWidth = 5f;
    public Color laserColor = Color.red;
    public float mainTextureLength = 1f;
    public float noiseTextureLength = 1f;
    public Material laserMaterial;

    [Header("Hit Effect (Optional)")]
    // Assign this to the already-instantiated scene object you want to use as the hit effect
    public GameObject hitEffect;
    public float hitEffectOffset = 0f;

    [Header("Start Effect (Optional)")]
    // Assign this to the already-instantiated scene object you want to use as the start effect
    public GameObject startEffect;
    public float startEffectOffset = 0f;

    [Header("Laser Direction")]
    public bool reverseDirection = false;

    [Header("Testing")]
    public KeyCode testToggleKey = KeyCode.Alpha0;

    [Header("Laser Logic")]
    public int damageOverTime = 30;
    public float maxLength = 100f;
    public bool useRaycast = true;
    public bool useLaserRotation = false;

    [Header("Laser Speed")]
    public float laserSpeed = 5f;

    [Header("Laser Animation")]
    public float textureScrollSpeed = 1f;

    [SerializeField]
    private bool laserActive = true;

    private LineRenderer lineRenderer;
    private ParticleSystem[] startEffects;
    private ParticleSystem[] hitEffects;
    private bool updateSaver = false;
    private Vector4 length = new Vector4(1, 1, 1, 1);
    private float currentLaserLength = 0f;
    private float textureScrollOffset = 0f;
    private Transform originalStartPoint;
    private Transform originalEndPoint;
    private bool hasStoredOriginals = false;

    // --- New: Store GameObjects for endpoints ---
    public GameObject startPointGO;
    public GameObject endPointGO;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (laserMaterial != null)
            lineRenderer.material = laserMaterial;
        lineRenderer.startWidth = minLaserWidth;
        lineRenderer.endWidth = maxLaserWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.startColor = laserColor;
        lineRenderer.endColor = laserColor;
        if (hitEffect != null)
            hitEffects = hitEffect.GetComponentsInChildren<ParticleSystem>();
        if (startEffect != null)
            startEffects = startEffect.GetComponentsInChildren<ParticleSystem>();
        else
            startEffects = GetComponentsInChildren<ParticleSystem>();
    }

    public void SetLaserActive(bool active)
    {
        laserActive = active;
        if (lineRenderer != null)
            lineRenderer.enabled = active;
        if (hitEffect != null)
            hitEffect.SetActive(active);
    }

    void Update()
    {
        // Determine start and end positions
        Vector3 startPos =
            startPointGO != null
                ? startPointGO.transform.position
                : (startPoint != null ? startPoint.position : transform.position);
        Vector3 endPos =
            endPointGO != null
                ? endPointGO.transform.position
                : (
                    endPoint != null
                        ? endPoint.position
                        : (startPos + transform.forward * maxLength)
                );

        if (!laserActive)
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
            if (hitEffect != null && hitEffect.activeSelf)
                hitEffect.SetActive(false);
            if (startEffect != null && startEffect.activeSelf)
                startEffect.SetActive(false);
            if (startEffects != null)
            {
                foreach (var effect in startEffects)
                {
                    if (effect.isPlaying)
                        effect.Stop();
                }
            }
            if (hitEffects != null)
            {
                foreach (var effect in hitEffects)
                {
                    if (effect.isPlaying)
                        effect.Stop();
                }
            }
            return;
        }
        if (lineRenderer != null)
            lineRenderer.enabled = true;

        Vector3 laserDir = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);
        RaycastHit hit;
        Vector3 hitPoint = endPos;
        float targetLength = distance;
        if (
            useRaycast
            && Physics.Raycast(startPos, laserDir, out hit, Mathf.Min(maxLength, distance))
        )
        {
            hitPoint = hit.point;
            targetLength = Vector3.Distance(startPos, hitPoint);
        }
        else if (distance > maxLength)
        {
            hitPoint = startPos + laserDir * maxLength;
            targetLength = maxLength;
        }

        // Set positions
        Vector3[] positions = new Vector3[3];
        positions[0] = startPos;
        positions[1] = startPos + (hitPoint - startPos) * 0.1f; //
        positions[2] = hitPoint;
        lineRenderer.positionCount = 3;
        lineRenderer.SetPositions(positions);

        // Set fixed width (no AnimationCurve, just like Hovl_Laser)
        // --- Width curve setup ---

        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(new Keyframe(0.0f, 0.0f));
        widthCurve.AddKey(new Keyframe(0.01f, minLaserWidth));
        widthCurve.AddKey(new Keyframe(1.0f, maxLaserWidth));

        // Smooth out the tangents (linear transitions with no sudden spikes)
        for (int i = 0; i < widthCurve.length; i++)
        {
            Keyframe key = widthCurve[i];
            key.inTangent = 0f;
            key.outTangent = 0f;
            widthCurve.MoveKey(i, key);
        }

        lineRenderer.widthCurve = widthCurve;
        lineRenderer.widthMultiplier = 1.0f; // Let the curve define the actual widths

        // Set color
        lineRenderer.startColor = laserColor;
        lineRenderer.endColor = laserColor;

        // Texture tiling/scaling (no scrolling, no reverse)
        float mainTexScale = mainTextureLength * Vector3.Distance(startPos, hitPoint);
        float noiseTexScale = noiseTextureLength * Vector3.Distance(startPos, hitPoint);
        if (lineRenderer.material != null)
        {
            lineRenderer.material.SetTextureScale("_MainTex", new Vector2(mainTexScale, 1));
            lineRenderer.material.SetTextureScale("_Noise", new Vector2(noiseTexScale, 1));
        }

        // Hit effect (end of laser)
        if (hitEffect != null)
        {
            Vector3 normal =
                (
                    useRaycast
                    && Physics.Raycast(startPos, laserDir, out hit, Mathf.Min(maxLength, distance))
                )
                    ? hit.normal
                    : laserDir;
            hitEffect.transform.position = hitPoint + normal * hitEffectOffset;
            if (useLaserRotation)
                hitEffect.transform.rotation = transform.rotation;
            else
                hitEffect.transform.LookAt(hitPoint + normal);
            if (!hitEffect.activeSelf)
                hitEffect.SetActive(true);
            if (hitEffects != null)
            {
                foreach (var effect in hitEffects)
                {
                    if (!effect.isPlaying)
                        effect.Play();
                    // Set particle color to match laser color
                    var main = effect.main;
                    main.startColor = laserColor;
                }
            }
        }
        // Start effect (start of laser)
        if (startEffect != null)
        {
            Vector3 startNormal = -laserDir;
            startEffect.transform.position = startPos + startNormal * startEffectOffset;
            if (useLaserRotation)
                startEffect.transform.rotation = transform.rotation;
            else
                startEffect.transform.LookAt(startPos + startNormal);
            if (!startEffect.activeSelf)
                startEffect.SetActive(true);
            if (startEffects != null)
            {
                foreach (var effect in startEffects)
                {
                    if (!effect.isPlaying)
                        effect.Play();
                    // Set particle color to match laser color
                    var main = effect.main;
                    main.startColor = laserColor;
                }
            }
        }
    }

    public bool IsLaserActive()
    {
        return laserActive;
    }

    /// <summary>
    /// Updates the laser's start and end transforms, or resets them to the original values if reset is true.
    /// </summary>
    public void UpdateLaserTransforms(Transform newStart, Transform newEnd, bool reset)
    {
        if (!hasStoredOriginals)
        {
            originalStartPoint = startPoint;
            originalEndPoint = endPoint;
            hasStoredOriginals = true;
        }
        if (reset)
        {
            startPoint = originalStartPoint;
            endPoint = originalEndPoint;
        }
        else
        {
            startPoint = newStart;
            endPoint = newEnd;
        }
    }

    /// <summary>
    /// Updates the laser's start and end GameObjects, or resets them to the original values if reset is true.
    /// </summary>
    public void UpdateLaserGameObjects(GameObject newStartGO, GameObject newEndGO, bool reset)
    {
        if (!hasStoredOriginals)
        {
            originalStartPoint = startPoint;
            originalEndPoint = endPoint;
            hasStoredOriginals = true;
        }
        if (reset)
        {
            startPointGO = null;
            endPointGO = null;
            startPoint = originalStartPoint;
            endPoint = originalEndPoint;
        }
        else
        {
            startPointGO = newStartGO;
            endPointGO = newEndGO;
            startPoint = (startPointGO != null) ? startPointGO.transform : null;
            endPoint = (endPointGO != null) ? endPointGO.transform : null;
        }
    }
}
