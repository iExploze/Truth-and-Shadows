using UnityEngine;

public interface ILightHittable
{
    // Called when the character enters the light
    void OnLightEnter(Light lightSource);

    // Called when the character exits the light
    void OnLightExit(Light lightSource);

    // Called while the character remains in the light
    void OnLightStay(Light lightSource);
}
