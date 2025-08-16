using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CombineManager : SceneOnlySingleton<CombineManager>
{
    public event Action<InventoryItem> OnItemCombined;


    private readonly ItemTable itemTable = TableManager.Instance.GetTable<ItemTable>();
    private readonly Tier maxTier = Enum.GetValues(typeof(Tier)).Cast<Tier>().Max();

    public EquipmentItem TryCombine(List<EquipmentItem> items)
    {
        if (!items.TrueForAll(x => x != null))
        {
            AudioManager.Instance.PlaySFX(SFXName.RejectUISound.ToString());
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("아이템 3개를 선택해야 합성이 가능합니다.");
            return null;
        }

        AccountManager.Instance.UseGold(Define.RequierCombineItemGold, out bool result);
        if (!result)
        {
            AudioManager.Instance.PlaySFX(SFXName.RejectUISound.ToString());
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("골드가 부족합니다.");
            return null;
        }
        
        EquipmentType combineResultType = items[Random.Range(0, items.Count)].EquipmentItemSo.EquipmentType;
        Tier          nextTier          = items[0].EquipmentItemSo.Tier;
        if (items[0].EquipmentItemSo.Tier < maxTier)
        {
            nextTier += 1;
        }

        List<EquipmentItemSO> combineItemList = itemTable.GetEquipmentsByTypeAndTier(combineResultType, nextTier);

        if (combineItemList == null || combineItemList.Count == 0)
            return null;
        EquipmentItemSO combineItemSo = combineItemList[Random.Range(0, combineItemList.Count)];

        AudioManager.Instance.PlaySFX(SFXName.EquipmentCombineSound.ToString());
        return new EquipmentItem(combineItemSo);
    }
}