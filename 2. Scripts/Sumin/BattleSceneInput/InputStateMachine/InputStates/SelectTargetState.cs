// 유닛의 타겟 선택

using System.Linq;

public class SelectTargetState : IInputState
{
    private readonly InputContext context;
    private readonly UnitSelector selector;
    private readonly InputStateMachine inputStateMachine;

    public SelectTargetState(InputContext context, UnitSelector selector, InputStateMachine inputStateMachine)
    {
        this.context = context;
        this.selector = selector;
        this.inputStateMachine = inputStateMachine;
    }

    public void Enter() { }

    public void HandleInput()
    {
        if (selector.TrySelectUnit(context.TargetLayer, out ISelectable target))
        {
            selector.InitializeHighlight();

            // Unit Select하면 context의 SelectedTarget에 넘겨줌
            context.SelectedTarget = target;


            // 튜토리얼 이벤트 발행
            var tutorial = TutorialManager.Instance;

            if (tutorial != null && tutorial.IsActive &&
                tutorial.CurrentStep?.Actions
                    .OfType<TriggerWaitActionData>()
                    .Any(x => x.triggerEventName == "TargetSelected") == true)
            {
                EventBus.Publish("TargetSelected");
            }

            Unit executerUnit = context.SelectedExecuter.SelectedUnit;
            Unit targetUnit   = context.SelectedTarget.SelectedUnit;

            // 선택 이펙트
            targetUnit.PlaySelectEffect();

            // 커맨드 생성
            context.PlanActionCommand?.Invoke(executerUnit, targetUnit, context.SelectedSkill);

            // 선택 단계로 이동
            inputStateMachine.ChangeState<SelectExecuterState>();
        }
    }

    public void Exit()
    {
        // 인디케이터 표시 전환
        selector.ShowSelectableUnits(context.TargetLayer, false);
        context.SelectedExecuter.ToggleSelectedIndicator(false);
        
        // Start 버튼 활성화
        context.EnableStartButtonUI?.Invoke();
    }
}