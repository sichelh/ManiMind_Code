using System.Collections;
using System.Linq;
using UnityEngine;

public class InputManager : SceneOnlySingleton<InputManager>
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private BattleSceneGameUI gameUI;

    [Header("선택 타겟 레이어 설정")]
    [SerializeField] private LayerMask unitLayer;

    [SerializeField] private LayerMask playerUnitLayer;
    [SerializeField] private LayerMask enemyUnitLayer;

    private InputStateMachine inputStateMachine;
    private InputContext context;
    private UnitSelector selector;

    private void Start()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }

        if (gameUI == null)
        {
            Debug.LogError("InputManager에 BattleSceneGameUI를 연결해주세요.");
        }

        context = new InputContext
        {
            UnitLayer = unitLayer,
            PlayerUnitLayer = playerUnitLayer,
            EnemyUnitLayer = enemyUnitLayer,
            OpenSkillUI = unit => UIManager.Instance.GetUIComponent<BattleSceneSkillUI>().UpdateSkillList(unit),
            CloseSkillUI = () => UIManager.Instance.Close(UIManager.Instance.GetUIComponent<BattleSceneSkillUI>()),
            DisableStartButtonUI = () => gameUI.ToggleInteractableStartButton(false),
            EnableStartButtonUI = () => gameUI.ToggleInteractableStartButton(true),
            PlanActionCommand = (executer, target, skillData) =>
            {
                IActionCommand command;
                if (context.SelectedSkill == null)
                    command = new AttackCommand(executer, target);
                else
                    command = new SkillCommand(executer, target, skillData);

                CommandPlanner.Instance.PlanAction(command);

                if (context.SelectedExecuter is PlayerUnitController owner)
                {
                    owner.ChangeUnitState(PlayerUnitState.ReadyAction);
                }

                Debug.Log($"커맨드 등록 : {executer}, {target}, {(skillData == null ? "기본공격" : skillData.skillSo.name)}");
            },
            HighlightSkillSlotUI = (toggle, index) =>
            {
                UIManager.Instance.GetUIComponent<BattleSceneSkillUI>().ToggleHighlightSkillSlot(toggle, index);
            },
            HighlightBasicAttackUI = (toggle) =>
            {
                UIManager.Instance.GetUIComponent<BattleSceneSkillUI>().ToggleHighlightBasicAttack(toggle);
            }
        };

        inputStateMachine = new InputStateMachine();
        selector = new UnitSelector(context, mainCam);

        inputStateMachine.CreateInputState<SelectExecuterState>(new SelectExecuterState(context, selector, inputStateMachine));
        inputStateMachine.CreateInputState<SelectSkillState>(new SelectSkillState(context, selector, inputStateMachine));
        inputStateMachine.CreateInputState<SelectTargetState>(new SelectTargetState(context, selector, inputStateMachine));
        inputStateMachine.CreateInputState<InputDisabledState>(new InputDisabledState(context, selector));

        StartCoroutine(WaitForBattleManagerInit());
    }

    // BattleManager가 초기화 완료된 후 InputManager의 SelectExecuter 상태 시작 (호출 순서 문제 해결)
    private IEnumerator WaitForBattleManagerInit()
    {
        yield return new WaitUntil(() => BattleManager.Instance != null && BattleManager.Instance.PartyUnits.Count > 0);

        Initialize();
    }

    void Update()
    {
        inputStateMachine.Update();
    }

    // Input매니저 초기화
    public void Initialize()
    {
        inputStateMachine.ChangeState<SelectExecuterState>();
        selector.InitializeHighlight();
        gameUI.ToggleActiveStartBtn(true);
    }

    public void BattleEnd()
    {
        gameUI.BattleEnd();
    }

    // Skill 선택 중 나가기 버튼
    public void OnClickSkillExitButton()
    {
        // 인디케이터 꺼주기
        selector.InitializeHighlight();
        selector.ShowSelectableUnits(context.UnitLayer, false);

        // Start 버튼 활성화
        context.EnableStartButtonUI?.Invoke();

        inputStateMachine.ChangeState<SelectExecuterState>();
    }

    // 바깥 부분 누르면 Exit 되는 기능 -> 롤백. 오히려 타겟 선택에 불편감을 줌.
    //public void ExitSkillSelect()
    //{
    //    if (inputStateMachine.CurrentState is SelectExecuterState)
    //        return;

    //    // 튜토리얼에서 PlayerSelected 이벤트 발행
    //    var tutorial = TutorialManager.Instance;

    //    // 튜토리얼 중 HighlightUI 액션일 경우엔 나가기 막기
    //    if (tutorial != null && tutorial.IsActive &&
    //        tutorial.CurrentStep?.Actions
    //            .Any(x => x.ActionType == TutorialActionType.HighlightUI || x.ActionType == TutorialActionType.TriggerWait) == true)
    //    {
    //        return;
    //    }

    //    OnClickSkillExitButton();
    //}

    // Start 버튼
    public void OnClickTurnStartButton()
    {
        OnPlayMode();
        CommandPlanner.Instance.ExecutePlannedActions();

        // 배틀매니저 턴 시작
        BattleManager.Instance.StartTurn();
    }

    // 인풋 불가 상태로 진입
    public void OnPlayMode()
    {
        inputStateMachine.ChangeState<InputDisabledState>();
    }

    // 스킬 선택 버튼
    public void SelectSkill(int index)
    {
        selector.InitializeHighlight();
        // 스킬 인덱스 받아서 context에 저장
        if (context.SelectedExecuter is PlayerUnitController playerUnit)
        {
            context.SelectedSkill = playerUnit.SkillController.GetSkillData(index);
            
            // 만약 선택한 스킬의 타겟이 자기 자신일 경우 command에 바로 저장하고 타겟 선택하지 않음 
            if (context.SelectedSkill.Effect.skillEffectDatas[0].selectTarget == SelectTargetType.OnSelf)
            {
                context.PlanActionCommand?.Invoke(context.SelectedExecuter.SelectedUnit, context.SelectedExecuter.SelectedUnit, context.SelectedSkill);
                inputStateMachine.ChangeState<SelectExecuterState>();
            }
            else
            {
                UpdateTargetIndicator();
            }
        }
    }

    // 기본 공격 선택 버튼
    public void SelectBasicAttack()
    {
        selector.InitializeHighlight();
        context.SelectedSkill = null;

        inputStateMachine.ChangeState<SelectTargetState>();

        UpdateTargetIndicator();
    }

    // 선택한 액션 타입(기본공격/스킬)에 따라 ChangeAction하는 함수
    private void ChangeSelectedUnitAction(ActionType actionType)
    {
        if (context.SelectedExecuter is PlayerUnitController playerUnit)
        {
            playerUnit.ChangeAction(actionType);
        }
    }

    // 타겟 인디케이터 업데이트
    private void UpdateTargetIndicator()
    {
        selector.ShowSelectableUnits(unitLayer, false);
        context.TargetLayer = selector.GetLayerFromSkill(context.SelectedSkill);
        selector.ShowSelectableUnits(context.TargetLayer, true);
    }
}