using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;


/* 스킬 데이터 ScriptableObject
 * skillName : 스킬이름
 * skillDescription : 스킬 설명
 * skillType : 스킬의 타입 (Melee - 근접, Range - 투사체 발사)
    - 스킬의 타입에 따라 유닛의 턴에서 행동하는 패턴이 바뀜
    - Melee 일 경우 StartTurn => MoveTo(타겟에게) => Act => EndTurn
    - Range 인 경우 StartTurn => Act => EndTurn
 * StatBaseSkillEffect : 스킬의 대상, 데미지, 효과를 주는 클래스
 * reuseMaxCount : 재사용가능횟수
 * coolTime : 쿨타임
 * skillIcon : 스킬아이콘
 * skillAnimation : 해당 스킬의 애니메이션 클립
 * skillTimeLine : 스킬 연출 효과 타임라인
 */
[CreateAssetMenu(fileName = "New ActiveSkillData", menuName = "ScriptableObjects/New ActiveSkillData")]
public class ActiveSkillSO : SkillSo
{
    public CombatActionSo skillType;
    public SelectCampType selectCamp;
    public StatBaseEffect effect;
    public Tier activeSkillTier;
    public int reuseMaxCount;
    public int coolTime;
    public AnimationClip skillAnimation;
    public PlayableAsset skillTimeLine;
    public bool isSkillScene = false;
    public SFXName SFX;
    public SFXName CastSFX;

    public CombatActionSo SkillType { get; private set; }

    public void CloneSkillType()
    {
        SkillType = Instantiate(skillType);
        if (SkillType is RangeSkillSO rangeSkillSo)
        {
            bool isHasProjectileSkill = effect.skillEffectDatas.Any(p => p.projectilePrefab != null || !string.IsNullOrWhiteSpace(rangeSkillSo.projectilePoolID));
            rangeSkillSo.SetIsProjectile(isHasProjectileSkill);
        }
    }


    public string GetDescription()
    {
        return skillDescription;
    }
}