using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AccountManager : Singleton<AccountManager>
{
    public int Gold               { get; private set; } = 0;
    public int Opal               { get; private set; } = 0;
    public int BestStage          { get; private set; } = 0;
    public int LastClearedStageId { get; private set; } = 0;

    public Dictionary<int, EntryDeckData> MyPlayerUnits { get; private set; } = new();
    public Dictionary<int, SkillData>     MySkills      { get; private set; } = new();
    public event Action<int>              OnGoldChanged;
    public event Action<int>              OnOpalChanged;


    private List<int> orderedStageIds;
    private Dictionary<JobType, List<int>> JobSkillInventory = new();

    protected override void Awake()
    {
        base.Awake();

        orderedStageIds = TableManager.Instance.GetTable<StageTable>().DataDic.Keys.OrderBy(id => id).ToList();
    }

    /// <summary>
    /// Gold를 로드해주는 메서드
    /// </summary>
    /// <param name="amount"></param>
    public void SetGold(int amount)
    {
        Gold = amount;
        OnGoldChanged?.Invoke(Gold);
        OnGoldChanged += Gold => SaveLoadManager.Instance.SaveModuleData(SaveModule.Gold);
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }


    public void UseGold(int amount, out bool result)
    {
        if (Gold < amount)
        {
            result = false;
            return;
        }

        Gold -= amount;
        result = true;
        OnGoldChanged?.Invoke(Gold);
    }

    /// <summary>
    /// Opal을 로드해주는 메서드
    /// </summary>
    /// <param name="amount"></param>
    public void SetOpal(int amount)
    {
        Opal = amount;
        OnOpalChanged?.Invoke(Opal);
        OnOpalChanged += Opal => SaveLoadManager.Instance.SaveModuleData(SaveModule.Opal);
    }

    public void AddOpal(int amount)
    {
        Opal += amount;

        OnOpalChanged?.Invoke(Opal);
    }

    public void UseOpal(int amount)
    {
        if (Opal < amount)
        {
            return;
        }

        Opal -= amount;
        OnOpalChanged?.Invoke(Opal);
    }

    // Opal 사용 가능한지 보고 UI에서 판단
    public bool CanUseOpal(int amount)
    {
        return Opal >= amount;
    }

    public void UpdateBestStage(StageSO stage)
    {
        if (BestStage < stage.ID)
        {
            BestStage = stage.ID;
            RewardManager.Instance.AddReward(stage.FirstClearReward.Id);
            SaveLoadManager.Instance.SaveModuleData(SaveModule.BestStage);
        }
    }

    public void UpdateLastChallengedStageId(int stageId)
    {
        if (LastClearedStageId != stageId)
        {
            LastClearedStageId = stageId;
            SaveLoadManager.Instance.SaveModuleData(SaveModule.CurrentStage);
        }
    }

    public void SetBestStage(int stage)
    {
        BestStage = stage;
    }

    public int GetNextStageId(int currentStageId)
    {
        int chapterId = currentStageId / 10000; // 예: 101
        int stageId   = currentStageId % 10000; // 예: 0101 ~ 0110
        int stageNum  = stageId % 100;          // 예: 01 ~ 10

        const int maxStagePerChapter = 10;

        if (stageNum < maxStagePerChapter)
        {
            // 같은 챕터에서 다음 스테이지로
            return currentStageId + 1;
        }
        else
        {
            // 다음 챕터의 첫 스테이지로 (챕터+1, 스테이지 0101)
            int nextChapterId = chapterId + 100;
            return (nextChapterId * 10000) + 101;
        }
    }

    public void AddPlayerUnit(PlayerUnitSO unit, int amount = 1)
    {
        if (!MyPlayerUnits.TryGetValue(unit.ID, out EntryDeckData data))
        {
            data = new EntryDeckData(unit.ID);
            data.AddAmount(amount - 1);
            MyPlayerUnits[unit.ID] = data;
        }
        else
        {
            data.AddAmount(amount);
        }

        SaveLoadManager.Instance.SaveModuleData(SaveModule.InventoryUnit);
    }

    public void AddSkill(ActiveSkillSO skill, out bool isDuplicate)
    {
        if (MySkills.TryAdd(skill.ID, new SkillData(skill)))
        {
            isDuplicate = false;
            AddSkillByJob(skill.jobType, skill.ID);
        }
        else
        {
            isDuplicate = true;
        }

        SaveLoadManager.Instance.SaveModuleData(SaveModule.InventorySkill);
    }

    private void AddSkillByJob(JobType jobType, int itemId)
    {
        if (!JobSkillInventory.TryGetValue(jobType, out List<int> inventoryList))
        {
            inventoryList = new List<int>();
        }

        inventoryList.Add(itemId);
        JobSkillInventory[jobType] = inventoryList;
    }


    public EntryDeckData GetPlayerUnit(int id)
    {
        return MyPlayerUnits.GetValueOrDefault(id);
    }

    public List<EntryDeckData> GetPlayerUnits()
    {
        return MyPlayerUnits.Values.ToList();
    }

    public List<SkillData> GetInventorySkillsByJob(JobType jobType)
    {
        if (!JobSkillInventory.TryGetValue(jobType, out List<int> idList))
        {
            return new List<SkillData>();
        }

        List<SkillData> items = idList.Where(id => MySkills.ContainsKey(id)).Select(id => MySkills[id]).ToList();
        return items;
    }

    public List<SkillData> GetInventorySkills()
    {
        return MySkills.Values.ToList();
    }

    public void ApplyLoadedUnits(List<SaveEntryDeckData> loadedUnits)
    {
        MyPlayerUnits.Clear();

        foreach (SaveEntryDeckData saveData in loadedUnits)
        {
            EntryDeckData data = saveData.ToRuntime();
            if (MyPlayerUnits.TryAdd(data.CharacterSo.ID, data))
            {
                if (data.CompeteSlotInfo.IsInDeck)
                {
                    DeckSelectManager.Instance.SetUnitInDeck(data, data.CompeteSlotInfo.SlotIndex);
                }
            }
        }
    }

    public void ApplyLoadedSkills(List<SaveSkillData> loadedSkills)
    {
        MySkills.Clear();
        foreach (SaveSkillData saveData in loadedSkills)
        {
            SkillData data = saveData.ToRuntime();
            if (MySkills.TryAdd(data.skillSo.ID, data))
            {
                AddSkillByJob(data.skillSo.jobType, data.skillSo.ID);
            }
        }
    }


    public void Cheat()
    {
#if UNITY_EDITOR
        foreach (ItemSO itemSo in TableManager.Instance.GetTable<ItemTable>().DataDic.Values)
        {
            if (itemSo is EquipmentItemSO equipSo)
            {
                InventoryManager.Instance.AddItem(new EquipmentItem(equipSo));
            }
        }

        foreach (ActiveSkillSO skill in TableManager.Instance.GetTable<ActiveSkillTable>().DataDic.Values)
        {
            AddSkill(skill, out _);
        }

        foreach (PlayerUnitSO unit in TableManager.Instance.GetTable<PlayerUnitTable>().DataDic.Values)
        {
            AddPlayerUnit(unit);
        }
#endif
    }
}