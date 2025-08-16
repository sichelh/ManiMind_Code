using Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraManager : SceneOnlySingleton<CameraManager>
{
    [SerializeField]public CinemachineBrain cinemachineBrain;
    [SerializeField]public VirtualCameraController mainCameraController;
    [SerializeField]public VirtualCameraController skillCameraController;

    [SerializeField] private CinemachineVirtualCamera startCam;
    [SerializeField] private CinemachineVirtualCamera mainCam;

    public bool followNextIEffectProvider = true;
    private LoadingScreenController loadingScreenController;

    private void OnEnable()
    {
        loadingScreenController = LoadingScreenController.Instance;
        loadingScreenController.OnLoadingComplete += WaitForLoading;
        startCam.Priority = 20;
        mainCam.Priority = 11;
    }

    private void WaitForLoading()
    {
        startCam.Priority = 5;
        mainCam.Priority = 11;
    }

    public void ChangeFollowTarget(IEffectProvider target)
    {
        if (followNextIEffectProvider)
        {
            skillCameraController.Target = target.Collider.transform;
            skillCameraController.FocusOnUnit();
        }
        else
        {
            followNextIEffectProvider = true;
        }
    }

    public void ChangeMainCamera()
    {
        mainCameraController.ChangeCamera();
        skillCameraController.ThrowCamera();
        TimeLineManager.Instance.CurrentCameraController = mainCameraController;
    }
    
    public void ChangeSkillCamera()
    {
        skillCameraController.ChangeCamera();
        mainCameraController.ThrowCamera();     
        TimeLineManager.Instance.CurrentCameraController = skillCameraController;
    }

    private void OnDisable()
    {
        if (loadingScreenController != null)
            loadingScreenController.OnLoadingComplete -= WaitForLoading;
    }
}
