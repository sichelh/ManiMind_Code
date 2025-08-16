using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseInventoryUI : MonoBehaviour
{
    [SerializeField] protected ReuseScrollview<InventoryItem> reuseScrollview;

    protected InventoryManager InventoryManager => InventoryManager.Instance;
    protected UIManager        UIManager        => UIManager.Instance;

    protected Func<List<InventoryItem>> GetInventorySource;
    protected List<InventoryItem> inventoryList;

    public ReuseScrollview<InventoryItem> ReuseScrollview => reuseScrollview;

    public virtual void Initialize(Func<List<InventoryItem>> inventoryGetter, Action<InventorySlot> onClickHandler)
    {
        GetInventorySource = inventoryGetter;

        reuseScrollview.SetData(GetInventorySource());
        inventoryList = GetInventorySource();
        int count = Mathf.Min(reuseScrollview.ItemList.Count, inventoryList.Count);

        for (int i = 0; i < count; i++)
        {
            if (reuseScrollview.ItemList[i].TryGetComponent(out InventorySlot slot))
            {
                slot.SetOnClickCallback(onClickHandler);
            }
        }
    }

    public ScrollData<InventoryItem> GetDataByItem(InventoryItem item)
    {
        return reuseScrollview.GetDataFromItem(item);
    }

    public void RefreshAtSlotUI(InventoryItem item)
    {
        int index = reuseScrollview.GetDataIndexFromItem(item);
        reuseScrollview.RefreshSlotAt(index);
    }

    private void ClearClickHandlers()
    {
        if (reuseScrollview?.ItemList == null)
        {
            return;
        }

        foreach (RectTransform obj in reuseScrollview.ItemList)
        {
            if (obj.TryGetComponent(out InventorySlot slot))
            {
                slot.SetOnClickCallback(null);
            }
        }
    }


    private void OnDestroy()
    {
        ClearClickHandlers();
    }
}