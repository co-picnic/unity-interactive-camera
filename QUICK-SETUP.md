# InteractiveCameraSystem - Quick Setup Guide

## 🚀 One-Click Installation

### Automatic Setup (Recommended)
1. **Import the package** via Package Manager
2. **Run dependency check**: `Tools > Interactive Camera System > Check Dependencies`
3. **Click "Install Packages"** when prompted
4. **Done!** ✅

### Manual Setup (Alternative)
If automatic installation doesn't work, install these packages manually:

#### Required Packages:
1. **Cinemachine** (2.9.0+)
   - `Window > Package Manager > Unity Registry > Cinemachine`

#### Optional Packages:
2. **Lean Touch** (Optional - for advanced touch features)
   - Asset Store: Search "Lean Touch" by Carlos Wilkes
   - Or GitHub: https://github.com/carloswilkes/LeanTouch
   - **Note**: If not installed, the system uses a minimal built-in implementation

## 🔧 Verification

After installation, verify everything works:

1. **Check Console**: No compilation errors
2. **Test Scene**: Create a new scene and add `CinemachineCameraManager`
3. **Run Setup**: `Tools > Interactive Camera System > Check Dependencies`

## 📦 Package Structure

```
InteractiveCameraSystem/
├── Runtime/           # Core functionality
├── Editor/            # Editor tools and setup
├── Examples/          # Sample scenes and prefabs
└── Documentation/     # Guides and API reference
```

## 🆘 Troubleshooting

### Common Issues:

**"LeanTouch assembly not found"**
- This is normal! The system will use the built-in minimal implementation
- For advanced features, install Lean Touch from Asset Store or GitHub

**"Cinemachine not found"**
- Install Cinemachine via Package Manager
- Ensure version 2.9.0 or higher

**"Compilation errors"**
- Run `Tools > Interactive Camera System > Check Dependencies`
- Ensure Cinemachine is installed

### Getting Help:
- Check the Documentation folder
- Run `Tools > Interactive Camera System > Check Dependencies`
- Review Console for specific error messages

## 🎯 Quick Start

1. **Create a new scene**
2. **Add CinemachineCameraManager** to a GameObject
3. **Configure camera modes** in the inspector
4. **Test with the example prefabs** in the Examples folder

---

**Need more help?** Check the Documentation folder for detailed guides!
