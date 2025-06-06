using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class squidShadowInteraction : MonoBehaviour, ILightHittable
{
    public StateManager playerStateManager;
    public bool isInLight = false;
    public void OnLightEnter(Light lightSource)
    {
        isInLight = true;
        playerStateManager.ReturnToNormalForm();
    }

    public void OnLightExit(Light lightSource)
    {
        isInLight = false;
    }

    public void OnLightStay(Light lightSource)
    {
        isInLight = true;
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
