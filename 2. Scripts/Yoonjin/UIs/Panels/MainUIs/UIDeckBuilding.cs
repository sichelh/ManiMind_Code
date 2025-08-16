using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIDeckBuilding : UIBase
{
    [Header("보유한 전체 캐릭터 영역")]
    [SerializeField] private Transform ownedCharacterParent;

    [SerializeField] private RectTransform ownedCharacterRect;

    [SerializeField] private List<CompeteUnitSlot> competedUnitSlots;
    [SerializeField] private ScrollRect scrollRect;

    [FormerlySerializedAs("characterButtonPrefab")]
    [SerializeField] private UnitSlot unitSlotPrefab;

    [Header("UnitInfoPanel")]
    [SerializeField] private PanelSelectedUnitInfo unitInfoPanel;

    // 보유 캐릭터 & 선택 캐릭터 SO들을 담는 리스트
    private Dictionary<int, UnitSlot> characterSlotDic = new();
    private Vector2 originalPos;

    private UnitSlot selectedUnitSlot;
    private AvatarPreviewManager avatarPreviewManager => AvatarPreviewManager.Instance;

    private void Awake()
    {
        originalPos = ownedCharacterRect.anchoredPosition;
    }

    // 현재 보유 중인 캐릭터 목록 버튼 생성
    private void GenerateHasUnitSlots()
    {
        Dictionary<int, EntryDeckData> units = AccountManager.Instance.MyPlayerUnits;

        foreach (KeyValuePair<int, EntryDeckData> entryDeckData in units)
        {
            if (characterSlotDic.ContainsKey(entryDeckData.Key))
            {
                continue;
            }

            UnitSlot slot = Instantiate(unitSlotPrefab, ownedCharacterParent);
            slot.Initialize(entryDeckData.Value);
            characterSlotDic.Add(entryDeckData.Key, slot);
            slot.SetDoubleClicked(false);
            slot.SetHoldSlot(true);
            slot.OnClicked += OnClickedHasUnitSlot;
            slot.OnHeld += OnHeldHasUnitSlot;
        }
    }

    // 선택된 캐릭터 목록
    private void ShowCompetedUnit(List<EntryDeckData> selectedDeck)
    {
        int index = 0;
        foreach (EntryDeckData entry in selectedDeck)
        {
            if (entry == null)
            {
                index++;
                continue;
            }

            competedUnitSlots[index].SetCompeteUnitData(entry);
            avatarPreviewManager.ShowAvatar(index++, entry.CharacterSo);
        }
    }

    // 보유 캐릭터 버튼 클릭 처리
    private void OnClickedHasUnitSlot(EntryDeckData data)
    {
        if (!data.CompeteSlotInfo.IsInDeck)
        {
            DeckSelectManager.Instance.AddUnitInDeck(data, out int index);
            if (index == -1)
            {
                return;
            }

            competedUnitSlots[index].SetCompeteUnitData(data);
            avatarPreviewManager.ShowAvatar(index, data.CharacterSo);
        }
        else
        {
            characterSlotDic[data.CharacterSo.ID].SetSelectedMarker(false);
        }
    }

    private void OnHeldHasUnitSlot(EntryDeckData data)
    {
        unitInfoPanel.SetInfoPanel(data);
        unitInfoPanel.OpenPanel();
    }

    public void RemoveUnitInDeck(EntryDeckData data)
    {
        characterSlotDic[data.CharacterSo.ID].SetCompetedMarker(false);
        DeckSelectManager.Instance.RemoveUnitInDeck(data);
        avatarPreviewManager.HideAvatar(data.CharacterSo);
    }

    public void SetSelectedUnitSlot(UnitSlot slot)
    {
        if (slot != selectedUnitSlot)
        {
            selectedUnitSlot?.Deselect();
        }

        selectedUnitSlot = slot;
        selectedUnitSlot.SetSelectedMarker(true);
    }

    public override void Open()
    {
        base.Open();
        avatarPreviewManager.ShowOrHideDeckCamera(true);
        GenerateHasUnitSlots();
        ShowCompetedUnit(DeckSelectManager.Instance.GetSelectedDeck());

        ownedCharacterRect.DOKill();
        ownedCharacterRect.DOAnchorPos(originalPos, 0.3f).From(originalPos + (Vector2.down * 500f)).SetEase(Ease.OutBack);
    }

    public override void Close()
    {
        scrollRect.horizontalNormalizedPosition = 0;
        base.Close();

        foreach (UnitSlot slot in characterSlotDic.Values)
        {
            slot.Deselect();
        }

        unitInfoPanel.ClosePanel();
        avatarPreviewManager.HideAllBuilindUIAvatars();
        selectedUnitSlot = null;
    }
}