using System;
using System.Collections.Generic;
using UnityEngine;

/* 스킬을 사용하는 클래스
 * mainTarget => 선택한 메인 타겟 유닛
 * targets => 메인 타겟 유닛을 기반으로 선택된 진짜 target유닛들
    - 실제로 효과를 받는 유닛들
 * generateCost => 스킬들의 쿨다운을 감소시켜주는 값, 이번 턴에 사용한 스킬의 경우에는 쿨타운이 감소하지않음.
 */
[RequireComponent(typeof(SkillManager))]
public abstract class BaseSkillController : MonoBehaviour
{
    public List<SkillData> skills = new();
    public SkillData CurrentSkillData;
    public Dictionary<SkillEffectData, List<IDamageable>> SkillSubTargets = new();
    public int generateCost = 1; // 턴 종료 시 coolDown을 1감소, defalut 값 = 1
    public SkillManager SkillManager { get; set; }

    public bool IsCurrentSkillProjectile
    {
        get
        {
            ActiveSkillSO so = CurrentSkillData?.skillSo as ActiveSkillSO;
            if (so == null)
            {
                return false;
            }

            bool byEffect = so.effect != null && so.effect.IsProjectileSkill;
            bool byType   = (so.skillType as RangeSkillSO)?.IsProjectile ?? false;

            return byEffect || byType;
        }
    }

    public void Initialize(SkillManager manager)
    {
        SkillManager = manager;
    }

    public abstract void SelectSkillSubTargets(IDamageable mainTarget);

    public abstract void UseSkill();
    public abstract void EndTurn();

    public virtual void ChangeCurrentSkill(int index) { }

    public bool CheckAllSkills()
    {
        foreach (SkillData skill in skills)
        {
            if (skill == null)
            {
                continue;
            }

            if (skill.CheckCanUseSkill())
            {
                return true;
            }
        }

        return false;
    }

    // 인덱스 -> 스킬 데이터
    public SkillData GetSkillData(int index)
    {
        return skills[index];
    }

    // 스킬 데이터 -> 인덱스
    public int GetSkillIndex(SkillData skill)
    {
        return skills.FindIndex(s => s == skill);
    }
}