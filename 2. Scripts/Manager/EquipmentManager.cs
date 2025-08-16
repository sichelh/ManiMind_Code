using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItem : InventoryItem
{
    public bool IsEquipped;
    public int EnhanceLevel;
    public EntryDeckData EquippedUnit;

    public EquipmentItemSO EquipmentItemSo => ItemSo as EquipmentItemSO;

    public EquipmentItem(EquipmentItemSO itemSo) : base(itemSo, 1)
    {
        IsEquipped = false;
        EnhanceLevel = 0;
    }

    public EquipmentItem(int id) : base(id)
    {
        IsEquipped = false;
        EnhanceLevel = 0;
    }

    public override InventoryItem Clone()
    {
        return new EquipmentItem(EquipmentItemSo);
    }

    public void EquipItem(EntryDeckData equipUnit)
    {
        IsEquipped = true;
        EquippedUnit = equipUnit;
    }

    public void UnEquipItem()
    {
        IsEquipped = false;
        EquippedUnit = null;
    }
}

public class EquipmentManager
{
    public PlayerUnitController PlayerUnitController { get; private set; }


    public EquipmentManager(PlayerUnitController playerUnitController)
    {
        PlayerUnitController = playerUnitController;
    }

    public Dictionary<EquipmentType, EquipmentItem> EquipmentItems { get; private set; } = new();

    public event Action<EquipmentType> OnEquipmentChanged;

    public void EquipItem(EquipmentItem item)
    {
        if (!item.EquipmentItemSo.IsEquipableByAllJobs && PlayerUnitController.PlayerUnitSo.JobType != item.EquipmentItemSo.JobType)
        {
            Debug.Log("장착 가능한 직업이 아닙니다.");
            return;
        }

        EquipmentType type = item.EquipmentItemSo.EquipmentType;

        EquipmentItems[type] = item;
        foreach (StatData stat in item.EquipmentItemSo.Stats)
        {
            PlayerUnitController.StatManager.ApplyStatEffect(stat.StatType, StatModifierType.Equipment, stat.Value);
        }

        item.IsEquipped = true;
        OnEquipmentChanged?.Invoke(type);
    }

    public void UnequipItem(EquipmentType equipmentType)
    {
        if (!EquipmentItems.TryGetValue(equipmentType, out EquipmentItem item) || item == null)
        {
            return;
        }

        foreach (StatData stat in item.EquipmentItemSo.Stats)
        {
            PlayerUnitController.StatManager.ApplyStatEffect(stat.StatType, StatModifierType.Equipment, -stat.Value);
        }

        item.IsEquipped = false;
        // item.LinkedSlot.SetEquipMark(false);
        EquipmentItems[equipmentType] = null;
        Debug.Log($"아이템 장착 해제 : {item.ItemSo.ItemName}");
        OnEquipmentChanged?.Invoke(equipmentType);
    }
}