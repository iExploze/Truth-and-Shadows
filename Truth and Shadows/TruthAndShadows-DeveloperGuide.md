# Truth and Shadows - Comprehensive Developer Guide

## Table of Contents

1. [Overview](#overview)
2. [Input System](#input-system)
3. [Camera System](#camera-system)
4. [Interaction & Pickup System](#interaction--pickup-system)
5. [Spotlight System](#spotlight-system)
6. [Custom Interactables & Events](#custom-interactables--events)
7. [Checkpoint System](#checkpoint-system)
8. [Troubleshooting](#troubleshooting)
9. [Development & Extension](#development--extension)

---

## Overview

Truth and Shadows is a Unity game with custom systems for input, camera, interaction, and object pickup. This guide provides a condensed reference for developers.

---

## Input System

- **Centralized InputManager**: Handles all input (keyboard, mouse, controllers) and exposes high-level properties (e.g., `InteractPressed`, `PickupHeld`).
- **PlayerController**: Consumes input and manages player state (normal, aiming, pickup, etc.), ensuring only one state is active at a time.
- **InputContextProvider**: Centralized permission system for input actions, allowing state-based and manual overrides.
- **Best Practice**: Always use `InputManager` for input queries and check permissions via `InputContextProvider` before acting.

### InputManager API & Usage

The `InputManager` is a singleton and the only place you should query for input. Never use Unity's `Input` class directly in your scripts.

#### Common Input Properties
- `MoveInput` / `MoveInputRaw`: Player movement (Vector2)
- `IsRunning`: Sprint toggle
- `LookInput`: Camera look (Vector2)
- `PickupCameraInput`: Special camera input during pickup
- `InteractPressed`, `InteractHeld`, `InteractReleased`: Interact button states
- `PickupPressed`, `PickupHeld`, `PickupReleased`: Pickup button states
- `RotateHeld`: Whether rotate/aim button is held
- `ResetPressed`: Reset action
- `HintPressed`, `HintHeld`, `HintReleased`: Hint button states
- `UsingController`: True if a controller is active

#### Example Usage
```csharp
if (InputManager.Instance.InteractPressed && InputContextProvider.Instance.CanInteract)
{
    // Process interaction
}

if (InputManager.Instance.PickupHeld)
{
    // Handle pickup logic
}

Vector2 move = InputManager.Instance.MoveInput;
Vector2 look = InputManager.Instance.LookInput;
```

#### Legacy Support
The InputManager also provides legacy method-based APIs for backward compatibility:
```csharp
Vector2 movement = InputManager.Instance.GetMovementInput();
bool isInteracting = InputManager.Instance.GetInteractButton();
```

#### Input Exclusivity & State
- InputManager only reports input state; exclusivity (e.g., can't interact while picking up) is handled by PlayerController and InputContextProvider.
- Always check both the input property and the relevant permission (e.g., `CanInteract`, `CanPickup`).

#### Extending InputManager
- To add new input types, add a property to InputManager and update its internal state in the `Update()` method.
- For new permissions, add a property to InputContextProvider and update its logic.

#### Best Practices
- Query input once per frame and store locally if needed.
- Use property-based API for new code; legacy methods are for compatibility only.
- Use `InputManager.Instance.UsingController` to adapt UI or controls for controller vs. mouse/keyboard.

---

## Camera System

- Uses Cinemachine for all camera control.
- `CameraControllerBootstrap` prefab sets up controller support automatically.
- Cameras can be switched during interactions (e.g., spotlight control).

---

## Interaction & Pickup System

### Core Architecture
- **IInteractable**: Interface for all interactable objects, supporting camera, pickup, and custom conditions.
- **InteractableBase**: Abstract base class implementing pickup, outline, and event logic. Supports different pickup movement styles:
  - **Held**: Object follows in front of player with smoothing.
  - **HorizontalPushPull**: Object follows player horizontally, stays on ground.
- **InteractionManager**: Handles raycasts/proximity checks, manages interaction lifecycle, and camera switching.

### Pickup System
- **Hold-to-carry**: Hold F to pick up, release to drop.
- **Pickup Raise Amount**: Object raises slightly when picked up.
- **Pickup Smoothness**: Controls how quickly object follows player.
- **Physics Handling**: Rigidbody/colliders are managed for smooth pickup/drop.
- **Multiple Players**: Checkpoint and respawn logic now supports all objects with the `Player` tag, not just a single player.

---

## Spotlight System

- **SpotlightController**: Inherits from `InteractableBase`, adds mouse-controlled aiming, rotation limits, smooth interpolation, and pivot support.
- **Camera Switching**: Automatically switches to spotlight camera during interaction.
- **Configurable**: Sensitivity, angle limits, inversion, and smoothing are all adjustable in the inspector.
- **Pickup**: Spotlight can be picked up and moved like any other interactable.

---

## Custom Interactables & Events

- **Custom Conditions**: Override `CanInteract(MonoBehaviour player)` in your interactable to add custom logic (e.g., require player to be stationary, require light, etc.).
- **Events-Based System**: All interactables support events for interaction start/end, pickup, and more. Use the `InteractableEvents` component to hook up UnityEvents in the inspector.
- **Feedback Components**: Add visual, audio, or physical feedback by attaching modular components (e.g., `VisualInteractionFeedback`).

### Example: Custom Condition
```csharp
public override bool CanInteract(MonoBehaviour player)
{
    if (!base.CanInteract(player)) return false;
    // Custom logic here
    return true;
}
```

---

## Checkpoint System

- **CheckpointManager**: Handles respawn and checkpoint logic for all objects with the `Player` tag.
- **How it works**:
  - All objects tagged `Player` are moved to the checkpoint position on respawn.
  - Rigidbody state is reset for each player object.
  - No need to assign a single player transform; works for multiplayer or AI as well.

---

## Troubleshooting

- **Input not working?** Ensure `InputManager` and `InputContextProvider` are present in the scene (make sure the bootstrap scripts are present on atleast one component).
- **Pickup issues?** Check Rigidbody/collider setup and pickup settings in the inspector.
- **Spotlight not falling after drop?** Make sure `SpotlightController.EndPickup()` enables gravity and disables kinematic.
- **Multiple players not respawning?** Confirm all player objects are tagged `Player`.

---

## Development & Extension

- **Creating New Interactables**: Inherit from `InteractableBase`, override interaction methods, and optionally add custom conditions or feedback components.
- **Extending Camera Support**: Add new Cinemachine cameras and assign them to interactables as needed.
- **Adding Input Types**: Extend `InputManager` and update `InputContextProvider` for new actions.
