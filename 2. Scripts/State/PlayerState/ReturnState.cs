using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace PlayerState
{
    public class ReturnState : IState<PlayerUnitController, PlayerUnitState>
    {
        private readonly int isMove = Define.MoveAnimationHash;
        private bool waitFrame;

        public void OnEnter(PlayerUnitController owner)
        {
            owner.OnToggleNavmeshAgent(true);
            owner.Agent.avoidancePriority = 10;
            owner.Animator.SetBool(isMove, true);
            owner.MoveTo(owner.StartPostion);
            waitFrame = false;
        }

        public void OnUpdate(PlayerUnitController owner)
        {
            if (owner.IsDead)
            {
                owner.ChangeUnitState(PlayerUnitState.Die);
                return;
            }


            if (!owner.Agent.pathPending && owner.Agent.remainingDistance <= owner.Agent.stoppingDistance && !waitFrame)
            {
                waitFrame = true;
                owner.ChangeTurnState(TurnStateType.EndTurn);
            }
        }

        public void OnFixedUpdate(PlayerUnitController owner)
        {
        }

        public void OnExit(PlayerUnitController owner)
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