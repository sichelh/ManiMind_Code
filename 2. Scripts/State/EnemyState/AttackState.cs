using UnityEngine;

namespace EnemyState
{
    public class AttackState : IState<EnemyUnitController, EnemyUnitState>
    {
        private readonly int attack = Define.AttackAnimationHash;

        public void OnEnter(EnemyUnitController owner)
        {
            owner.IsAnimationDone = false;
            owner.OnToggleNavmeshAgent(false);
            owner.PlayAttackVoiceSound();
            owner.transform.LookAt(owner.IsCounterAttack ? owner.CounterTarget.Collider.transform : owner.Target.Collider.transform);
            owner.Animator.SetTrigger(attack);
        }

        public void OnUpdate(EnemyUnitController owner)
        {
        }

        public void OnFixedUpdate(EnemyUnitController owner)
        {
        }

        public void OnExit(EnemyUnitController owner)
        {
            owner.transform.localRotation = Quaternion.identity;
        }
    }
}