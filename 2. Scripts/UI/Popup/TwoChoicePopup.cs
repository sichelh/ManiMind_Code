using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TwoChoicePopup : UIBase
{
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private TextMeshProUGUI descTxt;

    [SerializeField] private TextMeshProUGUI leftBtnTxt;
    [SerializeField] private TextMeshProUGUI rightBtnTxt;

    [SerializeField] private CanvasGroup BG;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;

    public event Action OnLeftClicked;
    public event Action OnRightClicked;

    private void Start()
    {
        BG.gameObject.SetActive(false);
    }

    public void SetAndOpenPopupUI(string title, string desc, Action leftAct, Action rightAct = null, string leftBtnTxt = "확인", string rightBtnTxt = "닫기")
    {
        titleTxt.text = title;
        descTxt.text = desc;
        this.leftBtnTxt.text = leftBtnTxt;
        this.rightBtnTxt.text = rightBtnTxt;
        SetLeftButtonAction(leftAct);
        SetRightButtonAction(rightAct);
        UIManager.Open(this, false);
    }

    public void ClickLeftButton()
    {
        OnLeftClicked?.Invoke();
        UIManager.Close(this);
    }

    public void ClickRightButton()
    {
        OnRightClicked?.Invoke();
        UIManager.Close(this);
    }

    public override void Open() // 팝업 열 때 페이드인 추가
    {
        base.Open();
        BG.alpha = 0;
        BG.DOFade(1f, fadeInDuration).SetEase(Ease.InOutSine);
    }

    public override void Close() // 팝업 닫을 때 페이드아웃 추가
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(BG.DOFade(0f, fadeOutDuration).SetEase(Ease.OutSine));
        seq.AppendCallback(() => base.Close());

        OnLeftClicked = null;
        OnRightClicked = null;
    }

    private void SetLeftButtonAction(Action action)
    {
        OnLeftClicked += action;
    }

    private void SetRightButtonAction(Action action)
    {
        if (action == null)
        {
            action = () => { UIManager.Close(this); };
        }

        OnRightClicked += action;
    }
}