using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustBrightness : MonoBehaviour
{
    private Light spotLight;

    // Track current player with "Player" tag
    private GameObject[] currentPlayers;
    private float _brightness;

    void Start()
    {
        // Get the Light component attached to this GameObject
        spotLight = GetComponent<Light>();

        if (spotLight == null || spotLight.type != LightType.Spot)
        {
            Debug.LogError("This script requires a Spot Light assigned.");
        }

        _brightness = 1; // default until specified otherwise
        // spotLight.innerSpotAngle = spotLight.spotAngle;
    }

    // Update is called once per frame
    void Update()
    {
        _brightness = MainMenuController.brightness;
        Debug.Log(_brightness);
        spotLight.shadowStrength = _brightness;
    }
}
