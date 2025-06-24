# InputManager Usage Guide

## Overview

The InputManager is a centralized component that handles all game input from keyboard, mouse, and various controllers (Xbox, PlayStation, and Switch). It serves as the single source of truth for input across the entire application.

## Core Features

- **Single Point of Access**: All input queries should go through this manager instead of using Unity's `Input` class directly.
- **Simultaneous Input Processing**: All inputs are processed at once to ensure nothing is missed.
- **Input State Properties**: Easy-to-use properties expose current input state.
- **Controller Support**: Input works consistently across keyboard/mouse and various controllers.
- **Mutual Exclusivity**: Handles cases where certain inputs should block others (e.g., pickup and interact buttons).

## How to Use

### Basic Setup

The InputManager is designed as a singleton that should be present in your scene:

```csharp
// Check if input is available
if (InputManager.Instance != null)
{
    // Use input methods
}
```

### Getting Input State

Instead of directly accessing Unity's Input system, use the properties exposed by the InputManager:

```csharp
// Movement example
Vector2 moveDirection = InputManager.Instance.MoveInput;

// Button press example
if (InputManager.Instance.InteractPressed)
{
    // Handle interaction
}

// Button hold example
if (InputManager.Instance.PickupHeld)
{
    // Handle object pickup
}

// Button release example
if (InputManager.Instance.HintReleased)
{
    // Handle hint button release
}
```

### Available Input Properties

#### Movement
- `MoveInput`: Smoothed movement vector (for character movement)
- `MoveInputRaw`: Raw movement vector (for UI navigation)
- `IsRunning`: Whether sprint is active

#### Camera
- `LookInput`: Camera/look rotation input
- `PickupCameraInput`: Special camera input during object pickup

#### Interaction Buttons
- `InteractPressed`, `InteractHeld`, `InteractReleased`: Interact button states
- `PickupPressed`, `PickupHeld`, `PickupReleased`: Pickup button states
- `RotateHeld`: Whether rotate/aim button is held
- `ResetPressed`: Whether reset button was pressed
- `HintPressed`, `HintHeld`, `HintReleased`: Hint button states

#### Device Detection
- `UsingController`: Whether player is using controller or keyboard/mouse

### Legacy Method Support

For backward compatibility, the InputManager still provides the older method-based API:

```csharp
// These methods still work, but the property versions are preferred
Vector2 movement = InputManager.Instance.GetMovementInput();
bool isInteracting = InputManager.Instance.GetInteractButton();
```

## Best Practices

1. **Always Use InputManager**: Never use Unity's `Input` class directly in your scripts.
2. **Use Properties**: Prefer the property-based API (`InteractPressed`) over legacy methods (`GetInteractButtonDown()`).
3. **Check Once**: Input should be checked once per frame, so query values and store them locally if needed.
4. **Extend Responsibly**: If you need to add new input types, add them to the InputManager rather than creating separate input handling.

## Important Notes

- The InputManager handles mutual exclusivity between certain actions (e.g., can't interact while picking up)
- Special handling exists for the F key issue with simultaneous movement and looking
- Input values are updated at the beginning of each frame in `Update()`
