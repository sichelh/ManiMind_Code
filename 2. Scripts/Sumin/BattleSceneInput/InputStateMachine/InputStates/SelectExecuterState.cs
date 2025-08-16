// 플레이어 유닛 선택
using System.Linq;

public class SelectExecuterState : IInputState
{
    private readonly InputContext context;
    private readonly UnitSelector selector;
    private readonly InputStateMachine inputStateMachine;

    public SelectExecuterState(InputContext context, UnitSelector selector, InputStateMachine inputStateMachine)
    {
        this.context = context;
        this.selector = selector;
        this.inputStateMachine = inputStateMachine;
    }

    public void Enter()
    {
        context.SelectedSkill = null; // Excuter 고를때는 항상 SelectedSkill 비워주기

        // 이전에 선택해뒀던게 있으면 Indicator 지우기
        if (context.SelectedExecuter != null)
        {
            context.SelectedExecuter.ToggleSelectedIndicator(false);
        }

        // 선택 가능한 플레이어 유닛 표시
        selector.ShowSelectableUnits(context.PlayerUnitLayer, true);
        
        // 유닛 선택하기 전까지는 SkillUI 꺼두고 전투 시작 버튼은 활성화
        context.CloseSkillUI?.Invoke();
        context.EnableStartButtonUI?.Invoke();
    }

    public void HandleInput()
    {
        // 튜토리얼에서 PlayerSelected 이벤트 발행
        var tutorial = TutorialManager.Instance;

        // 튜토리얼 중 HighlightUI 액션일 경우엔 유닛 선택 막기
        if (tutorial != null && tutorial.IsActive &&
            tutorial.CurrentStep?.Actions
                .Any(x => x.ActionType == TutorialActionType.HighlightUI) == true)
        {
            return;
        }

        if (selector.TrySelectUnit(context.PlayerUnitLayer, out ISelectable unit))
        {
            // Unit Select하면 context의 SelectedUnit에 넘겨줌
            context.SelectedExecuter = unit;

            // 인디케이터 표시 전환
            selector.ShowSelectableUnits(context.PlayerUnitLayer, false);
            context.SelectedExecuter.PlaySelectEffect();
            context.SelectedExecuter.ToggleSelectedIndicator(true);

            // 이전에 지정한 명령이 있다면 보여줌
            // 명령이 있으면 context의 skillData에 넣어줌
            selector.ShowPrevCommand(unit.SelectedUnit);

            // SkillUI는 켜주고 StartButton은 꺼주기
            context.DisableStartButtonUI?.Invoke();
            context.OpenSkillUI?.Invoke(context.SelectedExecuter.SelectedUnit);

            if (tutorial != null && tutorial.IsActive &&
                tutorial.CurrentStep?.Actions
                    .OfType<TriggerWaitActionData>()
                    .Any(x => x.triggerEventName == "PlayerSelected") == true)
            {
                EventBus.Publish("PlayerSelected");
            }

            // 스킬 선택 상태로 넘어감
            inputStateMachine.ChangeState<SelectSkillState>();
        }
    }

    public void Exit() {}
}

