using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class ToastMessageUI : UIBase
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI toastMassage;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float showDuration = 1.5f;

    private Sequence fadeTween;

    private void Awake()
    {
        canvasGroup.alpha = 0;
    }

    public override void Open()
    {
        base.Open();
        ShowToastMessage();
    }

    public override void Close()
    {
        base.Close();
        fadeTween?.Kill();
    }

    public void SetToastMessage(string message)
    {
        toastMassage.text = message;
        this.Open();
    }

    private void ShowToastMessage()
    {
        fadeTween?.Kill();
        canvasGroup.alpha = 0f;

        fadeTween = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, fadeDuration)).SetEase(Ease.OutQuad)
            .AppendInterval(showDuration)
            .Append(canvasGroup.DOFade(0f, fadeDuration)).SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                UIManager.Close(this);
                toastMassage.text = "";
            });
    }
}