# Interaction Input System Documentation

## Overview

This document explains how the Interaction Input System works in Truth and Shadows, particularly focusing on the relationship between the `InputManager`, `PlayerController`, and `InteractionManager` components.

## Input Flow

1. **InputManager** - Processes raw input (keyboard, mouse, controller)
   - Exposes high-level input properties like `InteractPressed`, `PickupHeld`, etc.
   - Reports all input states independently without exclusivity logic

2. **PlayerController** - Manages player state based on inputs
   - Implements exclusivity logic in `UpdatePlayerState()`
   - Maintains a state machine with priority order (UI > Cutscene > Aiming > Pickup > Interact > Normal)
   - Adjusts movement, camera, and other behaviors based on current state

3. **InteractionManager** - Handles interaction with objects
   - Consumes input state from InputManager (`InteractPressed`, `PickupHeld`, etc.)
   - Performs raycast or proximity checks to find interactable objects
   - Manages interaction lifecycle (start, continue, end)
   - Handles camera switching during interactions

## Interaction Process

When a player presses the interaction button:

1. `InputManager` detects the button press and sets `InteractPressed = true`
2. `PlayerController` updates its state to `PlayerState.Interacting` (if no higher priority state is active)
3. `InteractionManager` detects `InteractPressed` and:
   - Performs a raycast/sphere cast to detect nearby `IInteractable` objects
   - If an interactable is found, calls its `StartInteraction()` method
   - Manages any camera transitions required for the interaction
   - Keeps track of the current interaction state

## Pickup Process

When a player presses the pickup button:

1. `InputManager` detects the button press and sets `PickupPressed = true`
2. `PlayerController` updates its state to `PlayerState.Pickup` (if no higher priority state is active)
3. `InteractionManager` detects `PickupPressed` and:
   - Performs a raycast/sphere cast to detect nearby `IInteractable` objects with `CanBePickedUp = true`
   - If a pickable object is found, calls its `StartPickup()` method
   - Manages the object movement and physics while being carried
   - Monitors for release to drop the object

## State Priority System

The player can only be in one state at a time, with priorities as follows:

1. UI - Highest priority, when interacting with UI elements
2. Cutscene - During scripted sequences
3. Aiming (Rotate) - When aiming the spotlight
4. Pickup - When picking up/carrying objects
5. Interacting - When interacting with objects/NPCs
6. Normal - Default gameplay state

This ensures that the most appropriate action is taken when multiple buttons are pressed simultaneously.

## Implementing IInteractable

To make an object interactable, it should implement the `IInteractable` interface:

```csharp
public interface IInteractable
{
    void StartInteraction();
    void ContinueInteraction();
    void EndInteraction();
    bool RequiresContinuousInteraction { get; }
    Component InteractionCamera { get; }
    bool CanBePickedUp { get; }
    void StartPickup(Transform playerTransform);
    void EndPickup();
    bool IsPickedUp { get; }
}
```

Implement these methods to define how an object responds to player interaction.
