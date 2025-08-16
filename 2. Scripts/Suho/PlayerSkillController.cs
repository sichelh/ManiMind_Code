using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * 플레이어 스킬 컨트롤러
 *
 * 1. ChangeSkill로 사용할 스킬을 정한다.
 *
 * 2. SelectTargets로 MainTareget과 SubTargets을 정한다
 *
 * 3. UseSkill()을 사용한다.
 *
 * 주의!!
 *  - SelectTargets를 먼저 사용할 경우 에러 발생.
 *
 * 현재 EndTurn을 스킬을 사용 후 바로 적용시켜줄 지 혹은 외부에서 호출해줄지 결정 중. [2025/06/24]
 * 스킬 사용 시 효과를 발생시키는 메서드 처리에 대한 고민 중 [2025/06/24]
 *
 */
[System.Serializable]
public class PlayerSkillController : BaseSkillController
{

    public override void SelectSkillSubTargets(IDamageable target)
    {
        SkillSubTargets.Clear();
        if (CurrentSkillData != null)
        {
            TargetSelect targetSelect = new TargetSelect(SkillManager.Owner.Target, SkillManager.Owner);
            foreach (var effectData in CurrentSkillData.Effect.skillEffectDatas)
            {
                SkillSubTargets.Add(effectData, targetSelect.FindTargets(effectData.selectTarget, effectData.selectCamp));
            }
        }
    }

    public override void ChangeCurrentSkill(int index)
    {
        if (index >= skills.Count) return;
        CurrentSkillData = skills[index];
        if (CurrentSkillData == null) return;
        SkillManager.Owner.ChangeClip(Define.SkillClipName, CurrentSkillData.skillSo.skillAnimation);

        // skillAnimationListener.skillData = CurrentSkillData;
    }

    public override void UseSkill()
    {
        if (!CurrentSkillData.CheckCanUseSkill())
        {
            Debug.LogWarning("사용 불가능한 스킬 사용시도");
            return;
        }

        
        CurrentSkillData.coolDown = CurrentSkillData.coolTime;
        CurrentSkillData.reuseCount--;
        CurrentSkillData.skillSo.SkillType.Execute(SkillManager.Owner, SkillManager.Owner.Target);

        // EndTurn();
    }


    public override void EndTurn()
    {
        foreach (SkillData skill in skills)
        {
            if (skill == null || skill == CurrentSkillData)
                continue;
            skill.RegenerateCoolDown(generateCost);
        }

        CurrentSkillData = null;
        this.SkillManager.Owner.SetTarget(null);
    }
}