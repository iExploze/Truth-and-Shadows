using UnityEngine;

[CreateAssetMenu(fileName = "NewCameraConfig", menuName = "Game/Camera Config")]
public class CameraConfig : ScriptableObject
{
    [Header("Camera Settings")]
    [Range(2f, 10f)]
    public float cameraDistance = 5f;
    [Range(0f, 5f)]
    public float cameraHeight = 2f;
    [Range(0f, 90f)]
    public float orbitalAngle = 45f;
    
    [Header("Rig Settings")]
    public float topRigHeight = 1.5f;
    public float middleRigHeight = 0f;
    public float bottomRigHeight = -1f;
    
    [Header("Damping")]
    [Range(0f, 2f)]
    public float followDamping = 0.5f;
}
