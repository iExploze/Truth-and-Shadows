# Input Context Provider

## Overview

The `InputContextProvider` is a centralized service that manages input permissions across different gameplay states. It solves the problem of having input exclusivity logic scattered across multiple components (like PlayerController and InteractionManager) by providing a single source of truth about what inputs are currently allowed.

## Key Features

- **Centralized Permission System**: Provides a single location to check if a specific input type is allowed
- **State-Based Permissions**: Automatically updates permissions when player state changes
- **Manual Permission Override**: Allows forcing specific permissions regardless of state
- **Decoupled Architecture**: Avoids circular dependencies between components

## How to Use

### Setting Up the InputContextProvider

1. Add the `InputContextProvider` component to a persistent GameObject (like the GameManager)
2. The InputContextProvider is a singleton and will maintain itself across scenes

### Updating Player State

The PlayerController automatically updates the InputContextProvider when the player's state changes. This happens in the `OnStateChanged` method:

```csharp
if (InputContextProvider.Instance != null)
{
    // Convert our PlayerState to the shared PlayerState enum
    InputSystem.PlayerState sharedState = (InputSystem.PlayerState)newState;
    InputContextProvider.Instance.UpdatePlayerState(sharedState);
}
```

### Checking Permissions in Other Systems

Any system that needs to check if certain inputs are allowed should use the InputContextProvider:

```csharp
// In InteractionManager.cs
void Update()
{
    if (InputContextProvider.Instance == null)
        return;

    // Check if interaction is allowed before processing input
    if (InputManager.Instance.InteractPressed && InputContextProvider.Instance.CanInteract)
    {
        // Process interaction
    }

    // Check if pickup is allowed before processing input
    if (InputManager.Instance.PickupPressed && InputContextProvider.Instance.CanPickup)
    {
        // Process pickup
    }
}
```

### Forcing Custom Permissions

In special gameplay situations, you can override permissions regardless of player state:

```csharp
// Force disable movement during a special event
InputContextProvider.Instance.ForcePermission("move", false);

// Later, when the event is over
InputContextProvider.Instance.UpdatePlayerState(InputContextProvider.Instance.CurrentPlayerState);
```

## Architecture Decisions

The InputContextProvider uses a duplicate of the PlayerState enum to avoid circular dependencies. This is a common pattern for service layers that need to be accessed by multiple components.

The InputContextProvider doesn't replace the InputManager - it works alongside it. InputManager still handles raw input detection, while InputContextProvider determines if those inputs should have an effect based on current gameplay state.

## Best Practices

1. **Always check permissions before acting on input**: 
   ```csharp
   if (InputManager.Instance.InteractPressed && InputContextProvider.Instance.CanInteract)
   ```

2. **Use player state for common scenarios**, and only force individual permissions for special cases

3. **Restore normal behavior** by updating the player state rather than re-enabling each permission individually

4. **Avoid circular dependencies** by using the shared PlayerState enum rather than referencing PlayerController directly
