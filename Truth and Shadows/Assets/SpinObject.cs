using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinObject : MonoBehaviour
{
    [Tooltip("Direction to spin in local space (e.g., Vector3.up for horizontal)")]
    public Vector3 localRotationAxis = Vector3.up;

    [Tooltip("Speed of rotation in degrees per second")]
    public float spinSpeed = 45f;

    void Update()
    {
        // Rotate around local axis
        transform.Rotate(localRotationAxis.normalized * spinSpeed * Time.deltaTime, Space.Self);
    }
}

