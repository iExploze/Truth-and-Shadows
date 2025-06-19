using UnityEngine;

public class SpiritFaceFloat : MonoBehaviour
{
    public float bobSpeed = 2f;
    public float bobAmount = 0.2f;
    public float rotateSpeed = 30f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.localPosition = startPos + Vector3.up * newY;
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
