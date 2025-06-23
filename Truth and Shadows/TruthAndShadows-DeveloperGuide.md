# Truth and Shadows - Comprehensive Developer Guide

## Table of Contents

1. [Overview](#overview)
2. [Input System](#input-system)
   - [Keyboard & Mouse Controls](#keyboard--mouse-controls)
   - [Controller Support](#controller-support)
   - [InputManager Setup](#inputmanager-setup)
3. [Camera System](#camera-system)
   - [Controller Camera Setup](#controller-camera-setup)
   - [Required Scripts](#required-scripts)
4. [Spotlight System](#spotlight-system)
   - [Quick Setup](#quick-setup)
   - [System Architecture](#system-architecture)
   - [Configuration Options](#configuration-options)
   - [Known Issues & Fixes](#known-issues--fixes)
5. [Pickup System](#pickup-system)
   - [InteractableBase Class](#interactablebase-class)
   - [Spotlight Pickup](#spotlight-pickup)
   - [Giant Cube Example](#giant-cube-example)
6. [Dog Hint System](#dog-hint-system)
   - [Setup Instructions](#dog-setup-instructions)
   - [Configuration Options](#dog-configuration-options)
   - [Example Usage](#dog-example-usage)
7. [Troubleshooting](#troubleshooting)
   - [Input Issues](#input-issues)
   - [Camera Issues](#camera-issues)
   - [Spotlight Issues](#spotlight-issues)
   - [Pickup Issues](#pickup-issues)
8. [Development & Extension](#development--extension)
   - [Creating New Interactables](#creating-new-interactables)
   - [Modifying Input System](#modifying-input-system)
   - [Extending Camera Support](#extending-camera-support)

---

## Overview

Truth and Shadows is a Unity game with several custom systems for interaction, camera management, and input handling. This guide provides comprehensive documentation for developers working with these systems.

The core systems include:
- Input management for keyboard/mouse and controllers
- Camera system with Cinemachine integration
- Spotlight interaction mechanic
- Object pickup and manipulation system

---

## Input System

The input system uses an abstraction layer that supports both keyboard/mouse and controller input methods, allowing for a consistent interface regardless of input device.

### Keyboard & Mouse Controls

| Action | Key |
|--------|-----|
| Movement | WASD |
| Look | Mouse |
| Interact/Aim | R |
| Pickup/Drop | F |
| Sprint | E |
| Reset | L |
| Dog Hint | K |

### Controller Support

The game supports Xbox, PlayStation, and Nintendo Switch Pro controllers with the following mappings:

| Action | Xbox | PlayStation | Switch Pro |
|--------|------|------------|------------|
| Movement | Left Stick | Left Stick | Left Stick |
| Look | Right Stick | Right Stick | Right Stick |
| Interact/Aim | LB | L1 | L |
| Pickup/Drop | RB | R1 | R |
| Reset | Back/View | Share | - |
| Dog Hint | B | Circle | A |

### InputManager Setup

The input system requires proper initialization to handle all input types:

1. **Add InputManagerBootstrap**: Ensure you have a GameObject with the `InputManagerBootstrap` component in your first/loading scene.

2. **Required Scripts**:
   - `InputManager.cs` - Core input handling
   - `InputManagerBootstrap.cs` - Creates the InputManager instance

3. **Custom Input Axes**:

   The following custom axes must be added to Unity's Input Manager:

   #### RightStickHorizontal
   - Type: JoystickAxis
   - Axis: 3rd axis (Joysticks)
   - Joy Num: Get Motion from all Joysticks

   #### RightStickVertical
   - Type: JoystickAxis  
   - Axis: 4th axis (Joysticks)
   - Joy Num: Get Motion from all Joysticks
   - Invert: Checked (to match mouse y-inversion)

   These are automatically set up by the `InputAxisSetup.cs` script if used.

---

## Camera System

The game uses Cinemachine for camera management, with additional custom scripts to support controller input.

### Controller Camera Setup

To ensure proper controller camera support:

1. **Recommended Setup**:
   - Drag the `CameraControllerBootstrap` prefab into your first scene
   - All cameras will automatically work with both mouse and controller

2. **Alternative Setup**:
   - Go to **Tools > Truth and Shadows > Setup Controller Camera Support** in the Unity menu
   - This creates the necessary components automatically

### Required Scripts

- `CameraControllerConfigAlt.cs` - Camera controller configuration
- `CameraControllerBootstrap.cs` - Creates the camera controller
- `ControllerCameraHelper.cs` - Helper for controller camera support

### How It Works

The `ControllerCameraHelper` component:
- Configures all CinemachineFreeLook cameras in your scene
- Automatically switches between mouse and controller settings
- Updates camera sensitivity when input device changes

---

## Spotlight System

### Quick Setup

1. **Use the Spotlight Prefab**: Drag the pre-made spotlight prefab into your scene
2. **Position as needed**: Move and rotate to desired location
3. **Configure InteractionManager**: Ensure your scene has an InteractionManager with proper camera assignments
4. **Test**: Use R to control, F to pick up and move

### System Architecture

#### Components Hierarchy

```
AimableDevice (SpotlightController or custom InteractableBase-derived component)
├── VerticalRotatable (rotates up/down)
│   └── HorizontalRotatable (rotates left/right)
│       └── FunctionalComponent (Light, Camera, Weapon, etc.)
└── InteractionCamera (Cinemachine Virtual Camera)
```

#### Core Classes

- `IInteractable` interface with camera support and pickup functionality
- `InteractableBase` abstract class for common functionality and pickup mechanics
- `InteractionManager` with camera switching, 'R' key interaction, and 'F' key pickup
- `SpotlightController` with full mouse-controlled aiming

### Configuration Options

- **Mouse Sensitivity**: How responsive the spotlight is to mouse movement
- **Vertical Angles**: Min/max angles for up/down rotation (-90 to 90 degrees)
- **Horizontal Max Rotation**: Maximum degrees the spotlight can rotate left/right from center
- **Invert Input**: Option to invert mouse X or Y axis
- **Smooth Rotation**: Enable/disable smooth interpolation
- **Rotation Smoothness**: Speed of smooth rotation interpolation

### Known Issues & Fixes

#### 1. Pressing F Switching to R Button Camera

**Issue:** Pressing F (pickup button) incorrectly activating the spotlight camera mode.

**Fix:** 
- Added condition checking in `SpotlightController.StartPickup()`
- Reset camera state only when switching from interaction mode
- Added null safety checks for InputManager

#### 2. Spotlight Not Falling After Being Picked Up

**Issue:** Spotlight remaining floating in the air after dropping.

**Fix:**
- Modified `SpotlightController.EndPickup()` to enable gravity and disable kinematic state
- Updated `PickupIsKinematic` property to return `true` only when being held
- Added better error handling for when Rigidbody is missing

#### 3. Spotlight Not Respecting Pickup Raise Amount

**Issue:** Spotlight not respecting the `pickupRaiseAmount` value.

**Fix:**
- Updated `UpdatePickupPosition()` to incorporate `pickupRaiseAmount` value
- Implemented correct position calculation relative to player's position
- Added proper smoothing using the base class's `pickupSmoothness` value

---

## Pickup System

The pickup system allows players to pick up and carry objects around the game world.

### InteractableBase Class

The base class for all interactable objects includes:

- **Can Be Picked Up**: Enable/disable pickup for this object
- **Pickup Raise Amount**: How much the item raises when picked up (default: 0.2f)
- **Pickup Smoothness**: How smoothly the item follows the player (default: 10f)
- **Movement Style**: How the object moves when picked up (held, horizontal push/pull)

### Spotlight Pickup

Spotlight objects can be picked up and moved, with special handling:

1. Player approaches spotlight
2. **Press and hold 'F'** or controller RB to pick up
3. Spotlight follows player while F/RB is held
4. **Release 'F'** or RB to drop the spotlight, which falls to the ground

### Giant Cube Example

A demonstration of custom pickup behavior:

1. Player approaches the giant cube
2. **Press and hold 'F'** or RB to pick up the cube
   - Cube changes color to indicate pickup state
   - Cube follows player horizontally while staying on the ground
3. **Release 'F'** or RB to drop the cube
   - Cube returns to original color
   - Cube stays at current horizontal position on ground

#### Giant Cube Setup Instructions

Required components:
- Transform, MeshRenderer, MeshFilter
- BoxCollider (critical for interaction detection)
- GiantCubeInteractable script
- Rigidbody (optional, for physics)

---

## Dog Hint System

The Dog Hint System provides a way to guide players through the game by having a dog move to different locations to show points of interest. The dog can be used as a narrative device or to provide gameplay hints.

### Dog Setup Instructions

To add the dog hint system to your level:

1. **Add the Dog Prefab**:
   - Drag the `Dog_001` prefab into your level scene
   - Position it at the starting location

2. **Set Up Transformation Points**:
   - Create empty GameObjects to serve as waypoints for the dog
   - Position these transforms at the locations where you want the dog to move during hints
   - Organize transforms into logical groups based on different hints
   
3. **Configure CreatureMoverV2 Script**:
   - Select the dog GameObject
   - In the CreatureMoverV2 component, expand the "TransformationSets" attribute
   - Add each group of transforms as a separate transformation set
   - Give each transformation set a unique, descriptive name (e.g., "Hint1_ChestLocation", "Hint2_KeyLocation")

4. **Test the System**:
   - Press "K" on keyboard or the designated controller button (B on Xbox, Circle on PlayStation, A on Switch Pro) during gameplay to make the dog move through each hint in sequence
   - Each button press will trigger the next hint in order
   - After all hints are shown, pressing the hint button again will replay the last hint

### Dog Configuration Options

The CreatureMoverV2 script has several configurable parameters:

- **TransformationSets**: Array of named transform groups for different hints
- **MovementSpeed**: How fast the dog moves between points
- **RotationSpeed**: How quickly the dog turns when changing direction
- **WaitTime**: How long the dog waits at each waypoint
- **AnimationController**: Reference to the dog's animation controller (set automatically if using the prefab)

Example configuration:

```
CreatureMoverV2 (Script)
├── TransformationSets
│   ├── Element 0
│   │   ├── Name: "Hint1_FindLever"
│   │   ├── Transforms: [StartPoint, LeverLocation, ExitPoint]
│   ├── Element 1
│   │   ├── Name: "Hint2_OpenDoor"
│   │   ├── Transforms: [DoorEntrance, DoorSwitch, DoorExit]
├── MovementSpeed: 3.5
├── RotationSpeed: 5.0
├── WaitTime: 2.0
├── AnimationController: (Reference to animation controller)
```

### Dog Example Usage

The dog hint system can be used in various ways:

1. **Progressive Hints**:
   - Create a sequence of hints that guide the player through a puzzle
   - Each hint builds upon the previous one, gradually revealing the solution

2. **Location Guidance**:
   - Use the dog to show players where important items or paths are located
   - Particularly useful in larger or maze-like environments

3. **Environmental Storytelling**:
   - Have the dog sniff or react to important story elements
   - Create emotional connections through the dog's behavior

A complete example scene called "Doggo!" demonstrates the hint system. Study this scene to understand how all the components work together.

### Triggering Hints Programmatically

While pressing "K" on keyboard or the designated controller button (B on Xbox, Circle on PlayStation, A on Switch Pro) manually cycles through hints, you can also trigger specific hints programmatically:

```csharp
// Get the CreatureMoverV2 component
var dogController = FindObjectOfType<CreatureMoverV2>();

// Play a specific hint by name
dogController.PlayTransformation("Hint1_FindLever");

// Or play a hint by index
dogController.PlayTransformationByIndex(0); // Plays the first hint
```

This allows you to trigger hints based on game events, player actions, or other conditions.

---

## Troubleshooting

### Input Issues

If keyboard inputs (R and F keys) or controller inputs are not working:

1. **Check InputManager Initialization**:
   - Add the `InputSystemDebugger` component to any GameObject in your scene
   - Check the console for error messages about InputManager being null
   
2. **Add InputManagerBootstrap**:
   - Create an empty GameObject in your first scene
   - Add the `InputManagerBootstrap` component to it
   - Make sure "Don't Destroy On Load" is checked if you have multiple scenes

3. **Create InputManager Manually**:
   ```csharp
   // Add this to Start() method of any script in your scene
   if (TruthAndShadows.InputSystem.InputManager.Instance == null)
   {
       var inputManagerObj = new GameObject("InputManager");
       inputManagerObj.AddComponent<TruthAndShadows.InputSystem.InputManager>();
       DontDestroyOnLoad(inputManagerObj);
   }
   ```

4. **Check Controller Connections**:
   - Disconnect and reconnect controllers
   - Ensure you've set up the custom axes in Input Manager

### Camera Issues

1. **Controller Camera Not Working**:
   - Check if `RightStickHorizontal` and `RightStickVertical` axes exist
   - Verify your controller is detected by Unity
   - Try different axis numbers for different controllers

2. **Camera Switching Issues**:
   - Make sure you have a CameraControllerBootstrap in your scene
   - Ensure no other script is overriding camera priorities
   - Check if InteractionManager has proper camera references

3. **Runtime Fix For Controller Cameras**:
   - While the game is running, select the ControllerCameraHelper GameObject
   - Click "Setup Now" in the Inspector

### Spotlight Issues

1. **Spotlight doesn't rotate**: Check VerticalRotatable and HorizontalRotatable assignments
2. **Camera doesn't switch**: Verify InteractionCamera and InteractionManager setup
3. **Rotation limits not working**: Check angle values are within valid ranges
4. **Mouse sensitivity too high/low**: Adjust sensitivity values
5. **Wild rotation at extreme angles**: Ensure using updated Quaternion-based SpotlightController

### Pickup Issues

1. **Object won't pick up**: Check CanBePickedUp is enabled and has proper colliders
2. **Item doesn't follow player**: Check pickupSmoothness value
3. **Item falls through ground**: Verify colliders are properly re-enabled after drop
4. **Can't pick up after dropping**: Check physics components are properly restored
5. **Spotlight camera stays active after pickup**: Verify StartPickup properly resets camera

---

## Development & Extension

### Creating New Interactables

To create a new interactable similar to the spotlight:

1. Inherit from `InteractableBase`
2. Implement the required methods (StartInteraction, EndInteraction, etc.)
3. Configure pickup behavior if needed
4. Set up proper camera references if your interactable uses a camera

### Modifying Input System

The input system can be extended or modified by:

1. Editing the `InputManager.cs` class to add new input methods
2. Updating the button mappings in InputManager
3. Implementing new GetButton/GetButtonDown methods for new actions

### Extending Camera Support

To extend the camera system:

1. Modify the `CameraControllerConfigAlt.cs` to adjust camera behavior
2. Update the `ControllerCameraHelper` component for new controller types
3. Adjust sensitivity settings as needed

Remember to test with different controllers to ensure broad compatibility.
