using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueActionExecutor : TutorialActionExecutor
{
    public override void Enter(TutorialActionData actionData)
    {
        Debug.Log("다이얼로그 액션 실행기 Enter!");
        var dialogueData = actionData as DialogueActionData;
        if (dialogueData == null) return;

        // 대사 출력 UI 호출
        DialogueController.Instance.Play(dialogueData.dialogGroupKey);
        EventBus.Subscribe("DialogueFinished", OnDialogueComplete);
    }

    private void OnDialogueComplete()
    {
        EventBus.Unsubscribe("DialogueFinished", OnDialogueComplete);
        manager.CompleteCurrentStep();
    }

    public override void Exit()
    {
        // 대사창 닫기
    }
}
