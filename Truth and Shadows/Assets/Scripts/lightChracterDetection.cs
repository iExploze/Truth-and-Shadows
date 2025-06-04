using System.Collections;
using System.Collections.Generic;
using Cinemachine.Examples;
using UnityEngine;

// Make sure your CharacterMovement script has a public `bool canMove`
// so you can enable/disable player input from here.
public class lightCharacterDetection : MonoBehaviour, ILightHittable
{
    private const int bufferSize = 60;
    private Vector3[] lastLightPositions = new Vector3[bufferSize];
    private int positionIndex = 0;    // Next write index
    private int validPositions = 0;   // How many slots have actually been filled

    private bool isInLight = false;
    private bool isBacktracking = false;
    private int backtrackIndex = -1;

    private CharacterMovement characterMovement;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();

        // Pre‐fill the entire array with the starting position so there’s always
        // at least one “light” coordinate to backtrack to.
        for (int i = 0; i < bufferSize; i++)
        {
            lastLightPositions[i] = transform.position;
        }

        validPositions = 1;
        positionIndex = 1;
    }

    void Update()
    {
        // 1) If we're currently in light and NOT backtracking, record our position:
        if (isInLight && !isBacktracking)
        {
            lastLightPositions[positionIndex] = transform.position;
            positionIndex = (positionIndex + 1) % bufferSize;

            if (validPositions < bufferSize)
                validPositions++;
        }

        // 2) If we’re backtracking, walk back through the saved “light” positions:
        if (isBacktracking)
        {
            if (validPositions > 0)
            {
                // Place the player at the saved spot:
                transform.position = lastLightPositions[backtrackIndex];

                // Move the index one step “backwards” in the circular buffer:
                backtrackIndex = (backtrackIndex - 1 + bufferSize) % bufferSize;
            }
        }
    }

    // Called the instant any light first intersects the player’s collider
    public void OnLightEnter(Light lightSource)
    {
        isInLight = true;
        isBacktracking = false;
        characterMovement.canMove = true;
    }

    // Called every frame while the player remains in light
    public void OnLightStay(Light lightSource)
    {
        isInLight = true;
    }

    // Called the moment the player exits the light (i.e., steps into shadow)
    public void OnLightExit(Light lightSource)
    {
        isInLight = false;
        isBacktracking = true;
        characterMovement.canMove = false;

        // Start the backtrack from the most‐recent “in‐light” slot:
        backtrackIndex = (positionIndex - 1 + bufferSize) % bufferSize;
    }
}
