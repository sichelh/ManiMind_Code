using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SaveEntryDeckData
{
    public int[] SkillDataIds = new int[3];
    public EquippedItemSaveData[] EquipItemItems = new EquippedItemSaveData[3];
    public int Level;
    public int Amount;
    public int TranscendLevel;
    public CompetedSlotInfo CompeteSlotInfo;
    public int CharacterId;

    public SaveEntryDeckData(EntryDeckData data)
    {
        SkillDataIds = data.SkillDatas?.Select(x => x?.skillSo?.ID ?? -1).ToArray() ?? new int[3];
        EquipItemItems = data.EquippedItems.Values
            .Select(equip =>
                new EquippedItemSaveData(
                    equip?.EquipmentItemSo.ID ?? -1,
                    equip?.InventoryId ?? -1)
            ).ToArray();

        Level = data.Level;
        Amount = data.Amount;
        TranscendLevel = data.TranscendLevel;
        CompeteSlotInfo = data.CompeteSlotInfo;
        CharacterId = data.CharacterSo.ID;
    }

    public SaveEntryDeckData() { }

    public SaveEntryDeckData(int id)
    {
        CharacterId = id;
        Level = 1;
    }

    public EntryDeckData ToRuntime()
    {
        EntryDeckData entry = new(this);
        return entry;
    }
}

public struct CompetedSlotInfo
{
    public int  SlotIndex { get; private set; }
    public bool IsInDeck  { get; private set; }

    public CompetedSlotInfo(int slotIndex, bool isInDeck)
    {
        SlotIndex = slotIndex;
        IsInDeck = isInDeck;
    }
}

public struct EquippedItemSaveData
{
    public int ItemId;      // ItemSO의 ID
    public int InventoryId; // InventoryManager에서 부여한 InventoryId (ex. 0, 1, 2...)

    public EquippedItemSaveData(int itemId, int inventoryId)
    {
        ItemId = itemId;
        InventoryId = inventoryId;
    }
}

public class EntryDeckData
{
    // 캐릭터 직업, 액티브 스킬 3개, 패시브 스킬
    // 장비 등등 equipItem
    // 패시브SO
    public SkillData[]                              SkillDatas      { get; private set; } = new SkillData[3];
    public Dictionary<EquipmentType, EquipmentItem> EquippedItems   { get; private set; } = new();
    public int                                      Level           { get; private set; }
    public int                                      Amount          { get; private set; }
    public int                                      TranscendLevel  { get; private set; }
    public CompetedSlotInfo                         CompeteSlotInfo { get; private set; } = new();
    public PlayerUnitSO                             CharacterSo     { get; private set; }


    private const int BASE_MAX_LEVEL = 10;
    private const int MAX_TRANSCEND_LEVEL = 5;
    public int MaxLevel => BASE_MAX_LEVEL + (TranscendLevel * BASE_MAX_LEVEL);


    public event Action OnEquipmmmentChanged;
    public event Action OnSkillChanged;
    public event Action OnTranscendChanged;
    public event Action OnLevelUp;

    public EntryDeckData(int id)
    {
        Level = 1;
        Amount = 0;
        TranscendLevel = 0;
        CharacterSo = TableManager.Instance.GetTable<PlayerUnitTable>().GetDataByID(id);
    }

    public EntryDeckData(SaveEntryDeckData data)
    {
        Level = data.Level;
        Amount = data.Amount;
        TranscendLevel = data.TranscendLevel;
        CharacterSo = TableManager.Instance.GetTable<PlayerUnitTable>().GetDataByID(data.CharacterId);
        if (CharacterSo == null)
        {
            Debug.LogError($"Character So is NUll ID : {data.CharacterId}");
        }

        Compete(data.CompeteSlotInfo.SlotIndex, data.CompeteSlotInfo.IsInDeck);
        // 장비 복원 (장비는 ID로 하면 안댐)
        foreach (EquippedItemSaveData equipSave in data.EquipItemItems)
        {
            if (equipSave.InventoryId < 0 || !InventoryManager.Instance.Inventory.TryGetValue(equipSave.InventoryId, out InventoryItem item))
            {
                continue;
            }

            if (item is EquipmentItem equipmentItem)
            {
                EquipItem(equipmentItem);
            }
        }

        SkillDatas = new SkillData[3];
        for (int i = 0; i < data.SkillDataIds.Length && i < SkillDatas.Length; i++)
        {
            int skillId = data.SkillDataIds[i];
            if (skillId <= 0)
            {
                continue;
            }

            //스킬 인벤토리에 있는걸 가져와야댐

            SkillData skills = AccountManager.Instance.GetInventorySkillsByJob(CharacterSo.JobType).Find(x => x.skillSo.ID == skillId);

            EquipSkill(skills);
        }
    }

    public void SetLevel(int level)
    {
        Level = level;
    }

    public void LevelUp(out bool result)
    {
        result = Level < MaxLevel;
        if (!result)
        {
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("이미 최대 레벨입니다.");
            return;
        }

        AccountManager.Instance.UseGold(Define.RequierUnitLevelUpGold, out result);
        if (!result)
        {
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("골드가 부족합니다.");
            return;
        }

        Level++;
        OnLevelUp?.Invoke();
        SaveLoadManager.Instance.SaveModuleData(SaveModule.InventoryUnit);
    }

    public void SetTranscendLevel(int transcendLevel)
    {
        TranscendLevel = transcendLevel;
    }

    public void Transcend(out bool result)
    {
        result = TranscendLevel < MAX_TRANSCEND_LEVEL;
        if (!result)
        {
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("더이상 초월을 할 수 없습니다.");
            return;
        }

        bool hasEnoughAmount = Amount >= Define.DupeCountByTranscend[TranscendLevel];
        if (!hasEnoughAmount)
        {
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("초월에 필요한 영웅이 부족합니다.");
            result = false;
            return;
        }

        bool hasEnoughGold = AccountManager.Instance.Gold >= Define.RequierUnitTranscendGold;
        if (!hasEnoughGold)
        {
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("초월에 필요한 재화가 부족합니다.");
            result = false;
            return;
        }

        // 검사는 끝났으니 이제 실제로 차감
        SubAmount(Define.DupeCountByTranscend[TranscendLevel], out bool _);
        AccountManager.Instance.UseGold(Define.RequierUnitTranscendGold, out bool _);

        TranscendLevel++;
        OnTranscendChanged?.Invoke();
        SaveLoadManager.Instance.SaveModuleData(SaveModule.InventoryUnit);
        result = true;
    }

    public void Compete(int index, bool isCompeted)
    {
        CompeteSlotInfo = new CompetedSlotInfo(index, isCompeted);
    }

    public void SetAmount(int amount)
    {
        Amount = amount;
    }

    public void AddAmount(int amount = 1)
    {
        Amount += amount;
    }

    public void SubAmount(int amount, out bool result)
    {
        result = Amount >= amount;
        if (result)
        {
            Amount -= amount;
        }
    }

    public void EquipItem(EquipmentItem item)
    {
        item.EquipItem(this);
        EquippedItems[item.EquipmentItemSo.EquipmentType] = item;
        InvokeEquipmentChanged();
    }

    public void UnEquipItem(EquipmentType type)
    {
        if (EquippedItems.TryGetValue(type, out EquipmentItem item))
        {
            item.UnEquipItem();
            EquippedItems.Remove(type);
            InvokeEquipmentChanged();
        }
    }

    public void EquipSkill(SkillData skillData)
    {
        int index = Array.IndexOf(SkillDatas, null);
        if (index > -1)
        {
            skillData.EquippedSkill(this);
            SkillDatas[index] = skillData;
            InvokeSkillChanged();
        }
    }

    public void UnEquipSkill(SkillData skillData)
    {
        int index = Array.IndexOf(SkillDatas, skillData);
        if (index > -1)
        {
            skillData.UnEquippedSkill();
            SkillDatas[index] = null;
            InvokeSkillChanged();
        }
    }

    public void InvokeEquipmentChanged()
    {
        OnEquipmmmentChanged?.Invoke();
    }

    public void InvokeSkillChanged()
    {
        OnSkillChanged?.Invoke();
    }
}