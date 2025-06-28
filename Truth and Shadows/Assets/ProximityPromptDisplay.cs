using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProximityPromptDisplay : MonoBehaviour
{
    public Transform player;
    public Transform targetObject;

    public float outerRange = 5f;
    public float midRange = 3f;
    public float closeRange = 2f;
    public float fadeSpeed = 5f;

    [Header("UI Elements")]
    //public Image circleImage;

    public Image keyboardKey;
    public Image controllerKey;
    //public TextMeshProUGUI keyTMP;
    public TextMeshProUGUI promptTMP;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (targetObject == null)
            targetObject = transform;

        SetAlpha(keyboardKey, 0f);
        SetAlpha(controllerKey, 0f);
        SetAlpha(promptTMP, 0f);
    }

    void Update()
    {
        if (player == null || targetObject == null) return;

        float distance = Vector3.Distance(player.position, targetObject.position);

        float keyboardAlpha = distance <= outerRange ? 1f : 0f;
        float controllerAlpha = distance <= midRange ? 1f : 0f;
        float promptAlpha = distance <= closeRange ? 1f : 0f;

        FadeGraphic(keyboardKey, keyboardAlpha);
        FadeGraphic(controllerKey, controllerAlpha);
        FadeTMP(promptTMP, promptAlpha);
    }

    void FadeGraphic(Graphic graphic, float targetAlpha)
    {
        if (graphic == null) return;
        Color color = graphic.color;
        color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
        graphic.color = color;
    }

    void FadeTMP(TextMeshProUGUI tmp, float targetAlpha)
    {
        if (tmp == null) return;
        Color color = tmp.color;
        color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
        tmp.color = color;
    }

    void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    void SetAlpha(TextMeshProUGUI tmp, float alpha)
    {
        if (tmp == null) return;
        Color color = tmp.color;
        color.a = alpha;
        tmp.color = color;
    }
}