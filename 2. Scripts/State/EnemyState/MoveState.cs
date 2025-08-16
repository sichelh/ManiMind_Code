using UnityEngine;

namespace EnemyState
{
    public class MoveState : IState<EnemyUnitController, EnemyUnitState>
    {
        private readonly int isMove = Define.MoveAnimationHash;
        private IDamageable target;
        private float totalReachDistance;

        private bool waitFrame = false;

        public void OnEnter(EnemyUnitController owner)
        {
            owner.OnToggleNavmeshAgent(true);
            owner.Agent.avoidancePriority = 80;
            owner.Animator.SetBool(isMove, true);
            owner.MoveTo(owner.Target.Collider.transform.position);
            target = owner.Target;
            float additionalDistance = Define.GetTargetColliderRadius(target) + Define.GetTargetColliderRadius(owner);
            totalReachDistance = Mathf.Ceil(owner.Agent.stoppingDistance + additionalDistance);
        }

        public void OnUpdate(EnemyUnitController owner)
        {
            if (!owner.Agent.pathPending && owner.Agent.remainingDistance <= totalReachDistance)
            {
                owner.ChangeTurnState(TurnStateType.Act);
            }
        }

        public void OnFixedUpdate(EnemyUnitController owner)
        {
        }

        public void OnExit(EnemyUnitController owner)
        {
            owner.Animator.SetBool(isMove, false);
        }
    }
}