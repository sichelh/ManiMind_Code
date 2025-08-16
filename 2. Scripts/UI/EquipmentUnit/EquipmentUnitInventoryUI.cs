using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUnitInventoryUI : BaseInventoryUI
{
    [SerializeField] private SelectEquipUI selectEquipUI;

    private JobType selectedUnitJobType;


    private ScrollData<InventoryItem> selectedData;

    public InventorySlot GetSlotByItem(EquipmentItem item)
    {
        foreach (RectTransform go in reuseScrollview.ItemList)
        {
            if (go.TryGetComponent(out InventorySlot slot) && slot.Item.InventoryId == item.InventoryId)
            {
                return slot;
            }
        }

        return null;
    }

    public void SelectItemSlot(EquipmentItem data)
    {
        ScrollData<InventoryItem> newSelectedData = GetDataByItem(data);
        if (selectedData != newSelectedData)
        {
            if (selectedData != null)
            {
                selectedData.IsSelected = false;
                RefreshAtSlotUI(selectedData.Data);
            }

            selectedData = newSelectedData;
        }

        selectedData.IsSelected = true;
        RefreshAtSlotUI(selectedData.Data);
    }
}