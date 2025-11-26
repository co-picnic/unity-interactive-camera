# InteractiveCameraSystem

A comprehensive Unity plugin for managing interactive camera behaviors with Cinemachine. This plugin provides a complete camera system with touch input support, camera mode switching, and seamless integration with Unity's Cinemachine package.

## Features

- 🎥 **Cinemachine Integration** - Built on top of Unity's Cinemachine package
- 📱 **Touch Input Support** - Drag, pinch-zoom, and edge-pan gestures
- 🔄 **Camera Mode Switching** - Seamless transitions between different camera modes
- 🎯 **Target Management** - Dynamic camera targeting with anchor system
- 🛠️ **Editor Tools** - Easy setup and configuration tools
- 📦 **Prefab-Based** - Ready-to-use prefabs for quick setup
- 🔧 **Extensible** - Modular design for easy customization

## Requirements

- Unity 2022.3 or later
- Cinemachine 2.9.0 or later
- Lean Touch (included in plugin)

## Quick Start

### 1. Import the Plugin

Add the InteractiveCameraSystem plugin to your Unity project by importing the `.unitypackage` file.

### 2. Setup Camera System

1. **Drag the CinemachineCameraManager prefab** from `Examples/Prefabs/Manager/` into your scene
2. **Assign camera modes** in the CinemachineCameraManager inspector
3. **Click "Auto-Discover Modes"** to automatically find camera modes from the plugin folder
4. **Click "Instantiate All Cameras"** to create camera instances in the scene

### 3. Configure Camera Modes

The plugin includes example camera modes:
- **ExplorationMode** - Free camera movement with drag and zoom
- **ConversationMode** - Framed camera for character conversations
- **IntroMode** - Cinematic camera for intro sequences

## Camera Modes

### Creating Camera Modes

1. **Right-click on a CinemachineCamera** in the scene
2. **Select "Create Camera Mode"** from the context menu
3. **Configure the camera mode** in the inspector
4. **The mode will be automatically added** to the CinemachineCameraManager

### Camera Mode Properties

- **Mode Name** - Display name for the camera mode
- **Description** - Optional description
- **Virtual Camera** - Reference to the CinemachineCamera prefab
- **Priority** - Camera priority (higher = more important)
- **Enable Drag** - Allow drag input for camera movement
- **Enable Zoom** - Allow pinch-zoom input
- **Enable Follow** - Enable camera following behavior
- **Camera Settings** - Field of view, clipping planes, etc.

## Input System

### Touch Gestures

- **Single Finger Drag** - Move camera around the scene
- **Pinch Zoom** - Zoom in/out with two fingers
- **Edge Pan** - Pan camera when touching screen edges

### Input Configuration

Configure input behavior in the camera mode settings:

- **Horizontal/Vertical Sensitivity** - How responsive the camera is to input
- **Invert Controls** - Reverse input directions
- **Damping** - Smooth camera movement
- **Momentum** - Camera continues moving after releasing input

## Camera Anchors

### VirtualCameraAnchor

Use `VirtualCameraAnchor` components to define camera targets:

```csharp
// Add anchor to a GameObject
var anchor = gameObject.AddComponent<VirtualCameraAnchor>();

// Set as active target
CinemachineCameraManager.Instance.SetActiveAnchor(anchor);
```

### Multiple Targets

For conversation cameras, use multiple anchors:

```csharp
// Set multiple targets for group framing
var anchors = new ICameraAnchor[] { anchor1, anchor2, anchor3 };
CinemachineCameraManager.Instance.SetMultipleActiveAnchors(anchors);
```

## API Reference

### CinemachineCameraManager

The main camera manager singleton:

```csharp
// Switch to a camera mode
CinemachineCameraManager.Instance.SwitchToMode(cameraMode, smoothTransition: true);

// Set active camera target
CinemachineCameraManager.Instance.SetActiveAnchor(anchor);

// Set multiple targets
CinemachineCameraManager.Instance.SetMultipleActiveAnchors(anchors);

// Add/remove camera modes
CinemachineCameraManager.Instance.AddCameraMode(mode);
CinemachineCameraManager.Instance.RemoveCameraMode(modeName);
```

### Camera Mode Switching

```csharp
// Switch to exploration mode
var explorationMode = // ... get mode reference
CinemachineCameraManager.Instance.SwitchToMode(explorationMode);

// Switch to conversation mode with targets
var conversationMode = // ... get mode reference
var characterAnchors = // ... get character anchors
CinemachineCameraManager.Instance.SetMultipleActiveAnchors(characterAnchors);
CinemachineCameraManager.Instance.SwitchToMode(conversationMode);
```

## Editor Tools

### CinemachineCameraManager Inspector

- **Auto-Discover Modes** - Find camera modes from plugin folder
- **Instantiate All Cameras** - Create camera instances in scene
- **Mode List** - View and manage camera modes
- **Debug Info** - System status and diagnostics

### Tools Menu

Access additional tools via `Tools > InteractiveCameraSystem`:

- **Setup Camera System** - Create all components from scratch
- **Clean Up Camera System** - Remove all components

## Examples

### Basic Camera Setup

```csharp
public class CameraController : MonoBehaviour
{
    public CameraMode explorationMode;
    public CameraMode conversationMode;
    
    void Start()
    {
        // Start in exploration mode
        CinemachineCameraManager.Instance.SwitchToMode(explorationMode);
    }
    
    public void StartConversation(Transform character1, Transform character2)
    {
        // Set conversation targets
        var anchors = new ICameraAnchor[] {
            character1.GetComponent<ICameraAnchor>(),
            character2.GetComponent<ICameraAnchor>()
        };
        
        CinemachineCameraManager.Instance.SetMultipleActiveAnchors(anchors);
        CinemachineCameraManager.Instance.SwitchToMode(conversationMode);
    }
}
```

### Custom Input Handling

```csharp
public class CustomInputHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Switch camera mode on space key
            var manager = CinemachineCameraManager.Instance;
            var currentMode = manager.GetCurrentMode();
            
            if (currentMode.modeName == "Exploration")
            {
                // Switch to conversation mode
                manager.SwitchToMode(conversationMode);
            }
            else
            {
                // Switch back to exploration
                manager.SwitchToMode(explorationMode);
            }
        }
    }
}
```

## Troubleshooting

### Common Issues

**Camera not responding to input:**
- Ensure `CinemachineInputManager` is in the scene
- Check that `FingerManager` is present
- Verify camera mode has input enabled (Enable Drag/Zoom)

**Camera modes not found:**
- Use "Auto-Discover Modes" button in CinemachineCameraManager inspector
- Ensure camera mode assets are in the correct folder
- Check that camera modes reference valid CinemachineCamera prefabs

**Zoom not working:**
- Test in Game View (not Simulator View) for Lean Touch simulator
- Verify zoom is enabled in camera mode settings
- Check that `CinemachineZoomInputComponent` is added to camera

**Target group not updating:**
- Ensure characters have `VirtualCameraAnchor` components
- Check that anchors are valid (`IsValid` returns true)
- Verify camera mode uses group framing

### Debug Mode

Enable debug mode in CinemachineCameraManager for detailed logging:

```csharp
CinemachineCameraManager.Instance.debugMode = true;
```

## File Structure

```
InteractiveCameraSystem/
├── Runtime/
│   ├── Core/                    # Core camera management
│   ├── Input/                   # Input components
│   ├── Modes/                   # Camera mode definitions
│   └── Extensions/              # Cinemachine extensions
├── Editor/                      # Editor tools and inspectors
├── Examples/
│   ├── Prefabs/                 # Ready-to-use prefabs
│   ├── CameraModeAssets/        # Example camera modes
│   └── Editor/                  # Example editor tools
├── Integrations/
│   └── FingerManager/           # Lean Touch integration
└── Tests/                       # Test scripts
```

## Version History

### v1.0.0
- Initial release
- Complete camera system with Cinemachine integration
- Touch input support (drag, zoom, edge-pan)
- Camera mode switching system
- Editor tools and prefabs
- Example camera modes and scenes

## License

This plugin is provided as-is for use in Unity projects. Please refer to the license terms included with the plugin.

## Support

For issues, questions, or feature requests, please refer to the plugin documentation or contact the development team.

---

**InteractiveCameraSystem** - Professional camera management for Unity projects.