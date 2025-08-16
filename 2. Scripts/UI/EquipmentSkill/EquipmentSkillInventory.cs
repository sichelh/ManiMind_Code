using UnityEngine;


public class EquipmentSkillInventory : BaseSkillInventory
{
    [SerializeField] private SelectSkillUI selectSkillUI;


    private ScrollData<SkillData> selectedData;


    public SkillSlot GetSlotByItem(SkillData item)
    {
        foreach (RectTransform go in reuseScrollview.ItemList)
        {
            if (go.TryGetComponent(out SkillSlot slot) && slot.SkillData.skillSo.ID == item.skillSo.ID)
            {
                return slot;
            }
        }

        return null;
    }

    public void SelectItemSlot(SkillData data)
    {
        ScrollData<SkillData> newSelectedData = GetDataByItem(data);
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