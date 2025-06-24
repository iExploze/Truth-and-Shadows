using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace TruthAndShadows.Interaction
{
    /// <summary>
    /// Interface for all interactable objects in the game.
    /// Provides standard methods and properties for interaction handling.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when the player starts interacting with this object
        /// </summary>
        void StartInteraction();

        /// <summary>
        /// Called every frame during an ongoing interaction
        /// </summary>
        void ContinueInteraction();

        /// <summary>
        /// Called when the player stops interacting with this object
        /// </summary>
        void EndInteraction();

        /// <summary>
        /// Whether this interactable requires the player to keep holding the interaction button
        /// </summary>
        bool RequiresContinuousInteraction { get; }

        /// <summary>
        /// Event triggered when interaction begins
        /// </summary>
        event Action<GameObject> OnInteractionStarted;

        /// <summary>
        /// Event triggered when interaction ends
        /// </summary>
        event Action<GameObject> OnInteractionEnded;

        /// <summary>
        /// Determines if the interactable can be interacted with based on custom conditions
        /// </summary>
        /// <param name="player">The player attempting to interact</param>
        /// <returns>True if interaction conditions are met, false otherwise</returns>
        bool CanInteract(MonoBehaviour player);

        // Optional camera that will be used during interaction
        // Returns a Cinemachine camera component (can be any Cinemachine camera type)
        Component InteractionCamera { get; }

        // Pickup functionality
        bool CanBePickedUp { get; }
        void StartPickup(Transform playerTransform);
        void EndPickup();
        bool IsPickedUp { get; }
    }
}
