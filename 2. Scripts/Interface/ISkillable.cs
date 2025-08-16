using System.Collections.Generic;

public interface ISkillable
{
    BaseSkillController SkillController { get; }
    SkillManager        SkillManager    { get; }
    List<IDamageable>   SubTargets      { get; }
}