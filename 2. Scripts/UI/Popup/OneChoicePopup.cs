using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OneChoicePopup : UIBase
{
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private TextMeshProUGUI descTxt;
    [SerializeField] private TextMeshProUGUI centerBtnTxt;

    [SerializeField] private CanvasGroup BG;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;

    [SerializeField] private Button exitButton;
    [SerializeField] private EventTrigger backgroundTrigger;

    public event Action OnCenterClicked;


    private void Start()
    {
        BG.gameObject.SetActive(false);
    }

    public void SetAndOpenPopupUI(string title, string desc, Action centerAct = null, string centerTxt = "확인")
    {
        titleTxt.text = title;
        descTxt.text = desc;
        centerBtnTxt.text = centerTxt;
        SetCenterButtonAction(centerAct);
        UIManager.Open(this, false);
    }

    public void ClickCenterButton()
    {
        OnCenterClicked?.Invoke();
        UIManager.Close(this);
    }

    public void ToggleActiveExitBtn(bool isActive) // 전투씬에서는 패배 팝업 닫기버튼 없어야 함
    {
        exitButton.gameObject.SetActive(isActive);
        backgroundTrigger.enabled = isActive;
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
        seq.AppendCallback(() => UIManager.Instance.Close(PopupManager.Instance.GetUIComponent<UIDefeat>()));

        OnCenterClicked = null;
    }

    private void SetCenterButtonAction(Action action)
    {
        action += () => { UIManager.Close(this); };
        OnCenterClicked += action;
    }
}