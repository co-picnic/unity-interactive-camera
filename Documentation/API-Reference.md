# API Reference

This document provides comprehensive API documentation for the InteractiveCameraSystem package.

## Core Classes

### CinemachineCameraManager

The main singleton that manages camera modes and transitions.

#### Properties

- `static CinemachineCameraManager Instance` - Singleton instance
- `CameraMode[] AvailableModes` - All available camera modes
- `CameraMode CurrentMode` - Currently active camera mode
- `bool IsTransitioning` - Whether a transition is in progress

#### Methods

- `void SwitchToMode(CameraMode mode)` - Switch to specified camera mode
- `void SwitchToMode(string modeName)` - Switch to mode by name
- `CameraMode GetCurrentMode()` - Get current active mode
- `void RefreshModeList()` - Refresh available modes from assets
- `void InstantiateAllCameras()` - Create all camera prefabs in scene

#### Events

- `event System.Action<CameraMode> OnModeChanged` - Fired when camera mode changes
- `event System.Action OnTransitionStarted` - Fired when transition begins
- `event System.Action OnTransitionCompleted` - Fired when transition ends

### CameraMode

ScriptableObject that defines camera configuration and behavior.

#### Properties

- `string modeName` - Unique name for the camera mode
- `GameObject virtualCamera` - Cinemachine camera prefab
- `CameraSettings cameraSettings` - Camera projection and lens settings
- `DragSettings dragSettings` - Drag input configuration
- `ZoomSettings zoomSettings` - Zoom input configuration
- `FollowSettings followSettings` - Target following configuration
- `TransitionSettings transitionSettings` - Transition timing and easing

### VirtualCameraAnchor

Component that defines camera targets and anchor points.

#### Properties

- `Transform targetTransform` - Primary target transform
- `Vector3 offset` - Offset from target position
- `bool IsValid` - Whether the anchor is properly configured
- `Vector3 WorldPosition` - Calculated world position

#### Methods

- `void SetTarget(Transform target)` - Set the target transform
- `void UpdatePosition()` - Update anchor position
- `bool IsInBounds(Vector3 position)` - Check if position is within bounds

## Input Components

### CinemachineDragInputComponent

Handles touch drag input for camera movement.

#### Properties

- `DragSettings dragSettings` - Drag configuration
- `VirtualCameraBounds virtualCameraBounds` - Movement boundaries
- `bool IsDragging` - Whether currently dragging

#### Methods

- `void ProcessDrag(Vector2 delta)` - Process drag input
- `void SetDragEnabled(bool enabled)` - Enable/disable drag input

### CinemachineZoomInputComponent

Handles pinch-zoom input for camera zoom.

#### Properties

- `ZoomSettings zoomSettings` - Zoom configuration
- `float CurrentFOV` - Current field of view
- `bool IsZooming` - Whether currently zooming

#### Methods

- `void ProcessZoom(float zoomDelta)` - Process zoom input
- `void SetZoomEnabled(bool enabled)` - Enable/disable zoom input

### FingerManager

Manages touch input and gesture detection using Lean Touch.

#### Properties

- `int FingerCount` - Number of active fingers
- `bool IsPinching` - Whether pinch gesture is active
- `bool IsDragging` - Whether drag gesture is active

#### Events

- `event System.Action OnPinchStart` - Pinch gesture started
- `event System.Action<float> OnPinchUpdate` - Pinch gesture updated
- `event System.Action OnPinchEnd` - Pinch gesture ended
- `event System.Action<Vector2> OnDragStart` - Drag gesture started
- `event System.Action<Vector2> OnDragUpdate` - Drag gesture updated
- `event System.Action OnDragEnd` - Drag gesture ended

## Settings Classes

### CameraSettings

Defines camera projection and lens settings.

#### Properties

- `CameraProjectionMode projectionMode` - Orthographic or Perspective
- `float fieldOfView` - Field of view for perspective mode
- `float orthographicSize` - Size for orthographic mode
- `float nearClipPlane` - Near clipping plane
- `float farClipPlane` - Far clipping plane

### DragSettings

Defines drag input behavior.

#### Properties

- `float sensitivity` - Drag sensitivity multiplier
- `bool invertX` - Invert horizontal drag
- `bool invertY` - Invert vertical drag
- `DragMode dragMode` - World or screen space dragging

### ZoomSettings

Defines zoom input behavior.

#### Properties

- `float minFOV` - Minimum field of view
- `float maxFOV` - Maximum field of view
- `float sensitivity` - Zoom sensitivity multiplier
- `bool invertZoom` - Invert zoom direction

### FollowSettings

Defines target following behavior.

#### Properties

- `bool enableFollow` - Enable target following
- `float followSpeed` - Follow speed multiplier
- `Vector3 followOffset` - Offset from target
- `bool smoothFollow` - Enable smooth following

### TransitionSettings

Defines camera transition behavior.

#### Properties

- `float transitionDuration` - Transition duration in seconds
- `AnimationCurve transitionCurve` - Transition easing curve
- `bool waitForTransition` - Wait for transition to complete
- `TransitionType transitionType` - Type of transition

## Enums

### CameraProjectionMode
- `Perspective` - Perspective projection
- `Orthographic` - Orthographic projection

### DragMode
- `WorldSpace` - Drag in world space
- `ScreenSpace` - Drag in screen space

### TransitionType
- `Instant` - Instant transition
- `Smooth` - Smooth transition
- `Blend` - Blend transition

## Editor Tools

### CinemachineCameraManagerEditor

Custom editor for CinemachineCameraManager with additional tools.

#### Available Tools
- Auto-Discover Modes - Automatically find camera modes
- Instantiate All Cameras - Create all camera prefabs
- Context Menu: Create Camera Mode - Create mode from Cinemachine camera

### CameraModeEditor

Custom editor for CameraMode ScriptableObjects.

#### Features
- Visual mode configuration
- Preview camera settings
- Validation warnings
- Quick setup tools

## Integration Classes

### CinemachineInputManager

Coordinates input events between components.

#### Methods
- `void RegisterInputComponent(ICinemachineCameraInputComponent component)` - Register input component
- `void UnregisterInputComponent(ICinemachineCameraInputComponent component)` - Unregister input component
- `void ProcessInput()` - Process all registered input

### ICinemachineCameraInputComponent

Interface for camera input components.

#### Methods
- `void Initialize()` - Initialize the input component
- `void ProcessInput()` - Process input for current frame
- `void SetEnabled(bool enabled)` - Enable/disable input processing
