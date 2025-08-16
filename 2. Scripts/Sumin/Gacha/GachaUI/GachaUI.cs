using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUI : UIBase
{
    [Header("뽑기 버튼들")]
    [SerializeField] private GachaDrawButton drawButtonOne;

    [SerializeField] private GachaDrawButton drawButtonTen;
    [SerializeField] private TextMeshProUGUI drawOneCost;
    [SerializeField] private TextMeshProUGUI drawTenCost;

    [Header("캐릭터 가챠")]
    [SerializeField] private Button characterGachaButton;

    [SerializeField] private CharacterGachaSystem characterGachaSystem;
    [SerializeField] private CharacterGachaResultUI characterGachaResultUI;
    private CharacterGachaHandler characterHandler;

    [Header("스킬 가챠")]
    [SerializeField] private Button skillGachaButton;

    [SerializeField] private SkillGachaSystem skillGachaSystem;
    [SerializeField] private SkillGachaResultUI skillGachaResultUI;
    private SkillGachaHandler skillHandler;

    [Header("장비 가챠")]
    [SerializeField] private Button equipmentGachaButton;

    [SerializeField] private EquipmentGachaSystem equipmentGachaSystem;
    [SerializeField] private EquipmentGachaResultUI equipmentGachaResultUI;
    private EquipmentGachaHandler equipmentHandler;

    [Header("좌측 메뉴")]
    [SerializeField] private GameObject gachaMenu;

    [SerializeField] private float fadeInDuration;


    [SerializeField] private GachaBanner gachaBanner;
    private RectTransform gachaMenuRect;
    private CanvasGroup gachaMenuCanvasGroup;
    private Vector2 originalPos;
    private bool initialized = false;

    private void Start()
    {
        characterHandler = new CharacterGachaHandler(characterGachaSystem, characterGachaResultUI);
        skillHandler = new SkillGachaHandler(skillGachaSystem, skillGachaResultUI);
        equipmentHandler = new EquipmentGachaHandler(equipmentGachaSystem, equipmentGachaResultUI);

        gachaMenuRect = gachaMenu.GetComponent<RectTransform>();
        gachaMenuCanvasGroup = gachaMenu.GetComponent<CanvasGroup>();

        characterGachaButton.onClick.RemoveAllListeners();
        characterGachaButton.onClick.AddListener(OnCharacterGachaSelected);

        skillGachaButton.onClick.RemoveAllListeners();
        skillGachaButton.onClick.AddListener(OnSkillGachaSelected);

        equipmentGachaButton.onClick.RemoveAllListeners();
        equipmentGachaButton.onClick.AddListener(OnEquipmentGachaSelected);
    }

    public override void Open()
    {
        base.Open();
        ShowGachaMenu();
        // 처음 열었을땐 영웅 소환
        OnCharacterGachaSelected();
        ToggleActiveButtons(characterGachaButton);
    }

    private void ShowGachaMenu()
    {
        // 초기 위치 저장
        if (!initialized)
        {
            originalPos = gachaMenuRect.anchoredPosition;
            initialized = true;
        }

        gachaMenuRect.DOKill();
        gachaMenuCanvasGroup.DOKill();

        gameObject.SetActive(true);
        gachaMenuCanvasGroup.alpha = 0f;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(gachaMenuRect.DOAnchorPos(originalPos, 0.3f).From(originalPos + (Vector2.left * 200f)).SetEase(Ease.OutBack));
        sequence.Join(gachaMenuCanvasGroup.DOFade(1f, fadeInDuration));
    }

    // 버튼 토글
    private void ToggleActiveButtons(Button button)
    {
        characterGachaButton.interactable = true;
        skillGachaButton.interactable = true;
        equipmentGachaButton.interactable = true;
        button.interactable = false;
    }

    private void OnCharacterGachaSelected()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        SetGachaSelection(characterHandler, 0, characterGachaButton);
    }

    private void OnSkillGachaSelected()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        SetGachaSelection(skillHandler, 1, skillGachaButton);
    }

    private void OnEquipmentGachaSelected()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        SetGachaSelection(equipmentHandler, 2, equipmentGachaButton);
    }

    private void SetGachaSelection(IGachaHandler handler, int index, Button activeButton)
    {
        drawButtonOne.Initialize(handler, 1);
        drawButtonTen.Initialize(handler, 10);
        InitializeGachaTypeUI();
        gachaBanner.ShowBanner(index);
        ToggleActiveButtons(activeButton);
        drawOneCost.text = $"x {handler.GetTotalCost(1)}";
        drawTenCost.text = $"x {handler.GetTotalCost(10)}";
    }

    // 가챠 종류별 버튼 누를때마다 초기화
    private void InitializeGachaTypeUI()
    {
        ResetAllBanners();
        RefreshDrawButtons();
    }

    // 배너들 초기화
    private void ResetAllBanners()
    {
        gachaBanner.HideAllBanner();
        // characterGachaBannerUI.HideBanner();
        // skillGachaBannerUI.HideBanner();
        // equipmentGachaBannerUI.HideBanner();
    }

    // 버튼들 초기화
    private void RefreshDrawButtons()
    {
        drawButtonOne.HideButton();
        drawButtonTen.HideButton();
        drawButtonOne.ShowButton();
        drawButtonTen.ShowButton();
    }
}