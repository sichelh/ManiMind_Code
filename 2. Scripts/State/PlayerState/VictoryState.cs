using UnityEngine;

namespace PlayerState
{
    public class VictoryState : IState<PlayerUnitController, PlayerUnitState>
    {
        public void OnEnter(PlayerUnitController owner)
        {
            owner.Animator.SetBool(Define.VictoryAnimationHash, true);
            owner.transform.localRotation = Quaternion.Euler(0, 110f, 0);
        }

        public void OnUpdate(PlayerUnitController owner)
        {
        }

        public void OnFixedUpdate(PlayerUnitController owner)
        {
        }

        public void OnExit(PlayerUnitController entity)
        {
            entity.Animator.SetBool(Define.VictoryAnimationHash, false);
        }
    }
}