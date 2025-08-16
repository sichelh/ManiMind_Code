using UnityEngine;

namespace EnemyState
{
    public class DeadState : IState<EnemyUnitController, EnemyUnitState>
    {
        public void OnEnter(EnemyUnitController owner)
        {
            owner.Animator.SetTrigger(Define.DeadAnimationHash);
            owner.PlayDeadSound();
            owner.OnToggleNavmeshAgent(false);
        }

        public void OnUpdate(EnemyUnitController owner)
        {
        }

        public void OnFixedUpdate(EnemyUnitController owner)
        {
        }

        public void OnExit(EnemyUnitController entity)
        {
            entity.LastAttacker?.InvokeHitFinished();
        }
    }
}