using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardActionExecutor : TutorialActionExecutor
{
    public override void Enter(TutorialActionData actionData)
    {
        var data = actionData as RewardActionData;
        if (data == null) return;

        RewardManager.Instance.AddReward(data.rewardKey);
        RewardManager.Instance.GiveRewardAndOpenUI(() =>
        {
            manager.CompleteCurrentStep();
        });
    }

    public override void Exit() { }
}
