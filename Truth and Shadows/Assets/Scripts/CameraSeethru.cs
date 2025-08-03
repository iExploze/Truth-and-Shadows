using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSeethru : MonoBehaviour
{
    public Transform playerTransform;
    public Transform squidTransform;
    public StateManager stateManager; // Assign in Inspector

    private Dictionary<Renderer, float> seeThruObjects = new Dictionary<Renderer, float>();

    // Angle in degrees for side rays
    public float sideRayAngle = 5f; // small angle, increase for wider fan

    void Update()
    {
        Transform target = stateManager.isHumanForm() ? playerTransform : squidTransform;
        if (target == null) return;

        Vector3 toTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position) - 1f;

        // Create side directions by rotating the main direction a bit left/right around the Y axis
        Vector3 leftDir = Quaternion.AngleAxis(-sideRayAngle, Vector3.up) * toTarget;
        Vector3 rightDir = Quaternion.AngleAxis(sideRayAngle, Vector3.up) * toTarget;

        Vector3[] directions = new Vector3[]
        {
            toTarget,   // center
            leftDir,    // left
            rightDir    // right
        };

        HashSet<Renderer> hitRenderers = new HashSet<Renderer>();

        bool mainRayHit = false;

        // --- MAIN RAY (always casts) ---
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

                    if (!seeThruObjects.ContainsKey(rend))
                        seeThruObjects[rend] = rend.material.color.a;

                    Material mat = rend.material;

                    if (mat.HasProperty("_Mode"))
                    {
                        mat.SetFloat("_Mode", 3); // Transparent
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }

                    Color col = mat.color;
                    if (col.a > 0.31f)
                    {
                        col.a = 0.3f;
                        mat.color = col;
                    }
                }
            }
        }

        // --- SIDE RAYS (only if main ray hit something) ---
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

                        if (!seeThruObjects.ContainsKey(rend))
                            seeThruObjects[rend] = rend.material.color.a;

                        Material mat = rend.material;

                        if (mat.HasProperty("_Mode"))
                        {
                            mat.SetFloat("_Mode", 3); // Transparent
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            mat.SetInt("_ZWrite", 0);
                            mat.DisableKeyword("_ALPHATEST_ON");
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            mat.renderQueue = 3000;
                        }

                        Color col = mat.color;
                        if (col.a > 0.31f)
                        {
                            col.a = 0.3f;
                            mat.color = col;
                        }
                    }
                }
            }
        }

        // Restore objects not hit this frame
        var prevObjects = new List<Renderer>(seeThruObjects.Keys);
        foreach (var rend in prevObjects)
        {
            if (!hitRenderers.Contains(rend))
            {
                Material mat = rend.material;
                Color col = mat.color;
                col.a = seeThruObjects[rend];
                mat.color = col;

                if (mat.HasProperty("_Mode"))
                {
                    mat.SetFloat("_Mode", 0); // Opaque
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = -1;
                }

                seeThruObjects.Remove(rend);
            }
        }
    }
}
