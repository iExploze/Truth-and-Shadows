using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Input = UnityEngine.Input;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    [SerializeField]
    private Slider _sliderBrightness;
    [SerializeField]
    private Image _blackOverlay;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void AdjustBrightness()
    {
        var tempColor = _blackOverlay.color;
        tempColor.a = _sliderBrightness.value;
        _blackOverlay.color = tempColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
