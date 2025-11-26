# Changelog

All notable changes to the InteractiveCameraSystem package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-10-22

### Added
- Initial release of InteractiveCameraSystem
- Complete camera management system with Cinemachine integration
- Touch input support (drag, pinch-zoom, edge-pan gestures)
- Camera mode switching system with seamless transitions
- Target management with VirtualCameraAnchor system
- Editor tools for easy setup and configuration
- Prefab-based workflow for quick integration
- Context menus for rapid camera mode creation
- Comprehensive documentation and examples
- Support for Unity 2022.3+ and Cinemachine 2.9.0+

### Features
- **CinemachineCameraManager** - Main camera management singleton
- **CameraMode** - ScriptableObject-based camera configuration
- **CinemachineDragInputComponent** - Touch drag input handling
- **CinemachineZoomInputComponent** - Pinch-zoom input handling
- **VirtualCameraAnchor** - Camera target management
- **FingerManager** - Lean Touch integration and gesture detection
- **CinemachineInputManager** - Input event coordination
- **Editor Tools** - Setup, cleanup, and debugging tools

### Examples
- ExplorationMode - Free camera movement with drag and zoom
- ConversationMode - Framed camera for character conversations  
- IntroMode - Cinematic camera for intro sequences
- Example prefabs and test scenes

---

## [Unreleased]

### Planned
- Additional camera modes (orbit, follow, cinematic)
- Enhanced input customization options
- Performance optimizations
- Additional editor tools
- Mobile-specific optimizations
