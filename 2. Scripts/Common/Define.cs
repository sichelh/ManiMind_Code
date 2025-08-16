using System;
using System.Collections.Generic;
using UnityEngine;

public static class Define
{
    public const int MaxCharacterCount = 4;
    public const float DefenseReductionBase = 100f;


    public static readonly int MoveAnimationHash = Animator.StringToHash("IsMove");
    public static readonly int AttackAnimationHash = Animator.StringToHash("Attack");
    public static readonly int SkillAnimationHash = Animator.StringToHash("Skill");
    public static readonly int DeadAnimationHash = Animator.StringToHash("Dead");
    public static readonly int VictoryAnimationHash = Animator.StringToHash("Victory");
    public static readonly int ReadyActionAnimationHash = Animator.StringToHash("ReadyAction");
    public static readonly int HitAnimationHash = Animator.StringToHash("Hit");

    public static readonly string IdleClipName = "Idle";
    public static readonly string MoveClipName = "Move";
    public static readonly string AttackClipName = "Attack";
    public static readonly string SkillClipName = "Skill";
    public static readonly string DeadClipName = "Die";
    public static readonly string VictoryClipName = "Victory";
    public static readonly string ReadyActionClipName = "ReadyAction";
    public static readonly string HitClipName = "Hit";


    public static readonly Dictionary<int, int> DupeCountByTranscend = new()
    {
        { 0, 10 },
        { 1, 20 },
        { 2, 30 },
        { 3, 40 },
        { 4, 50 }
    };

    public static string GetStatName(StatType statType)
    {
        return statType switch
        {
            StatType.MaxHp        => "최대 HP",
            StatType.AttackPow    => "공격력",
            StatType.Counter      => "반격 확률",
            StatType.Defense      => "방어력",
            StatType.Speed        => "속도",
            StatType.CriticalRate => "치명타 확률",
            StatType.CriticalDam  => "치명타 대미지",
            StatType.HitRate      => "명중률",
            _                     => string.Empty
        };
    }

    // 티어 별 가챠 확률
    public static readonly Dictionary<Tier, float> TierRates = new()
    {
        { Tier.A, 90f },
        { Tier.S, 9f },
        { Tier.SR, 0.8f },
        { Tier.SSR, 0.2f }
    };

    // 티어 별 중복 보상 값 계수
    public static float GetCompensationAmount(Tier tier)
    {
        return tier switch
        {
            Tier.A   => 0.1f,
            Tier.S   => 0.2f,
            Tier.SR  => 0.3f,
            Tier.SSR => 0.4f,
            _        => 0
        };
    }

    // 가챠 비용
    public static readonly Dictionary<GachaType, int> GachaDrawCosts = new()
    {
        { GachaType.Skill, 200 },
        { GachaType.Character, 150 },
        { GachaType.Equipment, 150 }
    };


    public static float GetTargetColliderRadius(IDamageable target)
    {
        Collider col = target?.Collider;

        if (col is CapsuleCollider capsule)
        {
            return capsule.radius;
        }
        else
        {
            return 0.5f; // 기본값
        }
    }


    public static Dictionary<int, string> ChapterNameDictionary =
        new()
        {
            { 1, "희망의 초원" },
            { 2, "위안의 평야" },
            { 3, "분노의 광산" },
            { 4, "고독의 사막" }
        };

    public static readonly int RequierCombineItemGold = 3000;
    public static readonly int RequierUnitLevelUpGold = 1000;
    public static readonly int RequierUnitTranscendGold = 10000;


    public static Dictionary<string, Action> FirstGivenItem = new()
    {
        { "Unit", () => AccountManager.Instance.AddPlayerUnit(TableManager.Instance.GetTable<PlayerUnitTable>().GetDataByID(101)) },
        { "Item", () => InventoryManager.Instance.AddItem(new EquipmentItem(11101)) },
        { "Skill", () => AccountManager.Instance.AddSkill(TableManager.Instance.GetTable<ActiveSkillTable>().GetDataByID(1002), out _) }
    };

    public static Gender GetGender(JobType jobType)
    {
        switch (jobType)
        {
            case JobType.Archer:
            case JobType.Male_Warrior: return Gender.Male;
            case JobType.FeMale_Warrior:
            case JobType.DragonKnight:
            case JobType.Priest:
            case JobType.SpearMan:
            case JobType.Mage:
            default: return Gender.Female;
        }
    }
}