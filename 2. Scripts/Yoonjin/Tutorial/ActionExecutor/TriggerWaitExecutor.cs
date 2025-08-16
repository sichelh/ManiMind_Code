using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerWaitExecutor : TutorialActionExecutor
{
    private string activeEventKey;

    public override void Enter(TutorialActionData actionData)
    {
        TriggerWaitActionData waitData = actionData as TriggerWaitActionData;
        if (waitData == null)
        {
            return;
        }

        // UI 인터랙션 차단
        if (waitData.blockAllUI)
        {
            TutorialUIBlocker.BlockAll();
        }

        activeEventKey = null;

        switch (waitData.triggerType)
        {
            case TriggerType.SceneLoaded:
                LoadingScreenController.Instance.OnLoadingComplete += OnTriggered;
                break;
            case TriggerType.MonsterKilled:
                activeEventKey = "MonsterKilled";
                EventBus.Subscribe(activeEventKey, OnTriggered);
                break;
            case TriggerType.BattleVictory:
                activeEventKey = "BattleVictory";
                EventBus.Subscribe(activeEventKey, OnTriggered);
                break;
            case TriggerType.CustomEvent:
                activeEventKey = waitData.triggerEventName;
                EventBus.Subscribe(activeEventKey, OnTriggered);
                break;
        }
    }

    private void OnTriggered()
    {
        Cleanup();
        manager.CompleteCurrentStep();
    }

    private void Cleanup()
    {
        LoadingScreenController.Instance.OnLoadingComplete -= OnTriggered;

        if (!string.IsNullOrEmpty(activeEventKey))
        {
            EventBus.Unsubscribe(activeEventKey, OnTriggered);
        }

        activeEventKey = null;

        TutorialUIBlocker.Clear();
    }

    public override void Exit()
    {
        Cleanup();
    }
}