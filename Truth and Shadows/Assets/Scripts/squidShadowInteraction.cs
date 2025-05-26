using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class squidShadowInteraction : MonoBehaviour, ILightHittable
{
    public StateManager playerStateManager;
    public void OnLightEnter(Light lightSource)
    {
        playerStateManager.ReturnToNormalForm();
    }

    public void OnLightExit(Light lightSource)
    {

    }

    public void OnLightStay(Light lightSource)
    {
        Debug.LogWarning("Squid Bit by Light!");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
