using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoPanel : MonoBehaviour
{
    [Header("편집 버튼")]
    public Button equipEditButton;

    public Button skillEditButton;

    [Header("캐릭터 이름")]
    [SerializeField] private TMP_Text characterNameText;

    [Header("장비 슬롯")]
    [SerializeField] private List<InventorySlot> equipmentSlots = new();

    [Header("액티브 스킬 슬롯")]
    [SerializeField] private Image[] activeSkillSlotImage = new Image[3];

    [Header("패시브 스킬 슬롯")]
    [SerializeField] private Image passiveSkillSlotImage;

    private EntryDeckData selectedUnitData;

    private void Awake()
    {
        // 편집 버튼에 클릭 이벤트 연결
        equipEditButton.onClick.AddListener(OnClickEquipUI);
        skillEditButton.onClick.AddListener(OnClickSkillUI);

        equipmentSlots.ForEach(slot => slot.Initialize(null, false));
    }

    private void UpdateEquipment(EntryDeckData data)
    {
        // 장비 표시
        equipmentSlots.ForEach(slot => slot.Initialize(null, false));
        if (data == null)
        {
            return;
        }

        foreach (KeyValuePair<EquipmentType, EquipmentItem> equipmentItem in data.EquippedItems)
        {
            equipmentSlots[(int)equipmentItem.Key].Initialize(equipmentItem.Value, false);
            equipmentSlots[(int)equipmentItem.Key].ShowEquipMark(false);
        }
    }

    private void UpdateEquipmentSkill(EntryDeckData data)
    {
        // 액티브 스킬들 표시
        if (data == null)
        {
            return;
        }

        for (int i = 0; i < activeSkillSlotImage.Length; i++)
        {
            if (data.SkillDatas[i] != null)
            {
                activeSkillSlotImage[i].sprite = data.SkillDatas[i].skillSo.SkillIcon;
            }

            else
            {
                activeSkillSlotImage[i].sprite = null;
            }
        }
    }

    public void SetData(EntryDeckData data)
    {
        if (data == null)
        {
            return;
        }

        // 이름 표시
        selectedUnitData = data;
        characterNameText.text = data.CharacterSo.UnitName;

        UpdateEquipment(selectedUnitData);
        UpdateEquipmentSkill(selectedUnitData);
    }

    // 장비 편집창 열기
    private void OnClickEquipUI()
    {
        List<EntryDeckData> entry = DeckSelectManager.Instance.GetSelectedDeck();

        if (entry != null)
        {
            SelectEquipUI ui = UIManager.Instance.GetUIComponent<SelectEquipUI>();
            ui.OnEquipChanged -= UpdateEquipment;
            ui.SetCurrentSelectedUnit(selectedUnitData);
            UIManager.Instance.Open(ui);
            ui.OnEquipChanged += UpdateEquipment;
        }
    }

    // 스킬 편집창 열기
    private void OnClickSkillUI()
    {
        List<EntryDeckData> entry = DeckSelectManager.Instance.GetSelectedDeck();

        if (entry != null)
        {
            SelectSkillUI ui = UIManager.Instance.GetUIComponent<SelectSkillUI>();
            ui.OnSkillChanged -= UpdateEquipmentSkill;
            ui.SetCurrentSelectedUnit(selectedUnitData);
            UIManager.Instance.Open(ui);
            ui.OnSkillChanged += UpdateEquipmentSkill;
        }
    }
}