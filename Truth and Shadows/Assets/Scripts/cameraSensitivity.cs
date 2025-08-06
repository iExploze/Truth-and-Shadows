using UnityEngine;
using Cinemachine;

public class cameraSensitivity : MonoBehaviour
{
    private CinemachineFreeLook freeLookCam;

    // Set these to your normal default inspector values for reference
    public float baseXSens = 200f;
    public float baseYSens = 2f;

    void Start()
    {
        freeLookCam = GetComponent<CinemachineFreeLook>();
        UpdateSensitivity();
    }

    void Update()
    {
        UpdateSensitivity();
    }

    private void UpdateSensitivity()
    {
        float sens = MainMenuController.Sensitivity;
        if (freeLookCam != null)
        {
            freeLookCam.m_XAxis.m_MaxSpeed = baseXSens * sens;
            freeLookCam.m_YAxis.m_MaxSpeed = baseYSens * sens;
        }
    }
}
