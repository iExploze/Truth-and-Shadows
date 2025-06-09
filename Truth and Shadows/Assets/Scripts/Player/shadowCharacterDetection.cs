using System.Collections;
using System.Collections.Generic;
using Cinemachine.Examples;
using UnityEngine;

public class shadowCharacterDetection : MonoBehaviour, ILightHittable
{
    // Size of the circular buffer that holds safe positions
    private const int bufferSize = 60;
    private Vector3[] lastSafePositions = new Vector3[bufferSize];
    private int positionIndex = 0;
    private int validPositions = 0;

    // Whether the player is currently considered “in light”
    private bool isInLight = false;

    // Timer to measure how long the player has stayed in light
    private float timeInLight = 0f;
    private const float maxTimeInLight = 0.3f;

    private ShadowCharacterMovement shadowMovement;
    public StateManager playerStateManager;

    void Start()
    {
        shadowMovement = GetComponent<ShadowCharacterMovement>();

        // Initialize the buffer with the current position
        for (int i = 0; i < bufferSize; i++)
        {
            lastSafePositions[i] = transform.position;
        }
        validPositions = 1;
    }

    void Update()
    {
        if (!isInLight) 
        {
            // If not in light, keep “rolling” the circular buffer of safe positions
            lastSafePositions[positionIndex] = transform.position;
            positionIndex = (positionIndex + 1) % bufferSize;

            if (validPositions < bufferSize)
                validPositions++;
        }
    }

    public void OnLightEnter(Light lightSource)
    {
        // Immediately disable movement
        shadowMovement.canMove = false;
        //.Log("Entered light: backtracking to last safe position.");

        // Find the most recent safe position:
        int latestIndex = (positionIndex - 1 + bufferSize) % bufferSize;
        transform.position = lastSafePositions[latestIndex];

        // Mark that we are now in light and reset the timer
        isInLight = true;
        timeInLight = 0f;
    }

    public void OnLightExit(Light lightSource)
    {
        // As soon as the player leaves the light, re-enable movement
        shadowMovement.canMove = true;
        //Debug.Log("Exited light: back to shadow form.");

        isInLight = false;
    }

    public void OnLightStay(Light lightSource)
    {
        // Count time spent in light
        timeInLight += Time.deltaTime;

        // If the player has been in light for more than 0.5s, force return to normal form
        if (timeInLight >= maxTimeInLight)
        {

            // Re-enable movement and reset light state
            shadowMovement.canMove = true;
            isInLight = false;
            timeInLight = 0f;
        }
    }
}
