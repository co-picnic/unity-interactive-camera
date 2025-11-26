# Integration Guide

This guide provides step-by-step instructions for integrating the InteractiveCameraSystem package into your Unity project.

## Prerequisites

- Unity 2022.3 or later
- Cinemachine 2.9.0 or later
- Lean Touch (included in package)

## Installation Methods

### Method 1: Package Manager (Recommended)

1. **Add Package from Git URL**
   - Open Unity Package Manager (Window > Package Manager)
   - Click the "+" button and select "Add package from git URL"
   - Enter your package repository URL
   - Click "Add"

2. **Add Package from Local Folder**
   - Open Unity Package Manager
   - Click the "+" button and select "Add package from disk"
   - Navigate to the package folder and select `package.json`
   - Click "Open"

### Method 2: Manual Installation

1. **Copy Package Files**
   - Copy the entire package folder to your project's `Packages` folder
   - Or copy to `Assets/Plugins/` folder

2. **Import Dependencies**
   - Ensure Cinemachine is installed via Package Manager
   - Lean Touch is included in the package

## Quick Setup

### 1. Basic Setup

1. **Create Camera Manager**
   - Right-click in Hierarchy
   - Select "Create Empty" and name it "CameraManager"
   - Add `CinemachineCameraManager` component

2. **Add Camera Brain**
   - Add `CinemachineBrain` component to your main camera
   - Configure brain settings as needed

### 2. Create Camera Modes

1. **Create Mode Assets**
   - Right-click in Project window
   - Select "Create > InteractiveCameraSystem > CameraMode"
   - Name your mode (e.g., "ExplorationMode")

2. **Configure Mode Settings**
   - Set mode name
   - Assign virtual camera prefab
   - Configure camera, drag, zoom, and transition settings

### 3. Setup Input System

1. **Add Input Components**
   - Add `CinemachineDragInputComponent` to camera manager
   - Add `CinemachineZoomInputComponent` to camera manager
   - Add `FingerManager` to camera manager

2. **Configure Input Settings**
   - Adjust sensitivity and behavior in mode settings
   - Test with Lean Touch Simulator in Game view

### 4. Create Camera Prefabs

1. **Create Virtual Cameras**
   - Create Cinemachine Virtual Camera prefabs
   - Configure each camera for different modes
   - Assign to corresponding CameraMode assets

2. **Setup Camera Hierarchy**
   - Create a "Cameras" GameObject in scene
   - Use "Instantiate All Cameras" button in editor
   - Cameras will be automatically parented

## Advanced Configuration

### Custom Camera Modes

1. **Create Custom Mode**
   ```csharp
   [CreateAssetMenu(fileName = "CustomMode", menuName = "InteractiveCameraSystem/CameraMode")]
   public class CustomCameraMode : CameraMode
   {
       // Add custom properties
       public float customParameter;
       
       // Override methods if needed
       public override void ApplySettings(CinemachineVirtualCamera camera)
       {
           base.ApplySettings(camera);
           // Custom logic here
       }
   }
   ```

2. **Custom Input Components**
   ```csharp
   public class CustomInputComponent : MonoBehaviour, ICinemachineCameraInputComponent
   {
       public void Initialize()
       {
           // Initialize custom input
       }
       
       public void ProcessInput()
       {
           // Process custom input
       }
       
       public void SetEnabled(bool enabled)
       {
           this.enabled = enabled;
       }
   }
   ```

### Target Management

1. **Setup Anchors**
   - Add `VirtualCameraAnchor` to target objects
   - Configure offset and bounds
   - Anchors automatically register with camera manager

2. **Dynamic Target Switching**
   ```csharp
   // Switch camera target
   var anchor = FindObjectOfType<VirtualCameraAnchor>();
   anchor.SetTarget(newTarget);
   
   // Update camera to follow new target
   CinemachineCameraManager.Instance.RefreshModeList();
   ```

### Event Integration

1. **Subscribe to Events**
   ```csharp
   void Start()
   {
       CinemachineCameraManager.Instance.OnModeChanged += OnCameraModeChanged;
   }
   
   void OnCameraModeChanged(CameraMode newMode)
   {
       Debug.Log($"Switched to {newMode.modeName}");
   }
   ```

2. **Trigger Mode Changes**
   ```csharp
   // Switch to conversation mode
   var conversationMode = Resources.Load<CameraMode>("ConversationMode");
   CinemachineCameraManager.Instance.SwitchToMode(conversationMode);
   ```

## Troubleshooting

### Common Issues

1. **Camera Not Switching**
   - Check if CameraMode assets are properly assigned
   - Verify virtual camera prefabs exist
   - Ensure CinemachineBrain is configured

2. **Input Not Working**
   - Confirm FingerManager is in scene
   - Check if input components are enabled
   - Test in Game view, not Simulator view

3. **Prefabs Not Instantiating**
   - Verify camera prefabs are assigned to modes
   - Check for missing references
   - Use "Debug Input System" button in editor

### Performance Optimization

1. **Camera Culling**
   - Use Cinemachine's built-in culling
   - Disable inactive cameras
   - Optimize camera update frequency

2. **Input Optimization**
   - Limit input processing to active cameras
   - Use object pooling for frequent operations
   - Cache frequently accessed components

## Best Practices

1. **Mode Organization**
   - Use descriptive names for camera modes
   - Group related modes in folders
   - Document mode purposes

2. **Prefab Management**
   - Keep camera prefabs organized
   - Use consistent naming conventions
   - Version control prefab changes

3. **Testing**
   - Test on multiple devices
   - Verify touch input responsiveness
   - Check performance on target platforms

## Migration from Other Systems

### From Standard Cinemachine

1. **Preserve Existing Cameras**
   - Keep existing Cinemachine setup
   - Gradually migrate to InteractiveCameraSystem
   - Use mode switching for new features

2. **Maintain Compatibility**
   - Keep existing camera references
   - Use adapter patterns if needed
   - Test thoroughly during migration

### From Custom Camera Systems

1. **Identify Components**
   - Map existing camera logic to modes
   - Convert input handling to components
   - Preserve important behaviors

2. **Gradual Migration**
   - Start with one camera mode
   - Test thoroughly before expanding
   - Keep fallback options available
