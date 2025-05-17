using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveShadow : MonoBehaviour
{
    private float _userHorizontalInput;
    private const float scaleMovement = 0.1f;
    private Transform playerTransform;

    private float _userVertInput;
    // private Vector3 _userRot;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTransform = gameObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        _userHorizontalInput = Input.GetAxis("Vertical");
        // Debug.Log(_userHorizontalInput);
        
        _userVertInput = Input.GetAxis("Horizontal");
        // _userRot = playerTransform.rotation.eulerAngles;
        // _userRot += new Vector3(0, _userRotInput, 0);

        // playerTransform.rotation = Quaternion.Euler(_userRot);
        playerTransform.position += transform.forward * _userHorizontalInput * scaleMovement;
        playerTransform.position += transform.right * _userVertInput * scaleMovement;
    }
}