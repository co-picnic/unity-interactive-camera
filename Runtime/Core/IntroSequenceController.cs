using UnityEngine;
using InteractiveCameraSystem;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Controller for managing intro camera sequences
    /// Automatically switches from intro mode to gameplay mode after a specified duration
    /// </summary>
    public class IntroSequenceController : MonoBehaviour
    {
        [Header("Camera Modes")]
        [SerializeField] private CameraMode introMode;
        [SerializeField] private CameraMode gameplayMode;
        
        [Header("Timing")]
        [SerializeField] private float introDuration = 3f;
        
        [Header("Debug Options")]
        [SerializeField] private bool skipIntroInEditor = false;

        void Start()
        {
            StartIntroSequence();
        }

        /// <summary>
        /// Starts the intro sequence by switching to intro mode
        /// </summary>
        void StartIntroSequence()
        {
            if (CinemachineCameraManager.Instance == null)
            {
                Debug.LogError("IntroSequenceController: CinemachineCameraManager not found in scene!");
                return;
            }

            CinemachineCameraManager.Instance.SwitchToMode(introMode, true);

            // Skip intro in editor if enabled
            if (skipIntroInEditor && Application.isEditor)
            {
                EndIntroSequence();
                return;
            }

            Invoke(nameof(EndIntroSequence), introDuration);
        }

        /// <summary>
        /// Ends the intro sequence by switching to gameplay mode
        /// </summary>
        void EndIntroSequence()
        {
            if (CinemachineCameraManager.Instance == null)
            {
                Debug.LogError("IntroSequenceController: CinemachineCameraManager not found in scene!");
                return;
            }

            CinemachineCameraManager.Instance.SwitchToMode(gameplayMode, true);
        }

        /// <summary>
        /// Manually end the intro sequence (useful for debugging or user input)
        /// </summary>
        public void SkipIntro()
        {
            CancelInvoke(nameof(EndIntroSequence));
            EndIntroSequence();
        }

        /// <summary>
        /// Restart the intro sequence
        /// </summary>
        public void RestartIntro()
        {
            CancelInvoke(nameof(EndIntroSequence));
            StartIntroSequence();
        }
    }
}
