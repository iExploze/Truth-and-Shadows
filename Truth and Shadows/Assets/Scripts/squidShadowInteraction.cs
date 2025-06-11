using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // For scene reset
using UnityEngine.UI; // For darkness overlay

public class squidShadowInteraction : MonoBehaviour, ILightHittable
{
    private StateManager playerStateManager;
    private bool isInLight = false;

    private float timeInLight = 0f;
    private bool hasTransformed = false;
    private const float transformDelay = 0.01f;

    // --- New for dark timer and camera effect ---
    public float maxTimeInDark = 10f; // Max seconds allowed in darkness
    private float timeInDark = 0f;

    [Header("UI")]
    public Image darknessOverlay; // Assign a UI Image with black color and alpha 0 initially

    void Start()
    {
        if (playerStateManager == null)
            playerStateManager = FindObjectOfType<StateManager>();
        if (darknessOverlay != null)
            SetDarknessAlpha(0f);
    }

    public void OnLightEnter(Light lightSource)
    {
        isInLight = true;
        timeInLight = 0f;
        hasTransformed = false;

        // Reset darkness
        timeInDark = 0f;
        SetDarknessAlpha(0f);
    }

    public void OnLightStay(Light lightSource)
    {
        isInLight = true;

        // Reset darkness timer and effect
        timeInDark = 0f;
        SetDarknessAlpha(0f);

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
        // Do not reset darkness timer here, so dark effect starts accumulating
    }

    void Update()
    {
        if (!isInLight)
        {
            timeInDark += Time.deltaTime;
            float darknessAmount = Mathf.Clamp01(timeInDark / maxTimeInDark);
            SetDarknessAlpha(darknessAmount * 1f);

            if (timeInDark >= maxTimeInDark)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            // In light, reset darkness timer and effect
            timeInDark = 0f;
            SetDarknessAlpha(0f);
        }
        // Prepare for next frame
        isInLight = false; // This forces you to call OnLightStay every frame
    }


    private void SetDarknessAlpha(float alpha)
    {
        if (darknessOverlay != null)
        {
            var color = darknessOverlay.color;
            color.a = alpha;
            darknessOverlay.color = color;
        }
    }
}
