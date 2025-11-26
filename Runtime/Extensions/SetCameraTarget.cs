using UnityEngine;
using InteractiveCameraSystem;

public class SetCameraTarget : MonoBehaviour
{

    private CinemachineCameraManager cinemachineCameraManager;
    [SerializeField]
    private Transform cameraTarget;
    [SerializeField]
    private float cameraTargetRadius = 6f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cinemachineCameraManager = CinemachineCameraManager.Instance;
        if(cameraTarget != null)
        {
            cinemachineCameraManager.AddTarget(cameraTarget, cameraTargetRadius);
        }
    }
}
