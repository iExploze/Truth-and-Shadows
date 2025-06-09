using UnityEngine;

public class movingLightDemo : MonoBehaviour
{
    // simple script for circling the light for testing
    public float radius = 5f;      // Radius of the circle
    public float speed = 1f;       // Speed of the movement
    private Vector3 center;        // Center point of the circle
    private float angle = 0f;      // Current angle in radians

    void Start()
    {
        // Set the center to where the light starts
        center = transform.position;
    }

    void Update()
    {
        // Increment the angle over time
        angle += speed * Time.deltaTime;

        // Calculate new position
        float x = center.x + Mathf.Cos(angle) * radius;
        float z = center.z + Mathf.Sin(angle) * radius;
        float y = center.y; // Keep the height constant

        transform.position = new Vector3(x, y, z);
    }
}
