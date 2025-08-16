using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class AttackState : IState<PlayerUnitController, PlayerUnitState>
{
    private readonly int attack = Define.AttackAnimationHash;

    public void OnEnter(PlayerUnitController owner)
    {
        owner.IsAnimationDone = false;
        owner.OnToggleNavmeshAgent(false);
        owner.PlayAttackVoiceSound();
        owner.transform.LookAt(owner.IsCounterAttack ? owner.CounterTarget.Collider.transform : owner.Target.Collider.transform);
        owner.Animator.SetTrigger(attack);
    }

    public void OnUpdate(PlayerUnitController owner)
    {
    }

    public void OnFixedUpdate(PlayerUnitController owner)
    {
    }

    public void OnExit(PlayerUnitController owner)
    {
        // owner.OnToggleNavmeshAgent(true);
        owner.transform.localRotation = Quaternion.identity;
    }
}