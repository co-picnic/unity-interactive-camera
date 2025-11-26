# InteractiveCameraSystem Plugin Testing Guide

## 🧪 How to Test the Plugin

### Comprehensive Test (Requires Cinemachine)
1. **Open Unity Editor** with the `rising-tides-app` project
2. **Create a new scene**: `File > New Scene > Save As > PluginTestScene`
3. **Add Event System** (Required for Lean Touch):
   - Go to `GameObject > UI > Event System`
   - This is required for Lean Touch input to work properly
4. **Add test objects**:
   - Create empty GameObject → Add `ComprehensivePluginTest` component
   - Create empty GameObject → Add `CinemachineCameraManager` component
   - Create empty GameObject → Add `CinemachineInputManager` component
   - Create empty GameObject → Add `VirtualCameraAnchor` component
   - Create empty GameObject → Add `FingerManager` component
   - Create empty GameObject → Add `CinemachineBrain` component (from Cinemachine)
5. **Press Play** and check Console for test results

### Expected Results
- ✅ **Camera Manager Test: PASSED**
- ✅ **Anchor System Test: PASSED** 
- ✅ **Input System Test: PASSED**
- ✅ **Singleton Test: PASSED**
- ✅ **Cinemachine Integration Test: PASSED**

### What to Look For
- **Console messages** showing ✅ (success) or ❌ (failure)
- **No compilation errors** in the Console
- **Components can be added** to GameObjects without errors
- **Inspector shows** all plugin components correctly

### If Tests Fail
1. **Check Console** for specific error messages
2. **Verify assembly references** are correct
3. **Ensure Lean Touch** is properly imported
4. **Check Cinemachine** is installed and working
5. **Add Event System** if you see Lean Touch errors about missing Event System

### Common Issues & Solutions
- **"Failed to RaycastGui because your scene doesn't have an event system!"**
  - Solution: Add Event System via `GameObject > UI > Event System`
- **"[CinemachineInputManager] No instance found in scene!"**
  - Solution: Add CinemachineInputManager component to a GameObject
- **"The type or namespace name 'Cinemachine' could not be found"**
  - Solution: Ensure Cinemachine package is installed and assembly references are correct
- **"FingerManager not found"**
  - Solution: Add FingerManager component to a GameObject in the scene

### Success Criteria
- **4/5 tests pass** = Plugin is ready for migration
- **All tests pass** = Plugin is fully functional
- **Less than 4 tests pass** = Plugin needs fixes

## 🚀 Next Steps After Testing

Once tests pass:
1. **Update project references** to use plugin
2. **Remove old camera system files**
3. **Update prefabs and scenes**
4. **Test in actual game scenarios**

## 📝 Test Components Available

- `ComprehensivePluginTest` - Full plugin test suite with Cinemachine integration
- `InteractiveCameraSystem.Tests.asmdef` - Test assembly definition with proper Cinemachine reference
