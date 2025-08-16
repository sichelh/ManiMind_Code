using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCharacterSettingUI : MonoBehaviour
{
    public void OnClickOpenCharacterSettingUI()
    {
        UIManager.Instance.Open(UIManager.Instance.GetUIComponent<UICharacterSetting>());
    }
}