using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReward : UIBase
{
    [SerializeField] private List<InventorySlot> inventorySlotList;

    [SerializeField] private RectTransform bgTransform;
    [SerializeField] private CanvasGroup bgCanvasGroup;
    [SerializeField] private RectTransform titleTransform;
    [SerializeField] private CanvasGroup titleCanvasGroup;

    private Action afterAction;
    private int index = 0;

    private Sequence rewardSequence;
    private Vector2 originalPos;
    private bool initialized = false;

    public void OpenRewardUI(Action action)
    {
        for (int i = index; i < inventorySlotList.Count; i++)
        {
            inventorySlotList[i].gameObject.SetActive(false);
        }

        afterAction = action;

        UIManager.Instance.Open(this, false);
    }

    public void AddReward(RewardSo rewardSo)
    {
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

        rewardSequence.Append(bgTransform.DOAnchorPos(originalPos, 0.3f).From(originalPos + (Vector2.up * 500f)).SetEase(Ease.InOutSine));
        rewardSequence.Join(bgCanvasGroup.DOFade(1f, 0.3f));

        titleTransform.localScale = Vector3.one * 1.5f;
        rewardSequence.Append(titleTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
        rewardSequence.Join(titleCanvasGroup.DOFade(1f, 0.3f));

        foreach (RewardData rewardData in rewardSo.RewardList)
        {
            if (index >= inventorySlotList.Count)
            {
                break;
            }

            inventorySlotList[index].Initialize(rewardData);
            inventorySlotList[index].gameObject.SetActive(true);
            inventorySlotList[index].transform.localScale = Vector3.zero;
            rewardSequence.Append(inventorySlotList[index].transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
                .SetDelay(index * 0.01f));
            index++;
        }
    }

    public void CloseRewardUI()
    {
        UIManager.Instance.Close(this);
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
        index = 0;
        afterAction?.Invoke();
    }
}