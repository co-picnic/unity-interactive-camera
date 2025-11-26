# InteractiveCameraSystem Examples

This folder contains example configurations, prefabs, and scenes to help you get started with the InteractiveCameraSystem plugin.

## 📋 **Prerequisites**

Before using this plugin, ensure your Unity project has the following packages installed:

### Required Packages
- **Unity Cinemachine** (2.9.0 or later) - For camera management
- **Unity Input System** (1.6.0 or later) - For input handling

### Required Assets
- **Lean Touch** - For touch input handling (available on Unity Asset Store)
  - The plugin references Lean Touch components
  - You must have Lean Touch installed in your project
  - Download from: [Unity Asset Store - Lean Touch](https://assetstore.unity.com/packages/tools/input-management/lean-touch-30111)

## 📁 Folder Structure

- **CameraModeAssets/** - Ready-to-use CameraMode ScriptableObject assets
- **CinemachineCameras/** - Camera mode prefabs (included with plugin)
- **Editor/** - Editor tools (Setup Window)

## 🎯 Available CameraModes

### Your Camera Modes
- **Conversation Mode** - Optimized for conversation scenes
- **Exploration Mode** - Perfect for exploration gameplay
- **Intro Mode** - Cinematic intro camera

## 🏗️ Available Prefabs

### Core Components
- **CinemachineCameraManager.prefab** - Camera manager component (in main Prefabs folder)

### Camera Mode Prefabs
- **ConversationMode_Camera.prefab** - Your existing conversation camera
- **ExplorationMode_Camera.prefab** - Your existing exploration camera  
- **IntroMode_Camera.prefab** - Your existing intro camera

## 🛠️ Tools Menu

### Tools > InteractiveCameraSystem >
- **Setup Camera System** - Creates all camera system components
- **Create CameraMode Assets** - Creates CameraMode ScriptableObject assets

## 🚀 Getting Started

### 📦 **Installation Steps**

1. **Install Required Packages**:
   - Open `Window > Package Manager`
   - Install **Cinemachine** (2.9.0+)
   - Install **Input System** (1.6.0+)

2. **Install Lean Touch**:
   - Go to [Unity Asset Store - Lean Touch](https://assetstore.unity.com/packages/tools/input-management/lean-touch-30111)
   - Download and import Lean Touch into your project
   - This is required for touch input functionality

3. **Import InteractiveCameraSystem Plugin**:
   - Copy the plugin folder to your project's `Assets/Plugins/` directory
   - Unity will automatically compile the plugin

4. **Verify Installation**:
   - Check that `Tools > InteractiveCameraSystem > Setup Camera System` appears in the menu
   - This confirms the plugin is properly installed

### 🎯 **Quick Setup with Prefabs (Recommended)**

1. **Open the Project window** and navigate to `Plugins/InteractiveCameraSystem/Prefabs/`
2. **Drag `CinemachineCameraManager.prefab`** into your scene
3. **Navigate to** `Plugins/InteractiveCameraSystem/Examples/CinemachineCameras/`
4. **Drag your desired camera mode prefab** into your scene
5. **That's it!** Your camera system is ready to use

**Benefits:**
- ✅ **Instant Setup** - No configuration needed
- ✅ **Complete System** - All components included
- ✅ **Proper Hierarchy** - Everything organized under one parent
- ✅ **Ready to Use** - Just drag and drop!

### 🔧 **Manual Setup with Tools**

1. **Open the Editor Window** - Go to `Tools > InteractiveCameraSystem > Setup Camera System`
2. **Configure settings** - Choose which components to create
3. **Click "Setup Camera System"** button
4. **Check Console** for setup progress messages
5. **All components created** automatically under "CinemachineCamera" parent GameObject!

**Note**: This works in both Edit Mode and Play Mode.

### Available Prefabs

The plugin includes ready-to-use prefabs that come with the plugin:

#### Core Components
- **CinemachineCameraManager.prefab** - Camera manager component (in main Prefabs folder)
- **IntroSequenceController** - Script for managing intro camera sequences

#### Camera Mode Prefabs (Examples/CinemachineCameras/)
- **ConversationMode_Camera.prefab** - Your existing conversation camera
- **ExplorationMode_Camera.prefab** - Your existing exploration camera  
- **IntroMode_Camera.prefab** - Your existing intro camera

### Cleanup

- **Open Setup Window** - Go to `Tools > InteractiveCameraSystem > Setup Camera System`
- **Click "Clean Up Camera System"** button

## 📝 Usage Examples

### Basic Usage
1. **Add CinemachineCameraManager** to your scene
2. **Add your desired camera mode prefab** (Conversation, Exploration, or Intro)
3. **Add IntroSequenceController** to manage intro sequences
4. **Configure targets** and **adjust settings** as needed
5. **Press Play** and test your camera system

### Intro Sequence Setup
1. **Add IntroSequenceController** to a GameObject in your scene
2. **Assign Intro Mode** - Drag your intro camera mode to the introMode field
3. **Assign Gameplay Mode** - Drag your gameplay camera mode to the gameplayMode field
4. **Set Duration** - Configure how long the intro should last
5. **Test** - The controller will automatically switch from intro to gameplay mode

### Creating CameraMode Assets
1. **Open the Creator Tool** - Go to `Tools > InteractiveCameraSystem > Create CameraMode Assets`
2. **Click "Create IntroMode Asset"** - Creates an asset for intro sequences
3. **Click "Create GameplayMode Asset"** - Creates an asset for gameplay
4. **Assign Virtual Camera** - Drag a CinemachineCamera from your scene or prefab to the `virtualCamera` field
5. **Use with IntroSequenceController** - Drag the assets to the IntroSequenceController fields

### Using Existing Plugin Assets
The plugin includes ready-to-use CameraMode assets:
- **IntroMode.asset** - For intro sequences (follow/zoom/drag disabled)
- **GameplayMode.asset** - For gameplay (all features enabled)
- **ConversationMode.asset** - For conversations (follow enabled, zoom/drag disabled)
- **ExplorationMode.asset** - For exploration (all features enabled)

Simply assign your CinemachineCamera to the `virtualCamera` field in any of these assets.

### Manual Creation (Alternative)
1. **Right-click** in Project window → `Create → Camera → Camera Mode`
2. **Configure settings** - Set mode name, description, and camera behaviors
3. **Assign Virtual Camera** - Drag a CinemachineCamera from your scene or prefab
4. **Save** - The asset will be ready to use with IntroSequenceController

### Advanced Usage
- **Mix and match** different camera modes
- **Create custom camera modes** based on the existing ones
- **Use CameraMode ScriptableObjects** for runtime camera switching

## 🎯 Next Steps

1. **Test the plugin** in your scenes
2. **Customize camera settings** for your specific needs
3. **Create additional camera modes** if needed
4. **Integrate with your game systems**

## 🚨 **Troubleshooting**

### Common Issues

**"Can't assign CameraMode to IntroSequenceController"**
- **Solution**: Use the included CameraMode assets in `Examples/CameraModeAssets/`
- **Alternative**: Use `Tools > InteractiveCameraSystem > Create CameraMode Assets` to create new assets
- **Check**: Ensure the CameraMode asset has a valid CinemachineCamera assigned

**"CameraMode shows missing reference"**
- **Solution**: Assign a CinemachineCamera to the virtualCamera field in the CameraMode asset
- **Check**: Ensure the CinemachineCamera exists in your scene or is a prefab

**"Lean Touch components not found"**
- **Solution**: Install Lean Touch from the Unity Asset Store
- **Check**: Ensure Lean Touch is imported and compiled successfully

**"Cinemachine components missing"**
- **Solution**: Install Cinemachine package via Package Manager
- **Check**: Verify Cinemachine version is 2.9.0 or later

**"Input System not working"**
- **Solution**: Install Input System package via Package Manager
- **Check**: Ensure Input System is enabled in Project Settings

**"Plugin not appearing in Tools menu"**
- **Solution**: Check that the plugin is in `Assets/Plugins/` folder
- **Check**: Verify there are no compilation errors in Console

### Getting Help

If you encounter issues:
1. **Check Console** for error messages
2. **Verify all dependencies** are installed correctly
3. **Ensure Unity version** is 2022.3 or later
4. **Check plugin folder structure** matches the documentation

## 📚 Additional Resources

- **Unity Cinemachine Documentation** - Learn more about Cinemachine
- **Lean Touch Documentation** - Input system documentation
- **Plugin Source Code** - Explore the plugin implementation