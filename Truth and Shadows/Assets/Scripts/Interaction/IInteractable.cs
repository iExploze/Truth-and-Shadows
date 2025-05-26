using UnityEngine;

public interface IInteractable
{
    void StartInteraction();
    void ContinueInteraction();
    void EndInteraction();
    bool RequiresContinuousInteraction { get; }
}
