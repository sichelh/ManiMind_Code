using UnityEngine;

namespace EnemyState
{
    public class ReturnState : IState<EnemyUnitController, EnemyUnitState>
    {
        private readonly int isMove = Define.MoveAnimationHash;
        private bool waitFrame;

        public void OnEnter(EnemyUnitController owner)
        {
            owner.OnToggleNavmeshAgent(true);
            owner.Agent.avoidancePriority = 10;
            owner.Animator.SetBool(isMove, true);
            owner.MoveTo(owner.StartPosition);

            waitFrame = false;
        }

        public void OnUpdate(EnemyUnitController owner)
        {
            if (owner.IsDead)
            {
                owner.ChangeUnitState(EnemyUnitState.Die);
                return;
            }


            if (!owner.Agent.pathPending && owner.Agent.remainingDistance <= owner.Agent.stoppingDistance && !waitFrame)
            {
                waitFrame = true;
                owner.ChangeTurnState(TurnStateType.EndTurn);
            }
        }

        public void OnFixedUpdate(EnemyUnitController owner)
        {
        }

        public void OnExit(EnemyUnitController owner)
        {
            owner.Animator.SetBool(isMove, false);
            owner.Agent.updateRotation = false;
            owner.transform.localRotation = Quaternion.identity;
            owner.Agent.updateRotation = true;
            owner.Agent.isStopped = true;
            owner.Agent.velocity = Vector3.zero;
            owner.Agent.ResetPath();
            owner.OnToggleNavmeshAgent(false);
        }
    }
}