using System;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : Singleton<GameManager>
{
    public bool isTestMode;
    public bool TimeScaleMultiplier = false;
    private AccountManager   AccountManager   => AccountManager.Instance;
    private InventoryManager InventoryManager => InventoryManager.Instance;
    private SaveLoadManager  SaveLoadManager  => SaveLoadManager.Instance;
    private TableManager     TableManager     => TableManager.Instance;
    private TutorialManager  TutorialManager  => TutorialManager.Instance;

    private void Start()
    {
        SaveLoadManager.Instance.LoadAll();
        ApplySaveDataToManagers();
    }


    public void TimeScaleMultiplierUpdate()
    {
        if (TimeScaleMultiplier)
        {
            Time.timeScale = 2f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void TimeScaleSetDefault()
    {
        Time.timeScale = 1f;
        TimeScaleMultiplier = false;
    }

    public void ToggleTimeScale()
    {
        TimeScaleMultiplier = !TimeScaleMultiplier;
        TimeScaleMultiplierUpdate();
    }


    private void ApplySaveDataToManagers()
    {
        // Gold
        if (SaveLoadManager.SaveDataMap[SaveModule.Gold] is SaveGoldData goldData)
        {
            AccountManager.SetGold(goldData.Gold);
        }

        if (SaveLoadManager.SaveDataMap[SaveModule.Opal] is SaveOpalData opalData)
        {
            AccountManager.SetOpal(opalData.Opal);
        }

        // Inventory Items
        if (SaveLoadManager.SaveDataMap[SaveModule.InventoryItem] is SaveInventoryItemData itemData)
        {
            InventoryManager.ApplyLoadedInventory(itemData.InventoryItems);
        }

        // Skill Inventory
        if (SaveLoadManager.SaveDataMap[SaveModule.InventorySkill] is SaveInventorySkill skillData)
        {
            AccountManager.ApplyLoadedSkills(skillData.SkillInventory);
        }

        // Unit Inventory
        if (SaveLoadManager.SaveDataMap[SaveModule.InventoryUnit] is SaveUnitInventoryData unitData)
        {
            AccountManager.ApplyLoadedUnits(unitData.UnitInventory);
        }

        if (SaveLoadManager.SaveDataMap[SaveModule.BestStage] is SaveBestStageData bestStageData)
        {
            AccountManager.SetBestStage(bestStageData.BestStage);
        }

        if (SaveLoadManager.SaveDataMap[SaveModule.CurrentStage] is SaveCurrentStageData currentStageData)
        {
            AccountManager.UpdateLastChallengedStageId(currentStageData.LastClearedStage);
        }

        if (SaveLoadManager.SaveDataMap[SaveModule.FirstInit] is SaveFirstInit firstInit)
        {
            if (!firstInit.IsFirstInit)
            {
                foreach (Action action in Define.FirstGivenItem.Values)
                {
                    action?.Invoke();
                }

                SaveLoadManager.SaveModuleData(SaveModule.FirstInit);
            }
        }

        // if (SaveLoadManager.SaveDataMap[SaveModule.Tutorial] is SaveTutorialData tutorialData)
        // {
        //     TutorialManager.SetTutorialIndex(tutorialData.Tutorial);
        // }
        // 등등...
    }
}