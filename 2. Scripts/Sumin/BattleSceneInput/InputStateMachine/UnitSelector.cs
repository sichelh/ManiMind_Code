using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 유닛 선택하는 기능을 하는 클래스
public class UnitSelector
{
    private readonly Camera cam;
    private readonly InputContext context;

    // InputManager에서 생성자를 통해 카메라 연결
    public UnitSelector(InputContext context, Camera cam)
    {
        this.cam = cam;
        this.context = context;
    }

    // 유닛 선택 메서드
    public bool TrySelectUnit(LayerMask selectableUnit, out ISelectable selected)
    {
        selected = null;
        Vector2 inputPos;

        if (IsPointerOverUI())
        {
            return false;
        }


        // PC
        if (Input.GetMouseButtonDown(0))
        {
            inputPos = Input.mousePosition;
        }

        // 모바일
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputPos = Input.GetTouch(0).position;
        }

        else
        {
            return false;
        }

        // selected에 선택한 유닛 넣어주고, true로 반환
        Ray ray = cam.ScreenPointToRay(inputPos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableUnit))
        {
            selected = hit.transform.GetComponent<ISelectable>();
            if (selected.SelectedUnit.IsDead)
            {
                return false;
            }

            return selected != null;
        }

        //InputManager.Instance.ExitSkillSelect();
        return false;
    }

    // 선택 가능한 유닛 레이어에 Selectable Indicator 띄워주기
    public void ShowSelectableUnits(LayerMask layer, bool show)
    {
        List<Unit> selectableUnits = new();

        selectableUnits.AddRange(GetUnitsFromLayer(layer));

        foreach (Unit unit in selectableUnits)
        {
            unit.ToggleSelectableIndicator(show);
            if (unit.IsDead)
            {
                unit.ToggleSelectableIndicator(false);
            }
        }
    }

    // 각 레이어에 맞는 유닛들을 배틀매니저에서 받아오기
    public List<Unit> GetUnitsFromLayer(LayerMask layer)
    {
        List<Unit> units = new();
        if ((layer.value & context.PlayerUnitLayer) != 0)
        {
            units.AddRange(BattleManager.Instance.PartyUnits);
        }

        if ((layer.value & context.EnemyUnitLayer) != 0)
        {
            units.AddRange(BattleManager.Instance.EnemyUnits);
        }

        return units;
    }

    // 선택한 스킬의 타겟 진영 받아오기
    public LayerMask GetLayerFromSkill(SkillData skill)
    {
        // 기본공격이면 skill이 null 이므로 Enemy, 스킬은 SelectCampType에 따라.
        return skill == null
            ? context.EnemyUnitLayer
            : skill.skillSo.selectCamp switch
            {
                SelectCampType.Enemy     => context.EnemyUnitLayer,
                SelectCampType.Colleague => context.PlayerUnitLayer,
                SelectCampType.BothSide  => context.UnitLayer,
                _                        => context.UnitLayer
            };
    }

    // 하이라이트 초기화
    public void InitializeHighlight()
    {
        if (context.SelectedExecuter == null)
        {
            return;
        }

        Unit executer = context.SelectedExecuter.SelectedUnit;

        if (executer is PlayerUnitController playerUnit)
        {
            int skillCount = executer.SkillController.skills.Count;
            for (int i = 0; i < skillCount; i++)
            {
                context.HighlightSkillSlotUI?.Invoke(false, i);
            }
        }

        context.HighlightBasicAttackUI?.Invoke(false);

        IActionCommand command = CommandPlanner.Instance.GetPlannedCommand(executer);
        if (CommandPlanner.Instance.HasPlannedCommand(executer))
        {
            command.Target?.ToggleSelectedIndicator(false);
        }
    }

    public void ShowPrevCommand(Unit unit)
    {
        InitializeHighlight();
        IActionCommand command = CommandPlanner.Instance.GetPlannedCommand(unit);
        if (CommandPlanner.Instance.HasPlannedCommand(unit))
        {
            if (command.SkillData != null)
            {
                int index = unit.SkillController.GetSkillIndex(command.SkillData);
                context.HighlightSkillSlotUI?.Invoke(true, index);
                context.SelectedSkill = command.SkillData; // 스킬이 있으면 넣어줌.
            }
            else
            {
                context.HighlightBasicAttackUI?.Invoke(true);
            }

            command.Target?.ToggleSelectedIndicator(true);
        }
    }

    private bool IsPointerOverUI()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return EventSystem.current.IsPointerOverGameObject();
#elif UNITY_ANDROID || UNITY_IOS
    if (Input.touchCount > 0)
    {
        return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
    }
        return false;
#else
        return false;
#endif
    }
}