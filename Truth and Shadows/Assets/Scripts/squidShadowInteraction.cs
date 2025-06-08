using System.Collections;
using UnityEngine;

public class squidShadowInteraction : MonoBehaviour, ILightHittable
{
    public StateManager playerStateManager;
    public bool isInLight = false;

    private float timeInLight = 0f;
    private bool hasTransformed = false;
    private const float transformDelay = 0.01f;

    public void OnLightEnter(Light lightSource)
    {
        isInLight = true;
        timeInLight = 0f;
        hasTransformed = false;
    }

    public void OnLightStay(Light lightSource)
    {
        isInLight = true;

        if (!hasTransformed)
        {
            timeInLight += Time.deltaTime;
            if (timeInLight >= transformDelay)
            {
                hasTransformed = true;
                if (playerStateManager != null)
                {
                    playerStateManager.SwitchToHumanForm();
                }
                else
                {
                    Debug.LogWarning("StateManager not assigned to squidShadowInteraction!");
                }
            }
        }
    }

    public void OnLightExit(Light lightSource)
    {
        isInLight = false;
        timeInLight = 0f;
        hasTransformed = false;
    }

    void Start()
    {
        if (playerStateManager == null)
            playerStateManager = FindObjectOfType<StateManager>();
    }
}
