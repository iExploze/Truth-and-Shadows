using UnityEngine;

public class CutoutFollowCamera : MonoBehaviour
{
    public float cutoutRadius = 2f;

    void LateUpdate()
    {
        Vector3 camPos = transform.position;
        Shader.SetGlobalVector("_CutoutCenter", camPos);
        Shader.SetGlobalFloat("_CutoutRadius", cutoutRadius);

        // debug:
        Debug.Log($"[Cutout] Center={camPos:F2}  Radius={cutoutRadius}");
    }
}
