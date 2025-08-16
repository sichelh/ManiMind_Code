using Cinemachine;
using System;
using UnityEngine;

public class VirtualCameraController : MonoBehaviour
{
   [SerializeField]public CinemachineVirtualCamera vCam;
   private CameraAdjustData cameraAdjustData;
   private CinemachineBasicMultiChannelPerlin perlin;
   public Animator CameraAnimator{get;set;}
   public Transform Target { get; set; }
   

   private void Awake()
   {
       vCam = GetComponent<CinemachineVirtualCamera>();
       cameraAdjustData = new CameraAdjustData(this);
       perlin = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
       CameraAnimator = GetComponent<Animator>();
   }

   public void FocusOnUnit()
   {
       vCam.LookAt = Target;
       // Camera.Follow = Target;
   }

   public void Unfocus()
   {
       vCam.LookAt = null;
       // Camera.Follow = null;
       // Camera.transform.rotation = cameraAdjustData.DefaultTransform.rotation;
   }

   public void ZoomInCamera()
   {
       vCam.m_Lens.FieldOfView -= cameraAdjustData.ZoomInFOVModifier;
   }

   public void ZoomOutCamera()
   {
       vCam.m_Lens.FieldOfView += cameraAdjustData.ZoomOutFOVModifier;
   }

   public void DefaultCamera()
   {
       vCam.m_Lens.FieldOfView = cameraAdjustData.DefaultFOV;
       vCam.transform.position = cameraAdjustData.DefaultPosition;
       vCam.transform.rotation = Quaternion.Euler(cameraAdjustData.DefaultRotation);
   }

   public void ShakeCamera()
   {
       if (perlin == null)
       {
           perlin = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
       }
       perlin.m_AmplitudeGain = cameraAdjustData.CameraShakeAmplitude;
       perlin.m_FrequencyGain = cameraAdjustData.CameraShakeFrequency;

   }

   public void StopShakeCamera()
   {
       if (perlin == null)
       {
           perlin = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
       }
       perlin.m_AmplitudeGain = 0f;
       perlin.m_FrequencyGain = 0f;
   }

   public void ChangeCamera()
   {
       vCam.m_Priority = cameraAdjustData.MainPriority;
   }

   public void ThrowCamera()
   {
       vCam.m_Priority = cameraAdjustData.SubPriority;
   }
   
   
   
}

