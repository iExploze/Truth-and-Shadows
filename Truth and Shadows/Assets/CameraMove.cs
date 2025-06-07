using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform cameraPosition;

    void Update()
    {
        //atatches camera position to game object AKA Cam Holder
        transform.position = cameraPosition.position;
    }
}
