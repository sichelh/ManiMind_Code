using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIStageSelect : UIBase
{
    [SerializeField] private List<Sprite> stageBgSprites;
    [SerializeField] private Image stageBgImg;

    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform viewPort;
    [SerializeField] private StageInfoPanel stageInfoPanel;

    [SerializeField] private List<StageSlot> stageSlots;
    [SerializeField] private List<RectTransform> cloudList;

    [Header("ChapterName")]
    [SerializeField] private RectTransform currentChapterNameRect;

    [SerializeField] private TextMeshProUGUI currentChapterName;
    [SerializeField] private RectTransform nextChapterNameRect;
    [SerializeField] private TextMeshProUGUI nextChapterName;

    private Vector2 offScreenRight;
    private Vector2 center;
    private Vector2 offScreenLeft;

    private StageSO currentStage;

    private int currentChpaterIndex = 0;

    private StageTable StageTable => TableManager.Instance.GetTable<StageTable>();

    public void SetStageInfo(StageSO stage)
    {
        currentStage = stage;
        PlayerDeckContainer.Instance.SetStage(stage);
        stageInfoPanel.SetStageInfo(stage, DeckSelectManager.Instance.GetSelectedDeck());
        stageInfoPanel.OpenPanel();
    }

    public void OnClickEnterStage()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        bool isDeckEmpty = DeckSelectManager.Instance
            .GetSelectedDeck()
            .All(u => u == null);

        if (isDeckEmpty)
        {
            OnClickEditDeckButton(); // 덱이 비어 있으면 편집 창 오픈
            return;
        }

        AccountManager.Instance.UpdateLastChallengedStageId(currentStage.ID);
        LoadSceneManager.Instance.LoadScene("BattleScene_Main");
    }

    public void OnClickEditDeckButton()
    {
        UIDeckBuilding uiDeckBuilding = UIManager.Instance.GetUIComponent<UIDeckBuilding>();
        UIManager.Instance.Open(uiDeckBuilding);
    }

    public override void Open()
    {
        base.Open();
        InitializeCapterNameText();
        SetStageSlot();

        foreach (RectTransform rectTransform in cloudList)
        {
            StartCloudLoop(rectTransform);
        }
    }

    public override void Close()
    {
        base.Close();
        stageInfoPanel.ClosePanel();
        foreach (RectTransform rectTransform in cloudList)
        {
            rectTransform.DOKill();
        }
    }

    private void SetStageSlot()
    {
        int index = 0;
        foreach (StageSO dataDicValue in StageTable.GetStagesByChapter(currentChpaterIndex))
        {
            if (stageSlots.Count <= index)
            {
                break;
            }

            stageSlots[index++].Initialize(dataDicValue);
        }
    }

    private void StartCloudLoop(RectTransform rect)
    {
        rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y);
        MoveCloud(rect);
    }

    private void MoveCloud(RectTransform rect)
    {
        float moveSpeed = Random.Range(10f, 30f);
        float delayTime = Random.Range(0f, 4f);
        rect.DOKill();
        rect.DOAnchorPos(new Vector2(-(Screen.width + rect.rect.width), rect.anchoredPosition.y), moveSpeed)
            .SetEase(Ease.Linear)
            .SetDelay(delayTime).OnComplete(() =>
            {
                rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y);
                MoveCloud(rect); // 재귀 호출로 반복
            });
    }


    private void InitializeCapterNameText()
    {
        currentChpaterIndex = AccountManager.Instance.LastClearedStageId / 1000000;
        currentChapterName.text = Define.ChapterNameDictionary[currentChpaterIndex];
        float width = currentChapterNameRect.rect.width;
        center = Vector2.zero;
        offScreenLeft = new Vector2(-width, 0);
        offScreenRight = new Vector2(width, 0);
        nextChapterNameRect.anchoredPosition = offScreenRight;
    }

    private void SlideToNextText(string newText)
    {
        currentChapterNameRect.DOKill();
        nextChapterNameRect.DOKill();
        nextChapterName.text = newText;

        nextChapterNameRect.anchoredPosition = offScreenRight;

        currentChapterNameRect.DOAnchorPos(offScreenLeft, 0.5f).SetEase(Ease.InOutCubic);

        nextChapterNameRect.DOAnchorPos(center, 0.5f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            (currentChapterNameRect, nextChapterNameRect) = (nextChapterNameRect, currentChapterNameRect);

            (currentChapterName, nextChapterName) = (nextChapterName, currentChapterName);

            nextChapterNameRect.anchoredPosition = offScreenRight;
        });
    }

    private void SlideToPrevText(string newText)
    {
        currentChapterNameRect.DOKill();
        nextChapterNameRect.DOKill();
        nextChapterName.text = newText;

        nextChapterNameRect.anchoredPosition = offScreenLeft;

        currentChapterNameRect.DOAnchorPos(offScreenRight, 0.5f).SetEase(Ease.InOutCubic);

        nextChapterNameRect.DOAnchorPos(center, 0.5f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            (currentChapterNameRect, nextChapterNameRect) = (nextChapterNameRect, currentChapterNameRect);
            (currentChapterName, nextChapterName) = (nextChapterName, currentChapterName);

            nextChapterNameRect.anchoredPosition = offScreenLeft;
        });
    }

    public void OnClickedRightChapterButton()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        int nextIndex = currentChpaterIndex + 1;
        if (Define.ChapterNameDictionary.TryGetValue(nextIndex, out string chapterName))
        {
            currentChpaterIndex = nextIndex;
            SlideToNextText(chapterName);
            stageBgImg.sprite = stageBgSprites[currentChpaterIndex - 1];
            SetStageSlot();
        }
    }

    public void OnClickedLeftChapterButton()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        int prevIndex = currentChpaterIndex - 1;
        if (Define.ChapterNameDictionary.TryGetValue(prevIndex, out string chapterName))
        {
            currentChpaterIndex = prevIndex;
            SlideToPrevText(chapterName);
            stageBgImg.sprite = stageBgSprites[currentChpaterIndex - 1];
            SetStageSlot();
        }
    }
}