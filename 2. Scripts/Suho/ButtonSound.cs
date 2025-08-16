using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour
{
    // 호버 사운드 재생 함수
    public void PlayHoverSound(BaseEventData arg0)
    {
        AudioManager.Instance.PlaySFX(SFXName.HoverUISound.ToString());
    }
    
    public void PlayHoverSound()
    {
        AudioManager.Instance.PlaySFX(SFXName.HoverUISound.ToString());
    }

    
   
}