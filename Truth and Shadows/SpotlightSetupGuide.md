# Spotlight Interaction System - Developer Guide

## Overview
The spotlight interaction system provides a robust framework for creating interactive objects with mouse-controlled aiming and pickup mechanics. While a ready-to-use spotlight prefab is available, this guide explains the underlying system architecture for developers who want to create similar interactive devices or understand the codebase.

## Ready-to-Use Assets
- **Spotlight Prefab**: Pre-configured spotlight with all components and settings
- **Just drag and drop** the spotlight prefab into your scene for immediate use

## System Architecture (For Developers)

This section explains how the interaction system works internally, useful for:
- Creating new types of aimable devices (turrets, cameras, etc.)
- Understanding the codebase structure
- Modifying or extending the existing spotlight behavior
- Building similar interaction mechanics

## Components Completed

### Core Interaction System
- `IInteractable` interface with camera support and pickup functionality
- `InteractableBase` abstract class for common functionality and pickup mechanics
- `InteractionManager` with camera switching, 'R' key interaction, and 'F' key pickup
- `GateSwitch` example implementation

### Spotlight Controller
- `SpotlightController` with full mouse-controlled aiming
- Quaternion-based rotation system preventing gimbal lock and wild rotation
- Configurable rotation limits (vertical and horizontal)
- Smooth rotation interpolation
- Pivot point support for realistic rotation
- Cursor management during interaction
- Full pickup support (spotlights can be picked up and moved)

### Pickup System
- Hold-to-carry mechanics with natural physics
- Items raise slightly when picked up (configurable)
- Maintains relative position to player while carried
- Natural drop behavior (items fall straight down)
- Configurable per-object pickup settings
- Physics handling (disables rigidbody/colliders during pickup)

## Setup Instructions

### Quick Setup (Recommended)
1. **Use the Spotlight Prefab**: Drag the pre-made spotlight prefab into your scene
2. **Position as needed**: Move and rotate to desired location
3. **Configure InteractionManager**: Ensure your scene has an InteractionManager with proper camera assignments
4. **Test**: Use R to control, F to pick up and move

### Custom Development (For New Interactive Devices)
If you want to create your own aimable devices similar to the spotlight:

#### 1. Basic Hierarchy Structure
1. Create a GameObject hierarchy:
   ```
   AimableDevice (SpotlightController or custom InteractableBase-derived component)
   ├── VerticalRotatable (rotates up/down)
   │   └── HorizontalRotatable (rotates left/right)
   │       └── FunctionalComponent (Light, Camera, Weapon, etc.)
   └── InteractionCamera (Cinemachine Virtual Camera)
   ```

2. Configure the component:
   - Assign the functional component (Light, Camera, etc.) to appropriate field
   - Assign `VerticalRotatable` and `HorizontalRotatable` GameObjects
   - Set rotation limits (`verticalMinAngle`, `verticalMaxAngle`)
   - Assign the interaction camera (optional)

#### 2. Camera Setup (Optional)
1. Create a Cinemachine Virtual Camera for interaction (if desired)
2. Position it to provide a good view of the device's operation area
3. Assign it to the `InteractionCamera` field in your component

#### 3. System Integration
1. Ensure your scene has an InteractionManager (usually on player or game manager)
2. Assign the default camera that should be active when not interacting
3. Configure interaction range and radius

#### 4. Pickup Configuration
Configure pickup behavior in InteractableBase-derived objects:
- **Can Be Picked Up**: Enable/disable pickup for this object
- **Pickup Raise Amount**: How much the item raises when picked up (default: 0.2f)
- **Pickup Smoothness**: How smoothly the item follows the player (default: 10f)

#### 5. Optional: Pivot Points
- Assign `verticalPivotPoint` and `horizontalPivotPoint` for more realistic rotation around mounting points
- If not assigned, the spotlight will rotate around its local origin

## Configuration Options (For Custom Development)

### Component Settings
- **Mouse Sensitivity**: How responsive the spotlight is to mouse movement
- **Vertical Angles**: Min/max angles for up/down rotation (-90 to 90 degrees)
- **Horizontal Max Rotation**: Maximum degrees the spotlight can rotate left/right from center
- **Invert Input**: Option to invert mouse X or Y axis
- **Smooth Rotation**: Enable/disable smooth interpolation
- **Rotation Smoothness**: Speed of smooth rotation interpolation

### Pickup System Settings
- **Can Be Picked Up**: Whether the object can be picked up and moved
- **Pickup Raise Amount**: Distance (in units) the item raises when first picked up
- **Pickup Smoothness**: How quickly the item follows player movement (higher = more responsive)

### Key Features
- **Camera Switching**: Automatically switches to spotlight camera during interaction
- **Rotation Constraints**: Prevents spotlight from rotating beyond realistic limits
- **Cursor Management**: Locks cursor during interaction for precise control
- **Error Handling**: Validates components and provides helpful warnings
- **Gimbal Lock Prevention**: Quaternion-based rotation prevents wild spinning at extreme angles
- **Hold-to-Carry**: Natural pickup mechanics requiring F key to be held
- **Physics Integration**: Proper physics handling during pickup and drop
- **Camera Preservation**: Pickup maintains player camera view (doesn't switch to object's camera)

## Controls

### Spotlight Interaction
1. Player approaches spotlight
2. **Press 'R'** to start controlling the spotlight
3. **Move mouse** to aim the spotlight
4. **Release 'R'** to stop controlling

### Pickup System
1. Player approaches any pickupable object
2. **Press and hold 'F'** to pick up the item
   - Item raises slightly in place when picked up
   - Item follows player while F is held
   - Camera stays on player (doesn't switch to object's camera)
3. **Release 'F'** to drop the item
   - Item falls straight down from current position
   - Physics and collisions re-enabled

### Code Extension
To create a new aimable device:
1. Inherit from `InteractableBase`
2. Implement the rotation logic (use SpotlightController as reference)
3. Replace the spotlight-specific code with your device logic
4. Configure the hierarchy as shown above

## Testing Scene
Use the `Interactables.unity` scene in `Assets/Scenes/GameGym/` for testing the interaction system.

## Troubleshooting

### Common Issues
1. **Spotlight doesn't rotate**: Check that VerticalRotatable and HorizontalRotatable GameObjects are assigned
2. **Camera doesn't switch**: Ensure InteractionCamera is assigned and InteractionManager has a default camera
3. **Rotation limits not working**: Verify angle values are within valid ranges
4. **Mouse sensitivity too high/low**: Adjust mouseSensitivity value
5. **Wild rotation at extreme angles**: Ensure using the updated Quaternion-based SpotlightController
6. **Items won't pick up**: Check that CanBePickedUp is enabled and object has appropriate colliders
7. **F key not working**: Verify InteractionManager is in scene and configured properly
8. **Camera switching during pickup**: Check that pickup doesn't override camera management

### Pickup-Specific Issues
- **Item doesn't follow player**: Check pickupSmoothness value and ensure Update() is being called
- **Item falls through ground**: Verify colliders are properly re-enabled after drop
- **Can't pick up after dropping**: Check that physics components are properly restored
- **Item position jumps**: Ensure relativePosition calculation is working correctly

### Debug Features
- Console logging when starting/stopping spotlight control
- Console logging for pickup and drop events
- Component validation in Start() with helpful error messages
- OnValidate() method ensures angle limits are valid
- Interaction range visualization (configurable debug rays)
- Physics state logging during pickup/drop operations

### Technical Implementation Notes
- **Quaternion Rotation**: Prevents gimbal lock issues that caused wild spinning in earlier versions
- **Relative Positioning**: Items maintain spatial relationship to player during pickup
- **Physics Management**: Automatic handling of rigidbody and collider states
- **Camera Preservation**: Pickup system respects player camera control
- **Hold-to-Carry**: More intuitive than toggle-based pickup systems
