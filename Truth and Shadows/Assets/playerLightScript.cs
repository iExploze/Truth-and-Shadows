using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerLightScript : MonoBehaviour, ILightHittable
{
    public void OnLightEnter(Light lightSource)
    {
        Debug.Log("enter light");
    }

    public void OnLightExit(Light lightSource)
    {
        Debug.Log("exit lights");
    }

    public void OnLightStay(Light lightSource)
    {
        Debug.Log("in lights");
    }
}
