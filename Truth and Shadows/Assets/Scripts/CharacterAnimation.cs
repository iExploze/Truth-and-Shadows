using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public Animator anim;

    // Stores latest movement vector from movement script
    private Vector3 movement;

    // Threshold to prevent flicker from tiny values
    public float movementThreshold = 0.01f;

    void Start()
    {
        movement = Vector3.zero;
        if (anim == null)
            anim = GetComponent<Animator>();
        if (anim == null)
            Debug.LogWarning("CharacterAnimation: Animator not assigned!");
    }

    void Update()
    {
        // Use magnitude on XZ plane only (ignore Y for jumping etc.)
        Vector2 movement2D = new Vector2(movement.x, movement.z);
        bool isMoving = movement2D.magnitude > movementThreshold;
        anim.SetBool("isMoving", isMoving);
    }

    // Call this from movement script every frame after moving
    public void updateMovement(Vector3 i)
    {
        movement = i;
    }

    // Example: Call this when you want to start "Move box anim"
    public void SetPushing(bool isPushing)
    {
        anim.SetBool("moveObject", isPushing);
    }
}
