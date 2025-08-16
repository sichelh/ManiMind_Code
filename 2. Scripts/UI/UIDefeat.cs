using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDefeat : UIBase
{
    [SerializeField] private RectTransform bgTransform;
    [SerializeField] private CanvasGroup bgCanvasGroup;
    [SerializeField] private RectTransform titleTransform;
    [SerializeField] private CanvasGroup titleCanvasGroup;

    private Sequence rewardSequence;
    private Vector2 originalPos;
    private bool initialized = false;

    public override void Open()
    {
        base.Open();

        // 초기 위치 저장
        if (!initialized)
        {
            originalPos = bgTransform.anchoredPosition;
            initialized = true;
        }

        bgTransform.DOKill();
        bgCanvasGroup.DOKill();
        titleTransform.DOKill();
        titleCanvasGroup.DOKill();

        bgCanvasGroup.alpha = 0f;
        titleCanvasGroup.alpha = 0f;

        rewardSequence = DOTween.Sequence();

        rewardSequence.Append(bgTransform.DOAnchorPos(originalPos, 0.3f).From(originalPos + Vector2.up * 500f).SetEase(Ease.InOutSine));
        rewardSequence.Join(bgCanvasGroup.DOFade(1f, 0.3f));

        titleTransform.localScale = Vector3.one * 1.5f;
        rewardSequence.Append(titleTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
        rewardSequence.Join(titleCanvasGroup.DOFade(1f, 0.3f));

        rewardSequence.AppendInterval(0.5f);
        rewardSequence.OnComplete(() => openChoicePopup());
    }

    private void openChoicePopup()
    {
        OneChoicePopup popup = PopupManager.Instance.GetUIComponent<OneChoicePopup>();
        popup.ToggleActiveExitBtn(false);
        popup.SetAndOpenPopupUI("전투 패배",
            "전투에 패배했습니다.",
            () =>
            {
                LoadSceneManager.Instance.LoadScene("DeckBuildingScene", () => UIManager.Instance.Open(UIManager.Instance.GetUIComponent<UIStageSelect>()));
            },
            "나가기");
    }
}
