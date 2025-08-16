using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유닛의 턴 진행 상태를 나타내는 열거형입니다
/// </summary>
public enum TurnStateType
{
    StartTurn,    // 턴 시작 상태
    MoveToTarget, // 타겟으로 이동하는 상태
    Act,          // 공격/스킬 행동을 수행하는 상태
    Return,       // 원위치로 돌아가는 상태 
    EndTurn       // 턴 종료 상태
}

/// <summary>
/// 유닛의 상태 기계를 제어하기 위한 인터페이스입니다
/// </summary>
public interface IUnitFsmControllable
{
    /// <summary>
    /// 유닛의 상태를 변경합니다
    /// </summary>
    void ChangeUnitState(Enum newState);

    /// <summary>
    /// 유닛이 목표 위치에 도달했는지 여부
    /// </summary>
    /// <summary>
    /// 현재 재생중인 애니메이션이 완료되었는지 여부
    /// </summary>
    bool IsAnimationDone { get; }
}

/// <summary>
/// 유닛의 턴 상태를 관리하고 상태 전환을 처리하는 상태 기계입니다
/// </summary>
public class TurnStateMachine
{
    private ITurnState currentState;
    private Unit owner;

    /// <summary>
    /// 턴 상태 머신을 초기화하고 시작 상태를 설정합니다
    /// </summary>
    /// <param name="unit">소유자 유닛</param>
    /// <param name="entryState">시작 상태</param>
    public void Initialize(Unit unit, ITurnState entryState)
    {
        owner = unit;
        ChangeState(entryState);
    }


    /// <summary>
    /// 현재 상태의 업데이트를 실행합니다
    /// </summary>
    /// <summary>
    /// 현재 상태의 업데이트 메서드를 호출합니다
    /// </summary>
    public void Update()
    {
        currentState?.OnUpdate(owner);
    }

    /// <summary>
    /// 현재 상태를 새로운 상태로 변경합니다
    /// </summary>
    /// <param name="state">변경할 새로운 상태</param>
    public void ChangeState(ITurnState state)
    {
        currentState?.OnExit(owner);
        currentState = state;
        currentState.OnEnter(owner);
    }
}

/// <summary>
/// 유닛의 턴 시작 상태를 처리하는 클래스입니다
/// 공격/스킬 행동에 따라 다음 상태로 전환합니다
/// </summary>
public class StartTurnState : ITurnState
{
    /// <summary>
    /// 유닛의 턴 시작 상태로 진입할 때 호출됩니다
    /// </summary>
    /// <param name="unit">상태를 시작하는 유닛</param>
    public void OnEnter(Unit unit)
    {
        if (unit.CurrentAction == ActionType.Attack || unit.CurrentAction == ActionType.Skill)
        {
            bool isMelee = unit.CurrentAttackAction != null &&
                           unit.CurrentAttackAction.DistanceType == AttackDistanceType.Melee;
            unit.ChangeTurnState(isMelee ? TurnStateType.MoveToTarget : TurnStateType.Act);
        }
    }

    /// <summary>
    /// 턴 시작 상태의 업데이트를 처리합니다
    /// </summary>
    /// <param name="unit">현재 유닛</param>
    public void OnUpdate(Unit unit)
    {
    }

    /// <summary>
    /// 턴 시작 상태에서 나갈 때 호출됩니다
    /// </summary>
    /// <param name="unit">상태를 종료하는 유닛</param>
    public void OnExit(Unit unit)
    {
    }
}

/// <summary>
/// 유닛이 타겟을 향해 이동하는 상태를 처리하는 클래스입니다
/// </summary>
public class MoveToTargetState : ITurnState
{
    /// <summary>
    /// 타겟으로 이동하는 상태로 진입할 때 호출됩니다
    /// </summary>
    /// <param name="unit">이동을 시작하는 유닛</param>
    public void OnEnter(Unit unit)
    {
        unit.EnterMoveState();
    }

    public void OnUpdate(Unit unit)
    {
    }

    public void OnExit(Unit unit)
    {
    }
}

/// <summary>
/// 유닛의 공격/스킬 행동을 처리하는 상태 클래스입니다
/// 행동 완료 및 타겟 사망 등의 이벤트를 처리합니다
/// </summary>
public class ActState : ITurnState
{
    private bool advanced;
    private Action onEnd;

    private Action<Unit> onFinalTargetLocked;
    private Action onAttackerAnimEnd;
    private Action onTargetDead;
    private Unit lockedTarget;

    /// <summary>
    /// 행동 상태로 진입할 때 호출됩니다
    /// 공격/스킬 행동과 관련된 이벤트 핸들러를 설정합니다
    /// </summary>
    /// <param name="unit">행동을 수행하는 유닛</param>
    public void OnEnter(Unit unit)
    {
        // 상태 진행 플래그 초기화
        advanced = false;
        onEnd = () =>
        {
            if (advanced)
            {
                return;
            }

            advanced = true;
            ProceedToNextState(unit);
        };
        //분노로 타겟이 바꼈을때를 대비한 
        onFinalTargetLocked = (finalTarget) =>
        {
            if (lockedTarget != null)
            {
                return;
            }

            lockedTarget = finalTarget;
            if (lockedTarget == null)
            {
                return;
            }

            onAttackerAnimEnd = () =>
            {
                if (advanced)
                {
                    return;
                }

                advanced = true;
                ProceedToNextState(unit);
                unit.InvokeHitFinished();
                unit.OnMeleeAttackFinished -= onAttackerAnimEnd;
                unit.OnRangeAttackFinished -= onAttackerAnimEnd;
                onAttackerAnimEnd = null;
            };

            onTargetDead = () =>
            {
                if (unit.CurrentAttackAction != null && unit.CurrentAttackAction.DistanceType == AttackDistanceType.Melee)
                {
                    unit.OnMeleeAttackFinished += onAttackerAnimEnd;
                }
                else
                {
                    unit.OnRangeAttackFinished += onAttackerAnimEnd;
                }

                //진짜 가끔 애니 이벤트가 끝나 있으면 즉시 진행
                if (unit.IsAnimationDone && onAttackerAnimEnd != null)
                {
                    Action animEnd = onAttackerAnimEnd;
                    onAttackerAnimEnd = null;
                    animEnd?.Invoke();
                }

                lockedTarget.OnDead -= onTargetDead;
                onTargetDead = null;
            };
            lockedTarget.OnDead += onTargetDead;
        };
        unit.FinalTargetLocked += onFinalTargetLocked;

        if (unit.CurrentAction == ActionType.Attack)
        {
            unit.OnHitFinished += onEnd;
            unit.EnterAttackState();
        }
        else if (unit.CurrentAction == ActionType.Skill)
        {
            unit.OnSkillFinished += onEnd;
            unit.EnterSkillState();
        }
    }

    public void OnUpdate(Unit unit)
    {
    }

    public void OnExit(Unit unit)
    {
        unit.OnHitFinished -= onEnd;
        unit.OnSkillFinished -= onEnd;

        if (lockedTarget != null && onTargetDead != null)
        {
            lockedTarget.OnDead -= onTargetDead;
        }

        if (onAttackerAnimEnd != null)
        {
            unit.OnMeleeAttackFinished -= onAttackerAnimEnd;
            unit.OnRangeAttackFinished -= onAttackerAnimEnd;
        }

        if (onFinalTargetLocked != null)
        {
            unit.FinalTargetLocked -= onFinalTargetLocked;
        }

        lockedTarget = null;
        onTargetDead = null;
        onAttackerAnimEnd = null;
        onFinalTargetLocked = null;
        onEnd = null;
        advanced = false;
    }

    private void ProceedToNextState(Unit unit)
    {
        if (unit.IsDead)
        {
            unit.ChangeTurnState(TurnStateType.EndTurn);
            return;
        }

        bool isMelee = unit.CurrentAttackAction != null &&
                       unit.CurrentAttackAction.DistanceType == AttackDistanceType.Melee;
        unit.ChangeTurnState(isMelee ? TurnStateType.Return : TurnStateType.EndTurn);
    }
}

/// <summary>
/// 유닛이 원래 위치로 돌아가는 상태를 처리하는 클래스입니다
/// </summary>
public class ReturnState : ITurnState
{
    /// <summary>
    /// 원위치로 돌아가는 상태로 진입할 때 호출됩니다
    /// </summary>
    /// <param name="unit">귀환하는 유닛</param>
    public void OnEnter(Unit unit)
    {
        unit.EnterReturnState();
    }

    public void OnUpdate(Unit unit) { }
    public void OnExit(Unit unit)   { }
}

/// <summary>
/// 유닛의 턴 종료를 처리하는 상태 클래스입니다
/// 턴 종료 지연 처리를 수행합니다
/// </summary>
public class EndTurnState : ITurnState
{
    /// <summary>
    /// 턴 종료 상태로 진입할 때 호출됩니다
    /// </summary>
    /// <param name="unit">턴을 종료하는 유닛</param>
    public void OnEnter(Unit unit)
    {
        unit.StartCoroutine(DelayEndTurn(unit));
    }

    public void OnUpdate(Unit unit) { }
    public void OnExit(Unit unit)   { }

    private IEnumerator DelayEndTurn(Unit unit)
    {
        yield return new WaitForFixedUpdate();
        unit.EndTurn();
    }
}