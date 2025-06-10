using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundScanner : MonoBehaviour
{
    [System.Serializable]
    private struct ScanPose
    {
        public float xRot;  // Pitch
        public float yRot;  // Yaw
    }

    // Sequence of rotation points (X, Y angles)
    [SerializeField]
    private ScanPose[] scanPath = new ScanPose[]
    {
        new ScanPose { xRot = 90f, yRot = 0f },   // Start: Pointing down
        //new ScanPose { xRot = 45f, yRot = 0f },   // Tilt right
        new ScanPose { xRot = 0f, yRot = -45f },  // Aim forward
        new ScanPose { xRot = 0f, yRot = -90f },  // Aim farther across
        new ScanPose { xRot = 45f, yRot = -90f }, // Tilt left
        new ScanPose { xRot = 90f, yRot = 0f }    // Back to center
    };

    public float transitionSpeed = 1.5f; // How fast to interpolate between poses

    private int currentPose = 0;
    private float t = 0f;
    private Quaternion startRot;
    private Quaternion targetRot;

    void Start()
    {
        SetNextRotation();
    }

    void Update()
    {
        t += Time.deltaTime * transitionSpeed;
        transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

        if (t >= 1f)
        {
            currentPose = (currentPose + 1) % scanPath.Length;
            SetNextRotation();
        }
    }

    void SetNextRotation()
    {
        startRot = transform.rotation;
        var pose = scanPath[currentPose];
        targetRot = Quaternion.Euler(pose.xRot, pose.yRot, 0f);
        t = 0f;
    }
}