using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SkillManager : MonoBehaviour
{
    public List<ActiveSkillSO> selectedSkill;
    public Unit Owner { get; private set; }


    public void InitializeSkillManager(Unit unit)
    {
        Owner = unit;
        Owner.SkillController.Initialize(this);
        foreach (ActiveSkillSO activeSkillSo in selectedSkill)
        {
            if (activeSkillSo == null)
            {
                Owner.SkillController.skills.Add(null);
                continue;
            }

            SkillData skillData = new(activeSkillSo);
            skillData.skillSo = activeSkillSo;
            skillData.Effect.Init();
            Owner.SkillController.skills.Add(skillData);
        }
    }

    public void AddActiveSkill(ActiveSkillSO skill)
    {
        selectedSkill.Add(skill);
    }
}