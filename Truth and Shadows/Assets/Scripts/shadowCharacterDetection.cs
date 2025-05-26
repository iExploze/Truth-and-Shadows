using System.Collections;
using System.Collections.Generic;
using Cinemachine.Examples;
using UnityEngine;

public class shadowCharacterDetection : MonoBehaviour, ILightHittable
{
    // this is the external script responsible for interacting
    // with the player so that it does not get in light for the shadow player
    private const int bufferSize = 60;
    private Vector3[] lastSafePositions = new Vector3[bufferSize];
    private int positionIndex = 0;
    private int validPositions = 0;

    private bool isInLight = false;
    private int backtrackIndex = -1; // -1 means not currently backtracking

    private ShadowCharacterMovement ShadowCharacterMovement;

    void Start()
    {
        ShadowCharacterMovement = GetComponent<ShadowCharacterMovement>();
        // Initialize buffer with current position
        for (int i = 0; i < bufferSize; i++)
        {
            lastSafePositions[i] = transform.position;
        }
        validPositions = 1;
    }

    void Update()
    {
        // Only store positions if not in light (i.e., safe)
        if (!isInLight)
        {
            // Store position in circular buffer
            lastSafePositions[positionIndex] = transform.position;
            positionIndex = (positionIndex + 1) % bufferSize;
            if (validPositions < bufferSize) validPositions++;
            backtrackIndex = -1; // Reset backtrack when out of light
        }
    }

    public void OnLightEnter(Light lightSource)
    {
        ShadowCharacterMovement.canMove = false;
        Debug.Log("get in light");
        // Go to the most recent safe location
        int latestIndex = (positionIndex - 1 + bufferSize) % bufferSize;
        transform.position = lastSafePositions[latestIndex];
        isInLight = true;
        backtrackIndex = latestIndex;
    }

    public void OnLightExit(Light lightSource)
    {
        ShadowCharacterMovement.canMove = true;
        Debug.Log("get out light");
        isInLight = false;
    }

    public void OnLightStay(Light lightSource)
    {
        isInLight = true;
        Debug.Log("in light");
        // Each call steps back to an older location if possible
        if (backtrackIndex == -1) return; // Only backtrack if in light

        // Step to previous index
        int prevIndex = (backtrackIndex - 1 + bufferSize) % bufferSize;

        // Don't step into uninitialized slots (less than 10 valid positions)
        if (validPositions < bufferSize && prevIndex >= validPositions)
            return;

        // Only backtrack if not looping past oldest known safe position
        if (prevIndex != positionIndex)
        {
            transform.position = lastSafePositions[prevIndex];
            backtrackIndex = prevIndex;
        }
    }
}
