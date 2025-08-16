using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialActionExecutor
{
    protected TutorialManager manager;

    public void SetManager(TutorialManager mgr) => manager = mgr;

    // 튜토리얼 단계 시작 시 호출 (동작 실행)
    public abstract void Enter(TutorialActionData actionData);

    // 튜토리얼 단계 종료 시 호출
    public abstract void Exit();
}
