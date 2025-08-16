using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PanelSelectedUnitInfo : MonoBehaviour
{
    [FormerlySerializedAs("inventoryItems")]
    [SerializeField] private InventorySlot[] unitEquippedItems;

    [SerializeField] private SkillSlot passiveSkillSlot;
    [SerializeField] private SkillSlot[] activeSkillSlots;

    [SerializeField] private UnitSlot unitSlot;

    [SerializeField] private CanvasGroup BG;

    private EntryDeckData selectedUnitData;

    private SelectEquipUI SelectEquipUI => UIManager.Instance.GetUIComponent<SelectEquipUI>();
    private SelectSkillUI SelectSkillUI => UIManager.Instance.GetUIComponent<SelectSkillUI>();

    public void SetInfoPanel(EntryDeckData data)
    {
        data.OnEquipmmmentChanged -= UpdateEquippedItemSlot;
        data.OnSkillChanged -= UpdateEquippedSkillSlot;
        selectedUnitData = data;
        unitSlot.Initialize(data);
        UpdateEquippedItemSlot();
        UpdateEquippedSkillSlot();
        data.OnEquipmmmentChanged += UpdateEquippedItemSlot;
        data.OnSkillChanged += UpdateEquippedSkillSlot;
    }

    public void OpenPanel()
    {
        AudioManager.Instance.PlaySFX(SFXName.OpenUISound.ToString());
        gameObject.SetActive(true);
        passiveSkillSlot.SetSkillIcon(selectedUnitData.CharacterSo.PassiveSkill, false);
        passiveSkillSlot.ShowEquipMark(false);
        BG.alpha = 0;
        BG.DOFade(1f, 0.3f).SetEase(Ease.InOutSine);
    }

    public void ClosePanel()
    {
        AudioManager.Instance.PlaySFX(SFXName.CloseUISound.ToString());
        Sequence seq = DOTween.Sequence();

        seq.Append(BG.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.AppendCallback(() => gameObject.SetActive(false));

        if (selectedUnitData != null)
        {
            selectedUnitData.OnEquipmmmentChanged -= UpdateEquippedItemSlot;
            selectedUnitData.OnSkillChanged -= UpdateEquippedSkillSlot;
            selectedUnitData = null;
        }
    }


    public void OnClickEditEquippedItemBtn()
    {
        SelectEquipUI.SetCurrentSelectedUnit(selectedUnitData);
        UIManager.Instance.Open(SelectEquipUI);
    }

    public void OnClickEditSkillBtn()
    {
        SelectSkillUI.SetCurrentSelectedUnit(selectedUnitData);
        UIManager.Instance.Open(SelectSkillUI);
    }

    private void UpdateEquippedItemSlot()
    {
        Dictionary<EquipmentType, EquipmentItem> equipItem = selectedUnitData.EquippedItems;
        for (int i = 0; i < unitEquippedItems.Length; i++)
        {
            if (equipItem.TryGetValue((EquipmentType)i, out EquipmentItem item))
            {
                unitEquippedItems[i].Initialize(item, false);
            }
            else
            {
                unitEquippedItems[i].Initialize(null, false);
            }

            unitEquippedItems[i].ShowEquipMark(false);
        }
    }

    private void UpdateEquippedSkillSlot()
    {
        SkillData[] equipSkill = selectedUnitData.SkillDatas;
        for (int i = 0; i < equipSkill.Length; i++)
        {
            SkillData skill = equipSkill[i];
            activeSkillSlots[i].SetSkillIcon(skill, false);
            activeSkillSlots[i].ShowEquipMark(false);
        }
    }
}