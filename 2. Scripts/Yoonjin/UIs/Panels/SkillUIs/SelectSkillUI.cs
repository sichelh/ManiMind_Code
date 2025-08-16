using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectSkillUI : UIBase
{
    [Header("장착한 스킬 이름 / 설명 / 효과")]
    [SerializeField]
    private RectTransform panelRect;

    [SerializeField]
    private TextMeshProUGUI skillName;

    [SerializeField]
    private TextMeshProUGUI skillDescription;

    [SerializeField]
    private TextMeshProUGUI skillCooltimeTxt;

    [SerializeField]
    private TextMeshProUGUI skillTotalUseCountTxt;

    [SerializeField]
    private GameObject skillEffect;


    [Header("스킬 슬롯")]
    [SerializeField]
    private List<SkillSlot> activeSkillSlots = new();

    [SerializeField]
    private SkillSlot passiveSkillSlot;

    [Header("스킬 버튼 프리팹")]
    [SerializeField]
    private SkillSlot skillSlotPrefab;


    [Header("인벤토리")]
    [SerializeField]
    private EquipmentSkillInventory inventoryUI;

    [Header("캐릭터 이름")]
    [SerializeField]
    private TextMeshProUGUI chaName;


    private SkillSlot selectedSkillSlot;
    private DeckSelectManager DeckSelectManager => DeckSelectManager.Instance;

    private AvatarPreviewManager AvatarPreviewManager => AvatarPreviewManager.Instance;

    // 현재 선택된 캐릭터
    public EntryDeckData CurrentCharacter { get; private set; }


    public event Action<EntryDeckData> OnSkillChanged;
    private Action<InventorySlot> onClickCallback;

    private Vector2 onScreenPos;
    private Vector2 offScreenPos;

    private void Awake()
    {
        CloseInfo();
        onScreenPos = panelRect.anchoredPosition;
        offScreenPos = new Vector2(Screen.width, panelRect.anchoredPosition.y);

        panelRect.anchoredPosition = offScreenPos;
    }

    private void HandleEquipItemChanged(EntryDeckData unit, SkillData newSkill, SkillData oldSkill)
    {
        if (unit != CurrentCharacter)
        {
            return;
        }

        if (oldSkill != null)
        {
            inventoryUI.RefreshAtSlotUI(oldSkill);
        }

        if (newSkill != null)
        {
            inventoryUI.RefreshAtSlotUI(newSkill);
        }
    }

    private void RefreshEquipSkillUI()
    {
        if (CurrentCharacter == null)
        {
            return;
        }

        // ClearEquipInfo();
        RefreshEquippedSkillSlots();

        List<SkillData> inventoryItems = AccountManager.Instance.GetInventorySkillsByJob(CurrentCharacter.CharacterSo.JobType);
        inventoryUI.Initialize(
            () => inventoryItems,
            (slot) =>
            {
                slot.OnClickSlot -= OnClickInventorySlot;
                slot.OnClickSlot += OnClickInventorySlot;
            });
    }

    private void OnClickInventorySlot(SkillData skill)
    {
        OpenInfo();

        inventoryUI.SelectItemSlot(skill);
        SkillSlot selectSlot = inventoryUI.GetSlotByItem(skill);
        if (selectSlot == null)
        {
            return;
        }


        if (selectedSkillSlot != selectSlot)
        {
            SetSkillInfoUI(skill.skillSo);
            selectedSkillSlot = selectSlot;
        }
        else
        {
            if (skill.IsEquipped && skill.EquippedUnit != CurrentCharacter)
            {
                Action leftAction = () =>
                {
                    DeckSelectManager.ForceEquipSkillToCurrentCharacter(skill);
                    RefreshEquippedSkillSlots();
                };
                string equippedUnitName = skill.EquippedUnit.CharacterSo.UnitName;
                string skillName        = skill.skillSo.skillName;
                string message          = $"{skillName}은(는) {equippedUnitName}이(가) 장착 중입니다.\n해제 후 장착하시겠습니까?";
                PopupManager.Instance.GetUIComponent<TwoChoicePopup>()?.SetAndOpenPopupUI("스킬 장착", message, leftAction, null, "장착", "취소");
            }
            else
            {
                DeckSelectManager.ProcessEquipSkillSelection(skill);
            }
        }

        RefreshEquippedSkillSlots();
    }

    private void OnClickPassiveSkillSlot(SkillData skill)
    {
        OnClickPassiveSkillSlot(CurrentCharacter.CharacterSo.PassiveSkill);
    }

    private void OnClickPassiveSkillSlot(PassiveSO passiveSkill)
    {
        OpenInfo();
        SetSkillInfoUI(passiveSkill);
    }

    private void SetSkillInfoUI(SkillSo equipmentSkill)
    {
        skillName.text = equipmentSkill.skillName;
        skillDescription.text = equipmentSkill.skillDescription;
        if (equipmentSkill is ActiveSkillSO activeSkill)
        {
            skillEffect.SetActive(true);
            skillCooltimeTxt.text = $"{activeSkill.coolTime}";
            skillTotalUseCountTxt.text = $"{activeSkill.reuseMaxCount}";
        }
        else
        {
            skillEffect.SetActive(false);
        }
    }

    private void RefreshEquippedSkillSlots()
    {
        for (int i = 0; i < CurrentCharacter.SkillDatas.Length; i++)
        {
            SkillData skill = CurrentCharacter.SkillDatas[i];
            activeSkillSlots[i].SetSkillIcon(skill, false);
            activeSkillSlots[i].ShowEquipMark(false);
        }
    }


    public void SetCurrentSelectedUnit(EntryDeckData currentUnit)
    {
        DeckSelectManager.Instance.SetCurrentSelectedCharacter(currentUnit);
        CurrentCharacter = currentUnit;
        chaName.text = currentUnit.CharacterSo.UnitName;
    }

    public override void Open()
    {
        base.Open();

        if (CurrentCharacter == null)
        {
            return;
        }

        passiveSkillSlot.SetSkillIcon(CurrentCharacter.CharacterSo.PassiveSkill, false);
        passiveSkillSlot.OnClickSlot += OnClickPassiveSkillSlot;

        RefreshEquipSkillUI();
        DeckSelectManager.Instance.OnEquipSkillChanged += HandleEquipItemChanged;
        AvatarPreviewManager.ShowAvatar(CurrentCharacter.CharacterSo);
    }

    public override void Close()
    {
        inventoryUI.ReuseScrollview.ResetScrollviewPosition();
        base.Close();
        if (CurrentCharacter != null)
        {
            if (CurrentCharacter.CompeteSlotInfo.IsInDeck)
            {
                int partyIndex = CurrentCharacter.CompeteSlotInfo.SlotIndex;
                if (partyIndex != -1)
                {
                    AvatarPreviewManager.ShowAvatar(partyIndex, CurrentCharacter.CharacterSo);
                }
            }
            else
            {
                AvatarPreviewManager.HideAvatar(CurrentCharacter.CharacterSo);
            }
        }

        passiveSkillSlot.OnClickSlot -= OnClickPassiveSkillSlot;
        DeckSelectManager.Instance.OnEquipSkillChanged -= HandleEquipItemChanged;
        OnSkillChanged?.Invoke(CurrentCharacter);
        CloseInfo();
    }

    private void OpenInfo()
    {
        panelRect.DOKill();
        panelRect.DOAnchorPos(onScreenPos, 0.5f).SetEase(Ease.OutCubic);
    }

    private void CloseInfo()
    {
        panelRect.DOKill();
        panelRect.DOAnchorPos(offScreenPos, 0.5f).SetEase(Ease.OutCubic);
    }
}