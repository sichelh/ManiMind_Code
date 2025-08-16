//디버프 주는 class 하나

using UnityEngine;

[System.Serializable]
public class StatBaseBuffSkillEffect
{
    [Header("영향을 주는 스텟")]
    public StatType ownerStatType; // 사용자에 의해 강해지거나 약해지는 사용자의 스텟타입 => ex) 남기사 실드스킬에서 남기사의 최대체력

    [Header("가중치와 고정값")]
    public float weight; // 계수 => ownerStatType의 value값에서 몇 배율을 적용할 것인가, weight = 0.1이면 value * 0.1 

    public float value; // 기본 고정 값 : weight와 관계없이 고정으로 부여할 수 있는 값

    [Header("영향을 받는 스텟")]
    public StatType opponentStatType; // 영향을 받는 스텟의 타입 => ex) 공격 스킬을 사용하면 상대의 curHP가 영향을 받음

    [Header("지속 턴")]
    public int lastTurn = 1; // 영향이 지속되는 턴 ( 턴 마다 지속되는 도트데미지, 디버프 등)

    [Header("받는 디버프의 타입")]
    public StatusEffectType statusEffectType;

    public StatModifierType modifierType;

    [Header("대상의 감정을 바꾸는 공격")]
    public EmotionType emotionType;

    [HideInInspector] public StatusEffectData StatusEffect; //디버프
}