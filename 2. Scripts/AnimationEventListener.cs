using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimationEventListener : MonoBehaviour
{
    private Unit owner;
    private SkillData skillData;

    public void Initialize(Unit unit)
    {
        owner = unit;
        skillData = owner.SkillController.CurrentSkillData;
    }

    public void EventTrigger()
    {
        if (owner.IsCounterAttack)
        {
            Attack();
            return;
        }

        if (owner.CurrentAction == ActionType.Attack)
        {
            Attack();
        }
        else if (owner.CurrentAction == ActionType.Skill)
        {
            // owner.SkillController.CurrentSkillData.skillSo.skillType.PlayVFX(owner,owner.Target);
            UseSkill();
        }
    }


    private void Attack()
    {
        owner.Attack();
    }

    private void UseSkill()
    {
        owner.UseSkill();
    }
}