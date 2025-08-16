// 인풋에서 관여하는 데이터, Delegate
using System;
using UnityEngine;

public class InputContext
{
    public ISelectable SelectedExecuter;
    public SkillData SelectedSkill;
    public LayerMask TargetLayer;
    public ISelectable SelectedTarget;

    public LayerMask UnitLayer;
    public LayerMask PlayerUnitLayer;
    public LayerMask EnemyUnitLayer;

    public Action<Unit> OpenSkillUI;
    public Action CloseSkillUI;
    public Action DisableStartButtonUI;
    public Action EnableStartButtonUI;
    
    public Action<Unit, Unit, SkillData> PlanActionCommand;

    public Action<bool, int> HighlightSkillSlotUI;
    public Action<bool> HighlightBasicAttackUI;
}
