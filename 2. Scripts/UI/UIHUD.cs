using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHUD : MonoBehaviour
{
    private UIManager          UIManager        => UIManager.Instance;
    private UIEquipmentCombine EquipmentCombine => UIManager.GetUIComponent<UIEquipmentCombine>();
    private UIDeckBuilding     DeckBuilding     => UIManager.GetUIComponent<UIDeckBuilding>();


    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void OnClickOpenEquipmentCombineBtn()
    {
        UIManager.Open(EquipmentCombine);
    }

    public void OnClickOpenEditPartyBtn()
    {
        UIManager.Open(DeckBuilding);
    }


    public void OnClickGoldBtn()
    {
#if UNITY_EDITOR
        AccountManager.Instance.AddGold(10000);
#endif
        if (GameManager.Instance.isTestMode)
        {
            AccountManager.Instance.AddGold(10000);
        }
    }

    public void OnClickOpalBtn()
    {
#if UNITY_EDITOR
        AccountManager.Instance.AddOpal(10000);
#endif
        if (GameManager.Instance.isTestMode)
        {
            AccountManager.Instance.AddOpal(10000);
        }
    }
}