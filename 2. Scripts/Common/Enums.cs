using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    MaxHp,
    CurHp,

    MaxMp,
    CurMp,

    AttackPow,
    Counter,
    Defense,

    Speed,

    CriticalDam,
    CriticalRate,

    HitRate,
    Shield,
    Aggro
}

public enum StatModifierType
{
    Base,
    BuffFlat,
    BuffPercent,
    Equipment,

    BasePercent
}

public enum PlayerUnitState
{
    Idle,
    Move,
    Attack,
    Return,
    Hit,
    Skill,
    Die,
    Victory,
    ReadyAction
}

public enum EnemyUnitState
{
    Idle,
    Move,
    Attack,
    Return,
    Skill,
    Hit,
    Stun,
    Die
}

public enum StatusEffectType
{
    InstantBuff,          //즉발
    OverTimeBuff,         //시간
    InstantDebuff,        // 즉발 디버프
    OverTimeDebuff,       // 시간 디버프
    TimedModifierBuff,    //일정 시간동안 유지되는 (1턴으로 지정)
    PeriodicDamageDebuff, //도트뎀
    Recover,              //회복
    RecoverOverTime,      // 지속 시간 동안 회복
    Damege,               // 즉발 대미지
    TurnBasedModifierBuff,
    Trigger,
    TurnBasedPeriodicDamageDebuff,
    TurnBasedStunDebuff
}

public enum EmotionType
{
    None,       //없음
    Neutral,    // 노말
    Anger,      //분노
    Depression, //우울
    Joy         // 기쁨
}

public enum TriggerEventType
{
    OnAttacked //피격 당했을때
}

public enum ItemType
{
    Equipment
}

public enum EquipmentType
{
    Weapon,
    Armor,
    Accessory
}

/*SelectTargetType : 스킬을 사용할 때 적을 선택하는 로직 타입
 * Single : 단일 타겟 - 메인타겟
 * All : 진영 전체 - 진영 전체가 서브타겟
 * SinglePlusRandomOne : 진영 한 쪽의 단일 한 명 -메인타겟-과 랜덤 한 명 -서브타겟-
 */
public enum SelectTargetType
{
    MainTarget,
    AllExceptMainTarget,
    All,
    OnSelf,
    RandomOneExceptMainTarget,
    Sector
}

/* 선택 가능한 진영
 * Player : Player쪽
 * Enemy : Enemy쪽
 * BothSide : 양 쪽 다
 */
public enum SelectCampType
{
    Colleague,
    Enemy,
    BothSide
}

/*
 *  Male_Warrior : 남자기사
    FeMale_Warrior : 여자기사
    SpearMan : 창술사
    DragonKnight : 용기사
    Archer : 궁수
    Priest : 성직자
    Mage : 마법사
 *
 */

public enum JobType
{
    Male_Warrior,
    FeMale_Warrior,
    SpearMan,
    DragonKnight,
    Archer,
    Priest,
    Mage,
    Monster
}

public enum Tier
{
    A,
    S,
    SR,
    SSR
}

public enum AttackDistanceType
{
    Melee,
    Range
}

public enum ProjectileInterpolationMode
{
    Linear,
    Lerp,
    MoveTowards,
    SmoothDamp,
    Slerp,
    Fall
}

public enum GachaType
{
    Skill,
    Character,
    Equipment
}

public enum VFXSpawnReference
{
    Caster,     // 시전자 위치에 이펙트 발생
    Target,     // 타겟위치에 이펙트 발생
    Projectile, // 투사체위치에 이펙트 발생
    World       // 월드좌표 기준으로 이펙트발생
}

public enum VFXType
{
    Cast, // 스킬애니메이션이 나올때
    Dot,  // 스킬로 인한 DotDamage가 발생할 때마다
    Buff, // 스킬로 인한 Buff, Debuff가 유지되는 동안 반복
    Hit,  // 스킬, 공격으로 인한 SkillEffect가 발생했을 때
    Start // 타임라인에서 호출
}


public enum RewardType
{
    Gold,
    Opal,
    Item,
    Skill,
    Unit
}

//씬 이름 데이터
public enum SceneName
{
    TitleScene,
    DeckBuildingScene,
    DialogueScene,
    BattleScene_Main
}


//항상 추가해야하는 사운드데이터레이블 ( UI사운드 등 )
public enum AlwaysLoad
{
    AlwaysLoadSound
}

public enum Gender
{
    Male,
    Female,
}

public enum MonsterType
{
    None,
    Slime,
    Dotorini,
    Sprout,
    Bee,
    Tree,
    Male,
    Female,
    RabbitSlime
}


public enum VFXBodyPartType
{
    Core,
    Head,
    feet,
    
}