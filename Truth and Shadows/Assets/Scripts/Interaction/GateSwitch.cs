using UnityEngine;

public class GateSwitch : MonoBehaviour, IInteractable
{
    private GameObject targetGate;
    public bool requireContinuousHold = false; // One-time interaction

    public bool RequiresContinuousInteraction => requireContinuousHold;

    void Start()
    {
        targetGate = GameObject.Find("Gate");
        if (targetGate == null)
        {
            Debug.LogWarning("No gate found for switch: " + gameObject.name);
        }
    }

    public void StartInteraction()
    {
        if (targetGate != null)
        {
            targetGate.SetActive(false);
        }
    }

    public void ContinueInteraction() { }

    public void EndInteraction() { }
}