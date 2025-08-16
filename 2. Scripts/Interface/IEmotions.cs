public interface IEmotionOnAttack
{
    void OnBeforeAttack(Unit attacker, ref IDamageable target);
}

public interface IEmotionOnTakeDamage
{
    void OnBeforeTakeDamage(Unit unit, out bool ignoreDamage);
}

public interface IEmotionOnHitChance
{
    void OnCalculateHitChance(Unit unit, ref float hitRate);
}

/// <summary>
/// 감정의 상태가 특정 스탯에 영향을 줄 때 호출이 됨
/// </summary>
public interface IEmotionStatModifier
{
}