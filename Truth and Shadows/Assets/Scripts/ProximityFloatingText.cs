using UnityEngine;
using TMPro;

public class ProximityFloatingText : MonoBehaviour
{
    public Transform player;
    public float triggerDistance = 5f;
    public float fadeSpeed = 2f;
    public TextMeshProUGUI floatingText;

    private void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (floatingText != null)
        {
            Color c = floatingText.color;
            c.a = 0f;
            floatingText.color = c;
        }
    }

    private void Update()
    {
        if (player == null || floatingText == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        float targetAlpha = distance <= triggerDistance ? 1f : 0f;

        Color currentColor = floatingText.color;
        currentColor.a = Mathf.MoveTowards(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
        floatingText.color = currentColor;
    }
}