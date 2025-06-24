# Input System Architecture

## Overview

This project uses a centralized InputManager that processes all input and provides a clean, consistent API for other systems to consume. This ensures that input handling is consistent across the game and prevents input conflicts.

## Key Components

### InputManager

The `InputManager` class in `Assets/Scripts/InputSystem/InputManager.cs` is the single source of truth for input in the game. It:

1. Processes raw input from keyboard, mouse, and various controllers
2. Exposes input state through easy-to-use properties
3. Handles mutual exclusivity between actions (e.g., can't interact while picking up)
4. Fixes issues with input blocking during special actions

### PlayerController

The new `PlayerController` class in `Assets/Scripts/Player/PlayerController.cs` consumes input from the InputManager and translates it into player behaviors based on gameplay state:

1. Manages player state (normal, aiming, pickup, etc.)
2. Converts input into context-appropriate movement and actions
3. Separates input handling from player behavior logic
4. Prevents input conflicts across systems

## How Input Flows Through the System

```
Raw Input (Unity) → InputManager → PlayerController → Character Movement/Actions
```

1. Unity's Input API processes raw hardware input
2. InputManager centralizes all input processing
3. PlayerController consumes processed input based on player state
4. Character movement and actions respond appropriately

## Usage Guidelines

### DO:
- Always access input through InputManager properties
- Consider player state when responding to input
- Process all inputs simultaneously
- Keep PlayerController as the single decision maker for player actions

### DON'T:
- Access Unity's Input API directly
- Process raw input in character movement scripts
- Make state decisions based on input in multiple places

## Input Properties Available

The InputManager provides these properties for accessing input state:

- **Movement**: `MoveInput`, `MoveInputRaw`, `IsRunning`
- **Camera**: `LookInput`, `PickupCameraInput`
- **Interaction**: `InteractPressed`, `InteractHeld`, `InteractReleased`
- **Pickup**: `PickupPressed`, `PickupHeld`, `PickupReleased`
- **Other**: `RotateHeld`, `ResetPressed`, `HintPressed`, `HintHeld`, `HintReleased`
- **Device**: `UsingController`

## Implementation Notes

1. For backward compatibility, the InputManager still provides the older method-based API like `GetMovementInput()`.
2. Special handling exists for the "F key issue" to ensure movement and looking works simultaneously.
3. The InputManager handles controller detection and mapping for different controller types.
