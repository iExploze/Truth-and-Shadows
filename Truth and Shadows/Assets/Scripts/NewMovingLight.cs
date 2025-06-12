using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMovingLight : MonoBehaviour
{
    public float radius = 5f;          // Radius of the circle
    public float speed = 1f;           // Speed of the movement
    private Vector3 center;            // Center point of the circle
    private float angle = 0f;          // Current angle in radians
    private int direction = 1;         // 1 for CCW, -1 for CW

    void Start()
    {
        center = transform.position;
    }

    void Update()
    {
        // Toggle direction on key press
        if (Input.GetKeyDown(KeyCode.B))
        {
            direction *= -1; // Flip the direction
        }

        // Update angle with direction
        angle += direction * speed * Time.deltaTime;

        // Compute new position
        float x = center.x + Mathf.Cos(angle) * radius;
        float z = center.z + Mathf.Sin(angle) * radius;
        float y = center.y;

        transform.position = new Vector3(x, y, z);
    }
}
