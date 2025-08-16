using UnityEngine;

namespace PlayerState
{
    public class ReadyAction : IState<PlayerUnitController, PlayerUnitState>
    {
        public void OnEnter(PlayerUnitController owner)
        {
            owner.OnToggleNavmeshAgent(false);
            owner.Animator.SetBool(Define.ReadyActionAnimationHash, true);
        }

        public void OnUpdate(PlayerUnitController owner)
        {
        }

        public void OnFixedUpdate(PlayerUnitController owner)
        {
        }

        public void OnExit(PlayerUnitController owner)
        {
            owner.Animator.SetBool(Define.ReadyActionAnimationHash, false);
        }
    }
}