// 행동 커맨드 인터페이스

public interface IActionCommand
{
    Unit             Executer  { get; }
    Unit             Target    { get; }
    public SkillData SkillData { get; }

    void Execute();
}

// 기본공격 커맨드
public class AttackCommand : IActionCommand
{
    public Unit Executer { get; }
    public Unit Target   { get; }

    public SkillData SkillData { get; }

    public AttackCommand(Unit executer, Unit target)
    {
        Executer = executer;
        Target = target;
        Executer.ChangeAction(ActionType.Attack);
    }

    // 유닛이 할 행동 커맨드를 저장할 때 유닛에게 반영해줌.
    public void Execute()
    {
        Executer.SetTarget(Target);
    }
}

// 스킬 커맨드
public class SkillCommand : IActionCommand
{
    public Unit      Executer  { get; }
    public Unit      Target    { get; }
    public SkillData SkillData { get; }

    public SkillCommand(Unit executer, Unit target, SkillData skillData)
    {
        Executer = executer;
        Target = target;
        SkillData = skillData;
    }

    // 유닛이 할 행동 커맨드를 저장할 때 유닛에게 반영해줌.
    public void Execute()
    {
        if (Executer is PlayerUnitController player)
        {
            int index = player.SkillController.GetSkillIndex(SkillData);
            player.SkillController.ChangeCurrentSkill(index);
        }

        Executer.ChangeAction(ActionType.Skill);
        Executer.SetTarget(Target);
    }
}