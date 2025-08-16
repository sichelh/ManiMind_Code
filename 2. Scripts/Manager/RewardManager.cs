using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : Singleton<RewardManager>
{
    private RewardTable rewardTable;


    private readonly Dictionary<RewardType, Action<RewardData>> rewardHandlers = new()
    {
        { RewardType.Gold, reward => AccountManager.Instance.AddGold(reward.Amount) },
        { RewardType.Opal, reward => AccountManager.Instance.AddOpal(reward.Amount) },
        {
            RewardType.Item, reward =>
            {
                ItemSO item = TableManager.Instance.GetTable<ItemTable>().GetDataByID(reward.ItemId);
                if (item == null)
                {
                    return;
                }

                if (item is EquipmentItemSO equipmentItem)
                {
                    InventoryManager.Instance.AddItem(new EquipmentItem(equipmentItem), reward.Amount);
                }
            }
        },
        {
            RewardType.Skill, reward =>
            {
                ActiveSkillSO skill = TableManager.Instance.GetTable<ActiveSkillTable>().GetDataByID(reward.ItemId);
                if (skill == null)
                {
                    return;
                }

                AccountManager.Instance.AddSkill(skill, out bool isDuplicate);
            }
        },
        {
            RewardType.Unit, reward =>
            {
                PlayerUnitSO unit = TableManager.Instance.GetTable<PlayerUnitTable>().GetDataByID(reward.ItemId);
                if (unit == null)
                {
                    return;
                }

                AccountManager.Instance.AddPlayerUnit(unit, reward.Amount);
            }
        }
    };

    protected override void Awake()
    {
        base.Awake();
        if (isDuplicated)
            return;
        rewardTable = TableManager.Instance.GetTable<RewardTable>();
    }

    public void GiveReward(string id)
    {
        RewardSo rewardSo = rewardTable.GetDataByID(id);
        if (rewardSo == null)
            return;

        foreach (RewardData reward in rewardSo.RewardList)
        {
            if (rewardHandlers.TryGetValue(reward.RewardType, out Action<RewardData> handler))
            {
                handler.Invoke(reward);
            }
        }
    }

    public void AddReward(string id)
    {
        RewardSo rewardSo = rewardTable.GetDataByID(id);
        if (rewardSo == null)
            return;

        PopupManager.Instance.GetUIComponent<UIReward>().AddReward(rewardSo);
        GiveReward(id);
    }

    /// <summary>
    /// 여러곳에서 보상을 추가 후 마지막에 호출하여 보상 UI를 Open
    /// </summary>
    /// <param name="onComplete"></param>
    public void GiveRewardAndOpenUI(Action onComplete = null)
    {
        PopupManager.Instance.GetUIComponent<UIReward>().OpenRewardUI(onComplete);
    }
}