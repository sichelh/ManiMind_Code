using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GachaDrawButton : MonoBehaviour
{
    private Button drawButton;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration;

    private IGachaHandler gachaHandler;
    private int drawCount;
    private Vector2 originalPos;
    private bool initialized = false;

    private void Awake()
    {
        drawButton = this.GetComponent<Button>();
        rectTransform = this.GetComponent<RectTransform>();
        canvasGroup = this.GetComponent<CanvasGroup>();
    }

    public void Initialize(IGachaHandler handler, int count)
    {
        gachaHandler = handler;
        drawCount = count;

        drawButton.onClick.RemoveAllListeners();
        drawButton.onClick.AddListener(OnDrawClicked);
    }

    // drawCount회 뽑기 버튼
    private void OnDrawClicked()
    {
        if (!gachaHandler.CanDraw(drawCount))
        {
            AudioManager.Instance.PlaySFX(SFXName.RejectUISound.ToString());
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("공명석이 부족합니다!");
            return;
        }
        
        string gachaTypeName = gachaHandler.GetGachaTypeName();
        string message = $"{drawCount}회 {gachaTypeName}을 진행하시겠습니까?\n 소모 공명석 : {gachaHandler.GetTotalCost(drawCount)}";
        Action leftAction = () => gachaHandler.DrawAndDisplayResult(drawCount);

        PopupManager.Instance.GetUIComponent<TwoChoicePopup>()?.SetAndOpenPopupUI
            (gachaTypeName, message, leftAction, null, "소환", "취소");
    }

    public void ShowButton()
    {
        // 초기 위치 저장
        if (!initialized)
        {
            originalPos = rectTransform.anchoredPosition;
            initialized = true;
        }

        rectTransform.DOKill();
        canvasGroup.DOKill();

        this.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOAnchorPos(originalPos, 0.4f).From(originalPos + Vector2.down * 200f).SetEase(Ease.OutBack));
        sequence.Join(canvasGroup.DOFade(1f, fadeInDuration));
    }

    public void HideButton()
    {
        rectTransform.DOKill();
        canvasGroup.DOKill();
        this.gameObject.SetActive(false);
    }
}
