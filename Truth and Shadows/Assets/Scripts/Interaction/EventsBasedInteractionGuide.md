# Events-Based Interaction System Guide

## Overview

The Truth and Shadows interaction system has been updated to use an events-based architecture that allows for more modular and flexible interactions. This guide will help you understand and use the new features.

## Core Components

### 1. IInteractable Interface

The `IInteractable` interface has been enhanced with event support:

```csharp
public interface IInteractable
{
    void StartInteraction();
    void ContinueInteraction();
    void EndInteraction();
    bool RequiresContinuousInteraction { get; }
    
    // New events for interaction state changes
    event Action<GameObject> OnInteractionStarted;
    event Action<GameObject> OnInteractionEnded;
    
    // Custom conditions for interaction
    bool CanInteract(MonoBehaviour player);
    
    // Camera and pickup properties
    Component InteractionCamera { get; }
    bool CanBePickedUp { get; }
    void StartPickup(Transform playerTransform);
    void EndPickup();
    bool IsPickedUp { get; }
}
```

### 2. InteractableEvents Component

The new `InteractableEvents` component exposes Unity inspector events for interactions, allowing designers to hook up custom responses without coding:

```csharp
public class InteractableEvents : MonoBehaviour
{
    public InteractionEvent onInteractionStarted;
    public InteractionEvent onInteractionContinued;
    public InteractionEvent onInteractionEnded;
    public InteractionEvent onInteractionFailed;
    public InteractionEvent onFocused;
    public InteractionEvent onUnfocused;
    public InteractionEvent onPickupStarted;
    public InteractionEvent onPickupEnded;
}
```

## Feedback Components

Three modular feedback components have been created that can be attached to any interactable to provide visual, audio, and physical feedback:

### 1. VisualInteractionFeedback

Provides visual feedback through material emission:
- Color changes for different interaction states
- Pulsing effect during interaction
- Flash effect for failed interactions

### 2. AudioInteractionFeedback

Provides audio feedback for interactions:
- Hover sounds
- Interaction start/end sounds
- Pickup/drop sounds
- Failed interaction sounds

### 3. PhysicalInteractionFeedback

Provides physical movement feedback:
- Hover bob effect (object gently floats up and down)
- Shake effect during interaction
- Bounce effect for failed interactions

## Using the System

### Creating a Basic Interactable

1. Create a GameObject with appropriate components (e.g., MeshRenderer, Collider)
2. Attach a script that inherits from `InteractableBase`
3. Implement the abstract methods to define interaction behavior

```csharp
public class MyInteractable : InteractableBase
{
    public override void StartInteraction()
    {
        Debug.Log("Interaction started");
        // Your interaction logic here
        
        // Don't forget to raise the event
        RaiseInteractionStartedEvent(GameObject.FindGameObjectWithTag("Player"));
    }
}
```

### Adding Feedback Components

1. Add your implementation of `IInteractable` to the object
2. Add the `InteractableEvents` component
3. Add any of the feedback components:
   - `VisualInteractionFeedback`
   - `AudioInteractionFeedback`
   - `PhysicalInteractionFeedback`
4. Configure the feedback settings in the inspector

### Creating Custom Conditions

To create an interactable with custom activation conditions:

```csharp
public class SpecialInteractable : InteractableBase
{
    public override bool CanInteract(MonoBehaviour player)
    {
        // Check base conditions first
        if (!base.CanInteract(player))
            return false;
            
        // Add your custom conditions here
        bool hasRequiredItem = CheckForRequiredItem();
        
        return hasRequiredItem;
    }
}
```

### Creating Custom Feedback Components

You can create your own custom feedback components by:

1. Creating a class that inherits from `MonoBehaviour`
2. Adding the `[RequireComponent(typeof(InteractableEvents))]` attribute
3. Subscribing to the events in `OnEnable()` and unsubscribing in `OnDisable()`
4. Implementing your custom feedback logic in the event handlers

```csharp
[RequireComponent(typeof(InteractableEvents))]
public class MyCustomFeedback : MonoBehaviour
{
    private InteractableEvents events;
    
    private void OnEnable()
    {
        events = GetComponent<InteractableEvents>();
        events.onInteractionStarted.AddListener(OnInteractionStarted);
    }
    
    private void OnDisable()
    {
        events.onInteractionStarted.RemoveListener(OnInteractionStarted);
    }
    
    private void OnInteractionStarted(InteractionEventData data)
    {
        // Your custom feedback logic here
    }
}
```

## Best Practices

1. **Always raise events in your interactables**:
   Use `RaiseInteractionStartedEvent` and `RaiseInteractionEndedEvent` in your implementations.

2. **Separate concerns**:
   - Interactable components: Define **what** the object does when interacted with
   - Feedback components: Define **how** the object responds visually/aurally/physically
   - Condition components: Define **when** interaction is allowed

3. **Test interactions in multiple states**:
   Test that your interactables behave correctly when the player is in different states (normal, aiming, etc.)

4. **Use composition over inheritance**:
   Instead of creating complex inheritance hierarchies, compose behavior using multiple components.

## Examples

### Door Interactable

```csharp
public class DoorInteractable : InteractableBase
{
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private bool isLocked = false;
    
    public override bool CanInteract(MonoBehaviour player)
    {
        return base.CanInteract(player) && !isLocked;
    }
    
    public override void StartInteraction()
    {
        doorAnimator.SetTrigger("Toggle");
        RaiseInteractionStartedEvent(GameObject.FindGameObjectWithTag("Player"));
    }
}
```

### Terminal Interactable

```csharp
public class TerminalInteractable : InteractableBase
{
    [SerializeField] private float requiredPowerLevel = 0.5f;
    [SerializeField] private PowerSystem powerSystem;
    
    public override bool CanInteract(MonoBehaviour player)
    {
        return base.CanInteract(player) && powerSystem.CurrentPower >= requiredPowerLevel;
    }
    
    public override void StartInteraction()
    {
        // Open terminal UI
        UIManager.Instance.ShowTerminalUI(this);
        RaiseInteractionStartedEvent(GameObject.FindGameObjectWithTag("Player"));
    }
}
```
