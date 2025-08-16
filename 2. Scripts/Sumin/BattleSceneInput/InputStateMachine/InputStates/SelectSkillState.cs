// 유닛의 행동 선택
public class SelectSkillState : IInputState
{
    private readonly InputContext context;
    private readonly UnitSelector selector;
    private readonly InputStateMachine inputStateMachine;

    public SelectSkillState(InputContext context, UnitSelector selector, InputStateMachine inputStateMachine)
    {
        this.context = context;
        this.selector = selector;
        this.inputStateMachine = inputStateMachine;
    }

    public void Enter() { }

    public void HandleInput() 
    {
        if (selector.TrySelectUnit(context.UnitLayer, out ISelectable target))
        {
            // 유닛 클릭 시 아무 일도 하지 않음
        }

        if (context.SelectedSkill != null)
        {
            inputStateMachine.ChangeState<SelectTargetState>();
        }
    }

    public void Exit() { }
}
