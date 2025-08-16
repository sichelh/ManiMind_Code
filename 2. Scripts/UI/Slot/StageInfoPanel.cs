using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class StageInfoPanel : MonoBehaviour
{
    
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private TextMeshProUGUI stageName;
    [SerializeField] private List<StagePanelMonsterSlot> spawnMonsters;
    [SerializeField] private List<StagePanelHeroSlot> competedHeroes;
    [SerializeField] private List<InventorySlot> rewardSlots;


    private Vector3 onScreenScale;
    private StageSO stageSo;

    private List<EntryDeckData> currentDeck;

    private void Awake()
    {
        onScreenScale = Vector3.one;
        panelRect.localScale = Vector3.zero;
        panelRect.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
    }


    public void OpenPanel()
    {
        panelRect.DOKill();
        panelRect.gameObject.SetActive(true);
        DeckSelectManager.Instance.OnChangedDeck += SetCompetedUnitSlot;
        panelRect.DOScale(onScreenScale, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            panelRect.localScale = onScreenScale;
        });
    }

    public void ClosePanel()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsActive)
            return;

        AudioManager.Instance.PlaySFX(SFXName.CloseUISound.ToString());
        panelRect.DOKill();
        panelRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            panelRect.gameObject.SetActive(false);
        });
        DeckSelectManager.Instance.OnChangedDeck -= SetCompetedUnitSlot;
    }

    public void SetCompetedUnitSlot(int index)
    {
        competedHeroes[index].SetHeroSlot(currentDeck[index]?.CharacterSo);
    }

    public void SetStageInfo(StageSO stage, List<EntryDeckData> selectedDeck)
    {
        stageSo = stage;
        currentDeck = selectedDeck;

        stageName.text = $"스테이지 {stage.ID/1000000}-{stage.ID % 100}";

        for (int i = 0; i < spawnMonsters.Count; i++)
        {
            if (stageSo.Monsters.Count > i)
            {
                spawnMonsters[i].SetMonsterSlot(stageSo.Monsters[i]);
            }
            else
            {
                spawnMonsters[i].EmptySlot();
            }
        }

        for (int i = 0; i < competedHeroes.Count; i++)
        {
            SetCompetedUnitSlot(i);
        }

        RewardSo firstClearReward = stageSo.FirstClearReward;

        string   rewardId    = $"{stageSo.ID}_Clear_Reward";
        RewardSo clearReward = TableManager.Instance.GetTable<RewardTable>().GetDataByID(rewardId);

        int index = 0;
        TrySetRewardSlot(firstClearReward.RewardList, ref index);
        TrySetRewardSlot(clearReward.RewardList, ref index);
        for (int i = index; i < rewardSlots.Count; i++)
        {
            rewardSlots[i].Initialize(null);
        }
    }

    private void TrySetRewardSlot(List<RewardData> rewardList, ref int index)
    {
        foreach (RewardData rewardData in rewardList)
        {
            if (index >= rewardSlots.Count)
                break;

            rewardSlots[index].Initialize(rewardData);
            index++;
        }
    }
}