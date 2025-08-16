using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static TutorialManager;
#if UNITY_EDITOR
using static UnityEditor.Progress;
#endif

/*
 * 세이브 목록
 * 1. BestStage
 * 2. CurrentStage
 * 3. 현재 보유 캐릭터
 * 4. 현재 보유 아이템
 * 5. 현재 진행된 튜토리얼
 * 6. 덱 빌딩 (장착한 아이템, 장착한 스킬)
 * 7. 재화 (Gold, Opal)
 */
public enum SaveModule
{
    Gold,
    Opal,
    BestStage,
    CurrentStage,
    Tutorial,
    InventoryItem,
    InventoryUnit,
    InventorySkill,
    FirstInit
}

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    public Dictionary<SaveModule, SaveData> SaveDataMap { get; private set; } = new();


    public Dictionary<SaveModule, Action> SaveAction { get; private set; } = new();


    protected override void Awake()
    {
        base.Awake();
        if (isDuplicated)
        {
            return;
        }

        InitializeSaveDataMap();
        foreach (SaveModule module in Enum.GetValues(typeof(SaveModule)))
        {
            SaveAction[module] = () => SaveModuleData(module);
        }
    }

    private string GetPath(SaveModule module)
    {
        return $"save_{module.ToString().ToLowerInvariant()}";
    }

    public void SaveModuleData(SaveModule module)
    {
        if (SaveDataMap.TryGetValue(module, out SaveData data))
        {
            data.OnBeforeSave();
            Save(module, data);
        }
    }

    public void LoadAll()
    {
        foreach (SaveModule module in Enum.GetValues(typeof(SaveModule)))
        {
            SaveData loadedData = module switch
            {
                SaveModule.Gold           => Load<SaveGoldData>(module),
                SaveModule.Opal           => Load<SaveOpalData>(module),
                SaveModule.BestStage      => Load<SaveBestStageData>(module),
                SaveModule.CurrentStage   => Load<SaveCurrentStageData>(module),
                SaveModule.Tutorial       => Load<SaveTutorialData>(module),
                SaveModule.InventoryItem  => Load<SaveInventoryItemData>(module),
                SaveModule.InventoryUnit  => Load<SaveUnitInventoryData>(module),
                SaveModule.InventorySkill => Load<SaveInventorySkill>(module),
                SaveModule.FirstInit      => Load<SaveFirstInit>(module),
                _                         => null
            };

            if (loadedData != null)
            {
                SaveDataMap[module] = loadedData;
            }
            else
            {
                Debug.LogWarning($"[SaveLoadManager] {module} 로딩 실패 또는 null 반환됨");
            }
        }
    }

    public void DeleteAll()
    {
        foreach (SaveModule m in Enum.GetValues(typeof(SaveModule)))
        {
            string path = $"save_{m.ToString().ToLowerInvariant()}";
            string enc  = Path.Combine(Application.persistentDataPath, path + ".jenc");
            if (File.Exists(enc))
            {
                File.Delete(enc);
            }

            string wrapped = Path.Combine(Application.persistentDataPath, "kv", path + "_wrapped.bin");
            if (File.Exists(wrapped))
            {
                File.Delete(wrapped);
            }

            // 레거시 json 정리(있다면)
            string legacy = Path.Combine(Application.persistentDataPath, $"{m}.json");
            if (File.Exists(legacy))
            {
                File.Delete(legacy);
            }
        }
    }

    private void InitializeSaveDataMap()
    {
        SaveDataMap.Clear();

        SaveDataMap[SaveModule.Gold] = new SaveGoldData();
        SaveDataMap[SaveModule.Opal] = new SaveOpalData();
        SaveDataMap[SaveModule.BestStage] = new SaveBestStageData();
        SaveDataMap[SaveModule.CurrentStage] = new SaveCurrentStageData();
        SaveDataMap[SaveModule.Tutorial] = new SaveTutorialData();
        SaveDataMap[SaveModule.InventoryItem] = new SaveInventoryItemData();
        SaveDataMap[SaveModule.InventoryUnit] = new SaveUnitInventoryData();
        SaveDataMap[SaveModule.FirstInit] = new SaveFirstInit();
    }

    private void Save(SaveModule module, SaveData data)
    {
        string path = GetPath(module);
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        JsonSecureStore.Save(path, json);
    }

    private T Load<T>(SaveModule module) where T : new()
    {
        string path = GetPath(module);

        try
        {
            string json = JsonSecureStore.Load(path);
            return string.IsNullOrEmpty(json) ? new T() : JsonConvert.DeserializeObject<T>(json);
        }
        catch { return new T(); }
    }

    public void HandleApplicationQuit()
    {
        foreach (SaveModule module in Enum.GetValues(typeof(SaveModule)))
        {
            SaveAction[module]?.Invoke();
        }

        Application.Quit();
    }
}


public abstract class SaveData
{
    public abstract void OnBeforeSave();
}

[Serializable]
public class SaveFirstInit : SaveData
{
    public bool IsFirstInit { get; set; }

    public override void OnBeforeSave()
    {
        if (!IsFirstInit)
        {
            IsFirstInit = true;
        }
    }
}

[Serializable]
public class SaveGoldData : SaveData
{
    public int Gold { get; set; }

    public override void OnBeforeSave()
    {
        Gold = AccountManager.Instance.Gold;
    }
}

[Serializable]
public class SaveOpalData : SaveData
{
    public int Opal { get; set; } = 0;

    public override void OnBeforeSave()
    {
        Opal = AccountManager.Instance.Opal;
    }
}

[Serializable]
public class SaveBestStageData : SaveData
{
    public int BestStage { get; set; } = 1010100;

    public override void OnBeforeSave()
    {
        BestStage = AccountManager.Instance.BestStage;
    }
}

[Serializable]
public class SaveCurrentStageData : SaveData
{
    public int LastClearedStage { get; set; } = 1010101;

    public override void OnBeforeSave()
    {
        LastClearedStage = AccountManager.Instance.LastClearedStageId;
    }
}

[Serializable]
public class SaveTutorialData : SaveData
{
    public bool IsCompleted = false;
    public TutorialPhase Phase = TutorialPhase.DeckBuildingBefore;

    public override void OnBeforeSave()
    {
    }
}

[Serializable]
public class SaveInventoryItemData : SaveData
{
    public List<SaveInventoryItem> InventoryItems { get; set; } = new();

    public override void OnBeforeSave()
    {
        InventoryItems = InventoryManager.Instance.GetInventoryItems().ConvertAll(item => new SaveInventoryItem(item));
    }
}

[Serializable]
public class SaveUnitInventoryData : SaveData
{
    public List<SaveEntryDeckData> UnitInventory { get; set; } = new();

    public override void OnBeforeSave()
    {
        UnitInventory = AccountManager.Instance.GetPlayerUnits().ConvertAll(unit => new SaveEntryDeckData(unit));
    }
}

[Serializable]
public class SaveInventorySkill : SaveData
{
    public List<SaveSkillData> SkillInventory { get; set; } = new();

    public override void OnBeforeSave()
    {
        SkillInventory = AccountManager.Instance.GetInventorySkills().ConvertAll(skill => new SaveSkillData(skill));
    }
}