using Cinemachine;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    public interface IInteractable
    {
        void StartInteraction();
        void ContinueInteraction();
        void EndInteraction();
        bool RequiresContinuousInteraction { get; }

        // Optional camera that will be used during interaction
        CinemachineVirtualCamera InteractionCamera { get; }

        // Pickup functionality
        bool CanBePickedUp { get; }
        void StartPickup(Transform playerTransform);
        void EndPickup();
        bool IsPickedUp { get; }
    }
}
