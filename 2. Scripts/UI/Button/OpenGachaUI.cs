using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenGachaUI : MonoBehaviour
{
    public void OnClickOpenGachaUI()
    {
        UIManager.Instance.Open(UIManager.Instance.GetUIComponent<GachaUI>());
    }
}
