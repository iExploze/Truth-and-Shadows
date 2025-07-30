using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSeethru : MonoBehaviour
{
    public Transform playerTransform;
    public Transform squidTransform;
    public StateManager stateManager; // Assign in Inspector

    private Dictionary<Renderer, float> seeThruObjects = new Dictionary<Renderer, float>();

    void Update()
    {
        Transform target = stateManager.isHumanForm() ? playerTransform : squidTransform;
        if (target == null) return;

        float centerDistance = Vector3.Distance(transform.position, target.position) - 1f;

        // Main ray in the center, edge rays much closer to center (0.4 and 0.6)
        Vector3[] viewportPoints = new Vector3[]
        {
            new Vector3(0.5f, 0.5f, 0), // center
            new Vector3(0.4f, 0.5f, 0), // left (closer)
            new Vector3(0.6f, 0.5f, 0)  // right (closer)
        };

        HashSet<Renderer> hitRenderers = new HashSet<Renderer>();

        bool mainRayHit = false;

        // --- MAIN RAY (always casts) ---
        {
            Ray ray = Camera.main.ViewportPointToRay(viewportPoints[0]);
            float maxRayDistance = centerDistance;
            Debug.DrawRay(ray.origin, ray.direction * maxRayDistance, Color.green);

            RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance);
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

        // --- EDGE RAYS (only if main ray hit something) ---
        if (mainRayHit)
        {
            for (int i = 1; i <= 2; i++)
            {
                Ray ray = Camera.main.ViewportPointToRay(viewportPoints[i]);
                float cosTheta = Vector3.Dot(ray.direction.normalized, Camera.main.transform.forward);
                if (Mathf.Abs(cosTheta) < 0.01f) cosTheta = 0.01f * Mathf.Sign(cosTheta);
                float maxRayDistance = centerDistance / Mathf.Abs(cosTheta);

                Debug.DrawRay(ray.origin, ray.direction * maxRayDistance, Color.green);

                RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance);

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
