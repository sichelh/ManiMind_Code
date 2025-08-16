using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

public class UnitSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText; // 캐릭터 이름
    [SerializeField] private TextMeshProUGUI unitLevel;
    [SerializeField] private Image unitSlotFrame;
    [SerializeField] private List<GameObject> unitTierStar;
    [SerializeField] private GameObject competedMarker;
    [SerializeField] private Image iconImage; // 캐릭터 이미지
    [SerializeField] private GameObject selectedNotiImg;
    [SerializeField] private Image holdCheckImage;
    [SerializeField] private List<Sprite> unitGradeSprites;
    [SerializeField] private Image jobTypeImage;
    [SerializeField] private List<Sprite> jobTypeSprites;

    private bool isSelected;
    private EntryDeckData selectedUnit;

    // 버튼에 데이터를 집어넣는 초기화 작업

    public event Action<EntryDeckData> OnClicked;
    public event Action<EntryDeckData> OnHeld;

    private UIDeckBuilding UIDeckBuilding => UIManager.Instance.GetUIComponent<UIDeckBuilding>();


    private bool isDoubleClickSlot = true;
    private bool isHoldSlot = false;

    private UIHoldDetector holdDetector;

    private void Awake()
    {
        holdDetector = GetComponent<UIHoldDetector>();
    }

    private void Start()
    {
        if (holdDetector != null && isHoldSlot)
        {
            holdDetector.OnHoldTriggered += () =>
            {
                OnHeld?.Invoke(selectedUnit);
            };
        }
    }

    public void Initialize(EntryDeckData data)
    {
        selectedUnit = data;
        PlayerUnitSO characterSo = data.CharacterSo;

        // UI 이미지와 텍스트 교체
        iconImage.sprite = characterSo.UnitIcon;
        nameText.text = characterSo.UnitName;
        unitLevel.text = $"Lv.{data.Level}";
        unitSlotFrame.sprite = unitGradeSprites[(int)characterSo.Tier];
        jobTypeImage.sprite = jobTypeSprites[(int)characterSo.JobType];

        gameObject.name = $"UnitSlot_{characterSo.ID}";

        SetCompetedMarker(data.CompeteSlotInfo.IsInDeck);
        for (int i = 0; i < unitTierStar.Count; i++)
        {
            unitTierStar[i].SetActive(i <= (int)characterSo.Tier);
        }
    }

    public void SetCompetedMarker(bool isCompeted)
    {
        if (isCompeted != competedMarker.activeSelf)
        {
            competedMarker.SetActive(isCompeted);
        }
    }

    public void SetSelectedMarker(bool isSelect)
    {
        if (isSelect != selectedNotiImg.activeSelf)
        {
            selectedNotiImg.SetActive(isSelect);
        }
    }

    public void Deselect()
    {
        isSelected = false;
        SetSelectedMarker(false);
    }

    public void SetDoubleClicked(bool isDoubleClicked = true)
    {
        isDoubleClickSlot = isDoubleClicked;
    }

    public void SetHoldSlot(bool isHold)
    {
        isHoldSlot = isHold;
    }

    public void HandleClick()
    {
        AudioManager.Instance.PlaySFX(nameof(SFXName.SelectedUISound));

        if (isDoubleClickSlot)
        {
            if (!isSelected)
            {
                isSelected = true;
                UIDeckBuilding.SetSelectedUnitSlot(this);
            }
            else
            {
                OnClicked?.Invoke(selectedUnit);
            }
        }
        else
        {
            isSelected = true;
            UIDeckBuilding.SetSelectedUnitSlot(this);
            OnClicked?.Invoke(selectedUnit);
        }

        SetCompetedMarker(selectedUnit.CompeteSlotInfo.IsInDeck);
        SetSelectedMarker(isSelected);
    }
}