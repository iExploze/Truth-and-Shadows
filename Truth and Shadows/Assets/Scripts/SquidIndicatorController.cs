using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquidIndicatorController : MonoBehaviour
{
    [SerializeField] private GameObject haloIndicator;

    void Start()
    {
        if (haloIndicator != null)
            haloIndicator.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (haloIndicator != null)
                haloIndicator.SetActive(!haloIndicator.activeSelf);
        }
    }

    // Optional: This can be used later when you implement the actual condition
    public bool IsHaloVisible()
    {
        return haloIndicator != null && haloIndicator.activeSelf;
    }
}