using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneGlow : MonoBehaviour
{
    public Transform player;
    public float glowDistance = 5f;
    public Color farColor = Color.white;
    public Color nearColor = Color.blue;
    public float glowIntensity = 2f;
    public float rotationSpeed = 30f; // Degrees per second

    private Material runeMat;
    private Color currentColor;

    void Start()
    {
        runeMat = GetComponent<Renderer>().material;
        runeMat.EnableKeyword("_EMISSION");
        currentColor = farColor;
        UpdateGlow(currentColor);
        Debug.Log("RuneDecalGlow initialized with farColor: " + farColor + ", nearColor: " + nearColor);
    }

    void Update()
    {
        Debug.Log("Updating RuneDecalGlow...");
        float distance = Vector3.Distance(transform.position, player.position);
        Debug.Log($"Distance from player: {distance}");
        Color targetColor = distance <= glowDistance ? nearColor : farColor;
        Debug.Log($"Target color based on distance: {targetColor}");

        // Constant rotation
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        Debug.Log(
            $"Rune position: {transform.position}, Player position: {player.position}, Distance: {distance}, Current Color: {currentColor}"
        );

        // Smooth color transition
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 5f);
        UpdateGlow(currentColor);
        Debug.Log($"Updated glow color: {currentColor}");
    }

    void UpdateGlow(Color glowColor)
    {
        runeMat.SetColor("_EmissionColor", glowColor * glowIntensity);
        DynamicGI.SetEmissive(GetComponent<Renderer>(), glowColor * glowIntensity);
    }
}
