# Custom Interaction Conditions Guide

This guide explains how to use the new custom interaction conditions system to create interactable objects that respond to specific player states and contexts.

## Overview

The interaction system has been extended to allow interactable objects to define their own activation conditions. This means that objects can now control whether they can be interacted with based on:
- Player state (moving, stationary, aiming, etc.)
- Environment conditions (is it dark, is it raining, etc.)
- Game progression (has the player unlocked this ability, etc.)
- Any other custom condition you can code!

This allows for more flexible and context-sensitive interactions without hardcoding player states into each object.

## How It Works

1. The `IInteractable` interface now includes a `CanInteract(MonoBehaviour player)` method.
2. When the player attempts to interact with an object, the system will first call `CanInteract()` on that object.
3. The interaction will only proceed if `CanInteract()` returns true.

## Basic Usage

To create an interactable object with custom conditions:

1. Inherit from `InteractableBase` (which already implements `IInteractable`).
2. Override the `CanInteract(MonoBehaviour player)` method.
3. Implement your custom condition logic in the override.

```csharp
public override bool CanInteract(MonoBehaviour player)
{
    // First check the base implementation
    if (!base.CanInteract(player))
        return false;
        
    // Your custom conditions here
    bool myConditionMet = CheckMyCondition();
    
    return myConditionMet;
}
```

## Example Implementations

### 1. StationaryInteractable

Requires the player to be standing still to interact. Great for:
- Reading notes/books
- Delicate mechanisms
- Precision tasks

### 2. LightRequiredInteractable

Requires the player's spotlight to be active. Perfect for:
- Objects that are only visible when illuminated
- Light-activated mechanisms
- Shadow puzzles

## Best Practices

1. **Always call the base implementation first**:
   ```csharp
   if (!base.CanInteract(player))
       return false;
   ```

2. **Provide feedback**: When interaction is denied due to a condition, let the player know why:
   ```csharp
   if (!conditionMet)
   {
       ShowPlayerMessage("You need to [do something] to interact with this");
       return false;
   }
   ```

3. **Keep conditions intuitive**: Players should be able to figure out what they need to do.

4. **Use defensive coding**: When accessing player properties via reflection, always use try/catch blocks to prevent crashes.

## Advanced Example: Multiple Conditions

You can combine multiple conditions in one interactable:

```csharp
public class ComplexInteractable : InteractableBase
{
    public override bool CanInteract(MonoBehaviour player)
    {
        if (!base.CanInteract(player))
            return false;
            
        bool isPlayerStationary = CheckIfPlayerIsStationary(player);
        bool isSpotlightActive = CheckIfSpotlightIsActive(player);
        
        // Require BOTH conditions
        return isPlayerStationary && isSpotlightActive;
    }
}
```

## Debugging Tips

If an interaction isn't working as expected:

1. Add debug logging in your `CanInteract` method:
   ```csharp
   Debug.Log($"CanInteract called: condition = {conditionValue}");
   ```

2. Check the console for error messages from reflection calls.

3. Make sure your interactable's colliders are properly set up for the interaction system.
