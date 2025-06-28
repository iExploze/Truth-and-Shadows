using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WallTextureScaler : MonoBehaviour
{
    public Vector2 baseTiling = new Vector2(1, 1); // base tiling for scale = (1,1,1)

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        Vector3 scale = transform.lossyScale;
        rend.material.mainTextureScale = new Vector2(scale.x * baseTiling.x, scale.y * baseTiling.y);
    }
}
