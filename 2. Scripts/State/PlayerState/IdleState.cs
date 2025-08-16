using System.Buffers;
using UnityEngine;

namespace PlayerState
{
    public class IdleState : IState<PlayerUnitController, PlayerUnitState>
    {
        private readonly int isMove = Define.MoveAnimationHash;

        public void OnEnter(PlayerUnitController owner)
        {
            owner.Agent.avoidancePriority = 1;
            owner.Animator.SetBool(isMove, false);

            owner.OnToggleNavmeshAgent(false);
            owner.transform.localRotation = Quaternion.identity;
        }

        public void OnUpdate(PlayerUnitController owner)
        {
        }

        public void OnFixedUpdate(PlayerUnitController owner)
        {
        }

        public void OnExit(PlayerUnitController owner)
        {
        }
    }
}