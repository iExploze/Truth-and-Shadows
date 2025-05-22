using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shadowCharacterDetection : MonoBehaviour, ILightHittable
{
    public void OnLightEnter(Light lightSource)
    {
        Debug.Log("get in light");
    }

    public void OnLightExit(Light lightSource)
    {
        Debug.Log("get out light");
    }

    public void OnLightStay(Light lightSource)
    {
        Debug.Log("in light");
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
