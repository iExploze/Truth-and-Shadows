using System.Collections;
using MagicPigGames;
using TruthAndShadows.CheckpointSystem; // For CheckpointManager
using UnityEngine;
using UnityEngine.SceneManagement; // For scene reset
using UnityEngine.UI; // For darkness overlay

public class squidShadowInteraction : MonoBehaviour, ILightHittable
{
    private StateManager playerStateManager;
    private bool isInLight = false;

    private bool hasTransformed = false;

    [SerializeField] private ProgressBar progressBar;

    // --- New for dark timer and camera effect ---
    public float maxTimeInDark = 10f; // Max seconds allowed in darkness
    private float timeInDark = 0f;

    [Header("UI")]
    [SerializeField] private Image darknessOverlay; // Assign a UI Image with black color and alpha 0 initially
    [SerializeField] private Image darknessOverlay2;

    void Start()
    {
        if (playerStateManager == null)
            playerStateManager = FindObjectOfType<StateManager>();
        if (darknessOverlay != null)
            SetDarknessAlpha(0f);
        if (darknessOverlay2 != null)
            darknessOverlay2.gameObject.SetActive(false);
        if (progressBar != null) 
        {
            progressBar.gameObject.SetActive(false);
        }
    }

    public void OnLightEnter(Light lightSource)
    {
        isInLight = true;
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
        setProgressBar(0f);

        if (!hasTransformed)
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

    public void OnLightExit(Light lightSource)
    {
        isInLight = false;
        hasTransformed = false;
        // Do not reset darkness timer here, so dark effect starts accumulating
    }

    void Update()
    {
        if (!isInLight)
        {
            darknessOverlay2.gameObject.SetActive(true);
            progressBar.gameObject.SetActive(true);
            timeInDark += Time.deltaTime;
            float darknessAmount = Mathf.Clamp01(timeInDark / maxTimeInDark);

            SetDarknessAlpha(darknessAmount * 1.5f);
            setProgressBar(darknessAmount);

            if (timeInDark >= maxTimeInDark)
            {
                // Instead of reloading the scene, use the CheckpointManager to respawn at checkpoint
                if (CheckpointManager.Instance != null)
                {
                    // Use the CheckpointManager's HandleShadowFormTimeout method
                    CheckpointManager.Instance.HandleShadowFormTimeout();
                    // Reset darkness effect and timer after respawn
                    timeInDark = 0f;
                    SetDarknessAlpha(0f);
                }
                else
                {
                    // Fallback to scene reload if CheckpointManager isn't available
                    Debug.LogWarning("CheckpointManager not found, falling back to scene reload");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }
        else
        {
            // In light, reset darkness timer and effect
            timeInDark = 0f;
            SetDarknessAlpha(0f);
            setProgressBar(0f);
            progressBar.gameObject.SetActive(false);
            darknessOverlay2.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
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

    private void setProgressBar(float alpha)
    {
        if (progressBar != null)
        {
            progressBar.SetProgress(alpha);
        }
    }
}
