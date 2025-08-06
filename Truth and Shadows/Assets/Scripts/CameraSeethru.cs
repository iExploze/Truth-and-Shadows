using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSeethru : MonoBehaviour
{
    public Transform playerTransform;
    public Transform squidTransform;
    public StateManager stateManager; // Assign in Inspector

    public Material seeThruMaterial; // Assign your transparent material here!

    private Dictionary<Renderer, Material> originalMats = new Dictionary<Renderer, Material>();

    public float sideRayAngle = 5f;

    void Update()
    {
        Transform target = stateManager.isHumanForm() ? playerTransform : squidTransform;
        if (target == null) return;

        Vector3 toTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position) - 1f;

        Vector3 leftDir = Quaternion.AngleAxis(-sideRayAngle, Vector3.up) * toTarget;
        Vector3 rightDir = Quaternion.AngleAxis(sideRayAngle, Vector3.up) * toTarget;

        Vector3[] directions = new Vector3[]
        {
            toTarget,
            leftDir,
            rightDir
        };

        HashSet<Renderer> hitRenderers = new HashSet<Renderer>();
        bool mainRayHit = false;

        // --- MAIN RAY ---
        {
            Ray ray = new Ray(transform.position, directions[0]);
            Debug.DrawRay(ray.origin, ray.direction * distanceToTarget, Color.green);

            RaycastHit[] hits = Physics.RaycastAll(ray, distanceToTarget);
            mainRayHit = hits.Length > 0;

            foreach (var hit in hits)
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend != null)
                {
                    hitRenderers.Add(rend);
                    SwapToSeeThru(rend);
                }
            }
        }

        // --- SIDE RAYS ---
        if (mainRayHit)
        {
            for (int i = 1; i <= 2; i++)
            {
                Ray ray = new Ray(transform.position, directions[i]);
                Debug.DrawRay(ray.origin, ray.direction * distanceToTarget, Color.yellow);

                RaycastHit[] hits = Physics.RaycastAll(ray, distanceToTarget);

                foreach (var hit in hits)
                {
                    Renderer rend = hit.collider.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        hitRenderers.Add(rend);
                        SwapToSeeThru(rend);
                    }
                }
            }
        }

        // --- Restore any that aren't hit ---
        var prevObjects = new List<Renderer>(originalMats.Keys);
        foreach (var rend in prevObjects)
        {
            if (!hitRenderers.Contains(rend))
            {
                RestoreOriginal(rend);
            }
        }
    }

    void SwapToSeeThru(Renderer rend)
    {
        if (!originalMats.ContainsKey(rend))
        {
            originalMats[rend] = rend.material; // Save original material
            rend.material = seeThruMaterial;    // Set see-thru material
        }
    }

    void RestoreOriginal(Renderer rend)
    {
        if (originalMats.ContainsKey(rend))
        {
            rend.material = originalMats[rend]; // Restore original
            originalMats.Remove(rend);
        }
    }
}
