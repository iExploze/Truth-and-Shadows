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

    public Image border;
    public TextMeshProUGUI keyboardkey;
    public TextMeshProUGUI xboxkey;
    public TextMeshProUGUI pskey;
    public TextMeshProUGUI switchkey;
    public TextMeshProUGUI promptTMP;

    [Header("Rotation Settings")]
    [SerializeField] private bool facePlayer = false;

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

        SetAlpha(border, 0f);
        SetAlpha(keyboardkey, 0f);
        SetAlpha(xboxkey, 0f);
        SetAlpha(pskey, 0f);
        SetAlpha(switchkey, 0f);
        SetAlpha(promptTMP, 0f);
    }

    void Update()
    {
        if (player == null || targetObject == null) return;

        float distance = Vector3.Distance(player.position, targetObject.position);

        float keyboardAlpha = distance <= outerRange ? 1f : 0f;
        float controllerAlpha = distance <= midRange ? 1f : 0f;
        float promptAlpha = distance <= closeRange ? 1f : 0f;

        FadeGraphic(border, keyboardAlpha);
        FadeTMP(xboxkey, controllerAlpha);
        FadeTMP(pskey, controllerAlpha);
        FadeTMP(switchkey, controllerAlpha);
        FadeTMP(promptTMP, promptAlpha);
        FadeTMP(keyboardkey, keyboardAlpha);

        if (facePlayer)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180f, 0);
        }
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