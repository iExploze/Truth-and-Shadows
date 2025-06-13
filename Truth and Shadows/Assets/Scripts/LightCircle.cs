using UnityEngine;

public class LightCircle : MonoBehaviour
{
    public Transform centerPoint; // The point to circle around
    public float radius = 5f;     // Circle radius
    public float speed = 1f;      // Circle speed (rotations per second)
    private float angle = 0f;

    void Update()
    {
        if (centerPoint == null)
            return;

        // Increase angle based on speed and time
        angle += speed * Time.deltaTime;

        // Calculate new position
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        Vector3 newPos = centerPoint.position + new Vector3(x, 0, z);

        transform.position = newPos;

        // Make the object look at the center point
        transform.LookAt(centerPoint);
    }
}
