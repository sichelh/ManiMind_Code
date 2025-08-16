using UnityEngine;

/* EnemySkillContorller => 적들의 스킬사용 로직을 담당하는 클래스
 * selector => 가중치의 값과 유효한 엔트리인 지 확인하는 클래스
 */

public class EnemySkillContorller : BaseSkillController
{
    public WeightedSelector<SkillData> skillSelector;


    /*
     * SelectTargets 메서드 => 적의 mainTarget을 정하는 메서드
     */
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

    /*
     * mainTarget을 기반으로 스킬을 사용하는 메서드
     * 플레이어 스킬 컨트롤러와 로직이 동일하다.
     */
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

    /*
     * 사용할 스킬을 선택하는 로직
     * selector에서 사용가능한 스킬과 각 스킬의 가중치에 따라서 사용할 스킬의 index를 반환
     */
    public void WeightedSelectSkill()
    {
        ChangeCurrentSkill(skillSelector.Select());
    }


    /*
     * 처음에 스킬을 선택하는 로직클래스인 selector를 초기화 시켜주는 메서드
     * 현재 몬스터의 SO를 확인하여 스킬 관련 데이터를 갖고 초기화가 이루어진다.
     */
    public void InitSkillSelector()
    {
        skillSelector = new WeightedSelector<SkillData>();
        EnemyUnitSO MonsterSo = SkillManager.Owner.UnitSo as EnemyUnitSO;
        if (MonsterSo == null) return;
        for (int i = 0; i < skills.Count; i++)
        {
            int index = i; // 캡처할 새로운 지역 변수
            var skill = skills[index];

            skillSelector.Add(
                skill,
                () => MonsterSo.SkillDatas[index].individualProbability,
                () => skill.CheckCanUseSkill()
            );
        }
    }


    /*
     * EndTurn메서드 => 스킬 사용 이후 종료로직
     * 스킬의 쿨다운을 미리 재생 ( 현재 턴에서 어떤 스킬을 사용했는 지 알기 위함 )
     * 현재 사용한 스킬과 타겟들을 모두 초기화
     */
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

    /*
     * ChangeCurrentSkill => 현재 사용할 스킬을 인덱스값을 통해 바꿔준다.
     * 스킬을 바꿔주면 애니메이션 클립을 스킬데이터에 등록되어있는 클립으로 바꾼다.
     */
    public override void ChangeCurrentSkill(int index)
    {
        CurrentSkillData = skills[index];
        if (CurrentSkillData == null)
            return;

        SkillManager.Owner.ChangeClip(Define.SkillClipName, CurrentSkillData.skillSo.skillAnimation);

        // skillAnimationListener.skillData = CurrentSkillData;
    }

    /*
     * ChangeCurrentSkill => 현재 사용할 스킬을 스킬데이터를 통해 바꿔준다.
     * 스킬을 바꿔주면 애니메이션 클립을 스킬데이터에 등록되어있는 클립으로 바꾼다.
     */

    public void ChangeCurrentSkill(SkillData skill)
    {
        CurrentSkillData = skill;
        if (CurrentSkillData == null)
            return;
        SkillManager.Owner.ChangeClip(Define.SkillClipName, CurrentSkillData.skillSo.skillAnimation);
    }

    public SkillData GetCurrentSkillData()
    {
        return CurrentSkillData;
    }
}