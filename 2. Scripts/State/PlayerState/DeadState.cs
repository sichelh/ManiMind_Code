public class DeadState : IState<PlayerUnitController, PlayerUnitState>
{
    public void OnEnter(PlayerUnitController owner)
    {
        owner.Animator.SetTrigger(Define.DeadAnimationHash);
        owner.OnToggleNavmeshAgent(false);
        owner.PlayDeadSound();
        owner.LastAttacker?.InvokeHitFinished();
    }

    public void OnUpdate(PlayerUnitController owner)
    {
    }

    public void OnFixedUpdate(PlayerUnitController owner)
    {
    }

    public void OnExit(PlayerUnitController entity)
    {
    }
}