using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSceneGameUI : MonoBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private GameObject speedBtnHighlight;
    [SerializeField] private GameObject playingImage;
    [SerializeField] private GameObject infoWindow;

    private BattleManager battleManager;
    private InputManager inputManager;
    private LoadingScreenController loadingScreenController;

    [Header("상단에 있는 턴 UI")]
    [SerializeField] private CanvasGroup TurnTopUI;

    [SerializeField] private TextMeshProUGUI turnText;

    [Header("두트윈으로 재생되는 턴 UI")]
    [SerializeField] private CanvasGroup TurnAniUI;

    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private RectTransform titleTextRect;
    [SerializeField] private TextMeshProUGUI turnAniText;
    [SerializeField] private TextMeshProUGUI turnDescriptionText;

    [SerializeField] private float slideDuration;
    [SerializeField] private float scaleDuration;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private float turnAniDuration;

    [Header("처음 전투 입장 시 UI 등장")]
    [SerializeField] private RectTransform monsterInfoUI;

    [SerializeField] private RectTransform playerInfoUI;
    [SerializeField] private RectTransform selectPhaseUI;
    [SerializeField] private RectTransform topButtons;

    private Vector2 originalPos;
    private bool initialized = false;
    private Sequence turnAniSequence;


    private void OnEnable()
    {
        battleManager = BattleManager.Instance;
        battleManager.OnTurnEnded += UpdateTurnCount;
        loadingScreenController = LoadingScreenController.Instance;
        loadingScreenController.OnLoadingComplete += WaitForLoading;
        inputManager = InputManager.Instance;
        speedBtnHighlight.SetActive(GameManager.Instance.TimeScaleMultiplier);
    }

    private void WaitForLoading()
    {
        PlayTurnIntroAnimation(false);
        PlayAniUIs();
    }

    // 처음 시작 시 UI Ani
    private void PlayAniUIs()
    {
        monsterInfoUI.DOKill();
        playerInfoUI.DOKill();
        topButtons.DOKill();

        turnAniSequence.Append(monsterInfoUI.DOAnchorPos(monsterInfoUI.anchoredPosition, 0.3f).From(monsterInfoUI.anchoredPosition + (Vector2.left * 500f)).SetEase(Ease.OutQuint));
        turnAniSequence.Join(topButtons.DOAnchorPos(topButtons.anchoredPosition, 0.3f).From(topButtons.anchoredPosition + (Vector2.up * 500f)).SetEase(Ease.OutQuint));
        turnAniSequence.Join(playerInfoUI.DOAnchorPos(playerInfoUI.anchoredPosition, 0.5f).From(playerInfoUI.anchoredPosition + (Vector2.down * 500f)).SetEase(Ease.OutQuint));
        turnAniSequence.Join(selectPhaseUI.DOAnchorPos(selectPhaseUI.anchoredPosition, 0.6f).From(selectPhaseUI.anchoredPosition + (Vector2.down * 500f)).SetEase(Ease.OutQuint));
    }

    // 턴 UI 애니메이션
    public void PlayTurnIntroAnimation(bool isBattleStart)
    {
        AudioManager.Instance.PlaySFX(SFXName.BattleStartUISound.ToString());
        // 이전 시퀀스 정리
        if (turnAniSequence != null && turnAniSequence.IsActive())
        {
            turnAniSequence.Kill(true); // 즉시 종료 (true면 콜백도 실행됨)
        }

        // 초기 위치 저장
        if (!initialized)
        {
            originalPos = titleTextRect.anchoredPosition;
            initialized = true;
        }

        turnDescriptionText.text = isBattleStart ? "전투 시작" : "전략 선택";

        titleTextRect.DOKill();
        TurnAniUI.DOKill();
        selectPhaseUI.DOKill();

        // 초기 설정
        TurnTopUI.gameObject.SetActive(false);
        TurnAniUI.gameObject.SetActive(true);
        TurnTopUI.alpha = 0f;
        TurnAniUI.alpha = 0f;

        // 새로운 시퀀스 생성 및 저장
        turnAniSequence = DOTween.Sequence();

        turnAniSequence.Append(TurnAniUI.DOFade(1f, fadeInDuration).SetEase(Ease.InOutSine));

        turnAniSequence.Join(titleTextRect.DOAnchorPosY(originalPos.y, slideDuration).From(originalPos + (Vector2.down * 200f)).SetEase(Ease.OutCubic));

        backgroundRect.localScale = Vector3.one * 2.3f;
        turnAniSequence.Join(backgroundRect.DOScale(2f, scaleDuration).SetEase(Ease.OutBack).SetDelay(0.1f));

        turnAniSequence.AppendInterval(turnAniDuration);

        turnAniSequence.AppendCallback(() => TurnTopUI.gameObject.SetActive(true));

        turnAniSequence.Append(TurnAniUI.DOFade(0f, fadeOutDuration).SetEase(Ease.OutSine));
        turnAniSequence.Join(TurnTopUI.DOFade(1f, fadeInDuration).SetEase(Ease.InOutSine));

        turnAniSequence.AppendCallback(() => TurnAniUI.gameObject.SetActive(false));


        // 애니 재생완료된 후에 전투 시작
        if (isBattleStart)
        {
            inputManager.OnPlayMode();
            ToggleActiveStartBtn(false);
            turnAniSequence.AppendCallback(() => InputManager.Instance.OnClickTurnStartButton());
        }
    }

    public void ToggleActiveStartBtn(bool toggle)
    {
        startBtn.gameObject.SetActive(toggle);
        playingImage.SetActive(!toggle);
    }

    public void BattleEnd()
    {
        playingImage.SetActive(false);
    }

    public void ToggleInteractableStartButton(bool toggle)
    {
        startBtn.interactable = toggle;
    }

    public void OnStartButton()
    {
        PlayTurnIntroAnimation(true);
    }

    public void OnSettingButton()
    {
        PopupManager.Instance.GetUIComponent<SettingPopup>()?.Open();
    }

    public void OnSpeedUpButton()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        GameManager.Instance.ToggleTimeScale();
        speedBtnHighlight.SetActive(GameManager.Instance.TimeScaleMultiplier);
    }

    public void OnInfoButton()
    {
        AudioManager.Instance.PlaySFX(SFXName.OpenUISound.ToString());
        infoWindow.SetActive(true);
    }

    public void OffInfoWindow()
    {
        AudioManager.Instance.PlaySFX(SFXName.CloseUISound.ToString());
        infoWindow.SetActive(false);
    }

    public void OnExitButton()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsActive)
        {
            return;
        }

        string message = "전투를 중단하시겠습니까?";
        Action leftAction = () =>
        {
            LoadSceneManager.Instance.LoadScene("DeckBuildingScene", () => UIManager.Instance.Open(UIManager.Instance.GetUIComponent<UIStageSelect>()));
        };
        PopupManager.Instance.GetUIComponent<TwoChoicePopup>()?.SetAndOpenPopupUI("전투 중단", message, leftAction, null, "중단", "취소");
    }


    private void UpdateTurnCount()
    {
        string turn = $"Turn {battleManager.TurnCount}";
        turnText.text = turn;
        turnAniText.text = turn;
        PlayTurnIntroAnimation(false);
    }

    private void OnDisable()
    {
        if (battleManager != null)
        {
            battleManager.OnTurnEnded -= UpdateTurnCount;
        }

        if (loadingScreenController != null)
        {
            loadingScreenController.OnLoadingComplete -= WaitForLoading;
        }
    }
}