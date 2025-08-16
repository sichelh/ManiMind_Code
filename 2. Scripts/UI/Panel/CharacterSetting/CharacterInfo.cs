using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CharacterInfo : MonoBehaviour
{
    [SerializeField] private PlayerUnitIncreaseSo statIncreaseSo;

    [SerializeField] private RectTransform panelRect;
    [SerializeField] private TextMeshProUGUI unitName;
    [SerializeField] private TextMeshProUGUI unitLevel;
    [SerializeField] private StatSlot[] statSlots;

    [SerializeField] private InventorySlot[] equippedItemSlot;
    [SerializeField] private SkillSlot[] skillSlots;

    [Header("UnitLevelUpPanel")]
    [SerializeField] private UnitLevelUpPanel unitLevelUpPanel;


    private UICharacterSetting uiCharacterSetting;
    private Vector2 onScreenPos;
    private Vector2 offScreenPos;
    private EntryDeckData selectedPlayerUnitData;


    private Dictionary<StatType, StatSlot> statSlotDic = new();

    private void Awake()
    {
        onScreenPos = panelRect.anchoredPosition;
        offScreenPos = new Vector2(Screen.width, panelRect.anchoredPosition.y);

        panelRect.anchoredPosition = offScreenPos;

        InitializeStatSlotDic();
    }

    private void Start()
    {
        uiCharacterSetting = UIManager.Instance.GetUIComponent<UICharacterSetting>();
    }

    private void InitializeStatSlotDic()
    {
        foreach (StatSlot slot in statSlots)
        {
            if (!statSlotDic.TryAdd(slot.StatType, slot))
            {
                Debug.LogWarning($"Duplicate StatSlot for type: {slot.StatType}");
            }
        }
    }

    private void SetCharacterStatInfo()
    {
        int                                      level          = selectedPlayerUnitData.Level;
        List<StatData>                           charBaseStats  = selectedPlayerUnitData.CharacterSo.Stats;
        List<StatData>                           statGrowthList = statIncreaseSo.Stats;
        Dictionary<EquipmentType, EquipmentItem> equippedItems  = selectedPlayerUnitData.EquippedItems;

        Dictionary<StatType, float> baseStats    = new();
        Dictionary<StatType, float> levelUpStats = new();
        Dictionary<StatType, float> equipStats   = new();

        // 1. 기본 스탯 + 레벨 증가 계산
        foreach (StatData stat in charBaseStats)
        {
            StatType statType = stat.StatType;
            baseStats[statType] = stat.Value;

            StatData growth = statGrowthList.Find(s => s.StatType == statType);
            if (growth != null && level > 1)
            {
                levelUpStats[statType] = growth.Value * (level - 1);
            }
            else
            {
                levelUpStats[statType] = 0;
            }
        }

        // 2. 장비 스탯 누적
        foreach (EquipmentItem equipment in equippedItems.Values)
        {
            foreach (StatData equipStat in equipment.EquipmentItemSo.Stats)
            {
                equipStats.TryAdd(equipStat.StatType, 0);

                equipStats[equipStat.StatType] += equipStat.Value;
            }
        }

        if (statSlotDic.Count == 0)
        {
            InitializeStatSlotDic();
        }

        // 3. StatSlot UI 초기화
        foreach (StatType statType in statSlotDic.Keys)
        {
            float baseValue  = baseStats.GetValueOrDefault(statType, 0);
            float levelValue = levelUpStats.GetValueOrDefault(statType, 0);
            float equipValue = equipStats.GetValueOrDefault(statType, 0);

            statSlotDic[statType].Initialize(baseValue + levelValue, equipValue);
        }
    }

    private void SetPlayerUnitEquipmentInfo()
    {
        Dictionary<EquipmentType, EquipmentItem> equipItem = selectedPlayerUnitData.EquippedItems;
        for (int i = 0; i < equippedItemSlot.Length; i++)
        {
            if (equipItem.TryGetValue((EquipmentType)i, out EquipmentItem item))
            {
                equippedItemSlot[i].Initialize(item, false);
            }
            else
            {
                equippedItemSlot[i].Initialize(null, false);
            }

            equippedItemSlot[i].ShowEquipMark(false);
        }
    }

    private void SetPlayerUnitSkillInfo()
    {
        PassiveSO   passiveSkill = selectedPlayerUnitData.CharacterSo.PassiveSkill;
        SkillData[] activeSkills = selectedPlayerUnitData.SkillDatas;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            // 패시브 스킬은 첫 번째 슬롯에
            if (i == 0)
            {
                skillSlots[i].SetSkillIcon(passiveSkill, false);
            }
            // 그 다음은 액티브 스킬
            else if (i - 1 < activeSkills.Length)
            {
                skillSlots[i].SetSkillIcon(activeSkills[i - 1], false);
            }

            skillSlots[i].ShowEquipMark(false);
        }
    }

    private void UpdateUnitLevel()
    {
        int          level  = selectedPlayerUnitData.Level;
        PlayerUnitSO unitSo = selectedPlayerUnitData.CharacterSo;

        unitLevel.text = $"Lv. {level}";

        foreach (StatData statData in statIncreaseSo.Stats)
        {
            float baseValue      = unitSo.GetStat(statData.StatType)?.Value ?? 0;
            float increasedValue = baseValue + (statData.Value * (level - 1));

            UpdateLevelUpStatValue(statData.StatType, increasedValue);
        }
    }

    public void OpenPanel(EntryDeckData unitData)
    {
        if (selectedPlayerUnitData != null)
        {
            selectedPlayerUnitData.OnEquipmmmentChanged -= RefreshUI;
            selectedPlayerUnitData.OnSkillChanged -= SetPlayerUnitSkillInfo;
        }

        selectedPlayerUnitData = unitData;
        selectedPlayerUnitData.OnEquipmmmentChanged += RefreshUI;
        selectedPlayerUnitData.OnSkillChanged += SetPlayerUnitSkillInfo;

        unitName.text = selectedPlayerUnitData.CharacterSo.UnitName;

        RefreshUI();
        SetPlayerUnitSkillInfo();
        panelRect.DOKill();
        panelRect.gameObject.SetActive(true);
        panelRect.DOAnchorPos(onScreenPos, 0.5f).SetEase(Ease.OutCubic);
    }

    public void ClosePanel()
    {
        if (selectedPlayerUnitData != null)
        {
            selectedPlayerUnitData.OnEquipmmmentChanged -= RefreshUI;
            selectedPlayerUnitData.OnSkillChanged -= SetPlayerUnitSkillInfo;
            unitLevelUpPanel.ClosePanel();
        }

        panelRect.DOKill();
        panelRect.DOAnchorPos(offScreenPos, 0.5f).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            panelRect.anchoredPosition = offScreenPos;
            panelRect.gameObject.SetActive(false);
        });
        selectedPlayerUnitData = null;
    }

    private void RefreshUI()
    {
        SetCharacterStatInfo();
        UpdateUnitLevel();
        SetPlayerUnitEquipmentInfo();
    }


    public void OpenUnitLevelUpPanel()
    {
        if (selectedPlayerUnitData != null)
        {
            selectedPlayerUnitData.OnLevelUp -= UpdateUnitLevel;
        }

        selectedPlayerUnitData.OnLevelUp += UpdateUnitLevel;
        unitLevelUpPanel.OpenPanel(selectedPlayerUnitData);
    }


    private void UpdateLevelUpStatValue(StatType statType, float value)
    {
        if (statSlotDic.TryGetValue(statType, out StatSlot statSlot))
        {
            statSlot.UpdateStatValue(value);
        }
    }
}