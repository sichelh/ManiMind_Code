using UnityEngine;

namespace PlayerState
{
    public class MoveState : IState<PlayerUnitController, PlayerUnitState>
    {
        private readonly int isMove = Define.MoveAnimationHash;

        private IDamageable target;
        private float totalReachDistance;

        public void OnEnter(PlayerUnitController owner)
        {
            owner.OnToggleNavmeshAgent(true);
            owner.Agent.avoidancePriority = 10;
            owner.Agent.isStopped = false;
            owner.Animator.SetBool(isMove, true);
            owner.MoveTo(owner.Target.Collider.transform.position);

            target = owner.Target;
            float additionalDistance = Define.GetTargetColliderRadius(target);
            totalReachDistance = Mathf.Ceil(owner.Agent.stoppingDistance + additionalDistance + Define.GetTargetColliderRadius(owner));
        }

        public void OnUpdate(PlayerUnitController owner)
        {
            if (!owner.Agent.pathPending && owner.Agent.remainingDistance <= totalReachDistance)
            {
                owner.ChangeTurnState(TurnStateType.Act);
            }
        }

        public void OnFixedUpdate(PlayerUnitController owner)
        {
        }

        public void OnExit(PlayerUnitController entity)
        {
            entity.Animator.SetBool(isMove, false);
        }
    }
}