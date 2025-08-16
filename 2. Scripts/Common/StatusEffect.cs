using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상태 효과의 기본 클래스. 모든 버프와 디버프의 기초가 되는 추상 클래스
/// </summary>
public abstract class StatusEffect
{
    public int StatusEffectID;
    public StatusEffectType EffectType;
    public StatType StatType;
    public StatModifierType ModifierType;
    public float Value;
    public float Duration;
    public float TickInterval = 1f;
    public Coroutine CoroutineRef;
    public Action ApplyEffect;
    public Action RemoveEffect;
    public List<VFXData> BuffVFX;
    public bool IsStackable;


    public abstract IEnumerator Apply(StatusEffectManager manager);

    public virtual void OnEffectRemoved(StatusEffectManager effect)
    {
    }

    public virtual void ApplyVFX(StatusEffectManager manager, VFXType vfxType)
    {
        if (BuffVFX != null)
        {
            List<PoolableVFX> list = VFXController.VFXListPlay(BuffVFX, vfxType, VFXSpawnReference.Target, manager.Owner, false);
            foreach (PoolableVFX vfx in list)
            {
                ApplyEffect += vfx.OnSpawnFromPool;
                RemoveEffect += vfx.RemoveVFX;
            }
        }
    }
}

/// <summary>
/// 즉시 적용되는 일회성 버프. 스탯을 즉시 증가시키고 효과가 바로 제거됨
/// </summary>
public class InstantBuff : StatusEffect
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        manager.ModifyBuffStat(StatType, ModifierType, Value);
        yield return null;
        manager.RemoveEffect(this);
    }
}

/// <summary>
/// 시간에 따라 지속적으로 적용되는 버프. 설정된 시간 동안 주기적으로 스탯을 증가시킴
/// </summary>
public class OverTimeBuff : StatusEffect
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            manager.ModifyBuffStat(StatType, ModifierType, Value);
            yield return new WaitForSeconds(TickInterval);
            elapsed += TickInterval;
        }

        manager.RemoveEffect(this);
    }
}

/// <summary>
/// 즉시 적용되는 일회성 디버프. 스탯을 즉시 감소시키고 효과가 바로 제거됨
/// </summary>
public class InstantDebuff : StatusEffect
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        manager.ModifyBuffStat(StatType, ModifierType, -Value);
        yield return null;
        manager.RemoveEffect(this);
    }
}

/// <summary>
/// 시간에 따라 지속적으로 적용되는 디버프. 설정된 시간 동안 주기적으로 스탯을 감소시킴
/// </summary>
public class OverTimeDebuff : StatusEffect
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            manager.ModifyBuffStat(StatType, ModifierType, -Value);
            yield return new WaitForSeconds(TickInterval);
            elapsed += TickInterval;
        }

        manager.RemoveEffect(this);
    }
}

/// <summary>
/// 일정 시간 동안 지속되는 버프. 효과가 제거될 때 자동으로 스탯이 원래대로 복구됨
/// </summary>
public class TimedModifierBuff : StatusEffect
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        // 스탯 증가
        manager.ModifyBuffStat(StatType, ModifierType, Value);

        yield return new WaitForSeconds(Duration);

        // 시간 지나면 원래대로 복구
        manager.RemoveEffect(this);
    }

    public override void OnEffectRemoved(StatusEffectManager manager)
    {
        manager.ModifyBuffStat(StatType, ModifierType, -Value);
    }
}

/// <summary>
/// 즉시 회복 효과. 스탯을 즉시 회복시키고 효과가 바로 제거됨
/// </summary>
public class RecoverEffect : StatusEffect
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        manager.RecoverEffect(StatType, ModifierType, Value);
        yield return null;
        manager.RemoveEffect(this);
    }
}

/// <summary>
/// 지속적인 회복 효과. 설정된 시간 동안 주기적으로 스탯을 회복시킴
/// </summary>
public class RecoverOverTime : StatusEffect
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            manager.RecoverEffect(StatType, ModifierType, Value);
            yield return new WaitForSeconds(TickInterval);
            elapsed += TickInterval;
        }

        manager.RemoveEffect(this);
    }
}

/// <summary>
/// 주기적인 데미지 디버프. 설정된 시간 동안 주기적으로 데미지를 입힘 (독, 화상 등)
/// </summary>
public class PeriodicDamageDebuff : StatusEffect
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            manager.Owner.TakeDamage(Value, ModifierType);
            yield return new WaitForSeconds(TickInterval);
            elapsed += TickInterval;
        }

        manager.RemoveEffect(this);
    }
}

public abstract class TurnBasedBuff : StatusEffect
{
    public void OnTurnPassed(StatusEffectManager manager)
    {
        Duration--;
        if (Duration <= 0)
        {
            manager.RemoveEffect(this);
        }
    }
}

public class TurnBasedModifierBuff : TurnBasedBuff
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        manager.ModifyBuffStat(StatType, ModifierType, Value);
        ApplyVFX(manager, VFXType.Buff);
        ApplyEffect?.Invoke();
        yield return null;
    }

    public override void OnEffectRemoved(StatusEffectManager manager)
    {
        manager.ModifyBuffStat(StatType, ModifierType, -Value);
        RemoveEffect?.Invoke();
        ApplyEffect = null;
    }
}

public class StunDebuff : TurnBasedBuff
{
    public override IEnumerator Apply(StatusEffectManager manager)
    {
        manager.Owner.SetStunned(true);
        ApplyVFX(manager, VFXType.Buff);
        ApplyEffect?.Invoke();
        yield return null;
    }

    public override void OnEffectRemoved(StatusEffectManager manager)
    {
        RemoveEffect?.Invoke();
        manager.Owner.SetStunned(false);
    }
}

public class TriggerBuff : TurnBasedBuff
{
    public TriggerEventType TriggerEvent;
    public Func<StatusEffectManager, bool> TriggerCondition;
    public Action<StatusEffectManager> OnTriggered;

    public override IEnumerator Apply(StatusEffectManager manager)
    {
        manager.RegisterTriggerBuff(this);
        yield return null;
    }

    public void TryTrigger(StatusEffectManager manager, TriggerEventType eventType)
    {
        if (eventType != TriggerEvent)
        {
            return;
        }

        if (TriggerCondition?.Invoke(manager) == true)
        {
            OnTriggered?.Invoke(manager);
            manager.RemoveEffect(this);
        }
    }
}

public class TurnBasedPeriodicDamageDebuff : TurnBasedBuff
{
    //배틀 End때 아야하는 느낌
    private StatusEffectManager manager;

    public override IEnumerator Apply(StatusEffectManager manager)
    {
        this.manager = manager;
        BattleManager.Instance.OnTurnEnded += TakeDamage;
        ApplyVFX(manager, VFXType.Dot);
        yield return null;
    }

    public override void OnEffectRemoved(StatusEffectManager effect)
    {
        BattleManager.Instance.OnTurnEnded -= TakeDamage;
        ApplyEffect = null;
        RemoveEffect?.Invoke();
    }

    private void TakeDamage()
    {
        ApplyEffect?.Invoke();
        manager.Owner.TakeDamage(Value, ModifierType);
        //대미지를 처리해준다?
    }

    public override void ApplyVFX(StatusEffectManager manager, VFXType vfxType)
    {
        if (BuffVFX != null)
        {
            List<PoolableVFX> list = VFXController.VFXListPlay(BuffVFX, vfxType, VFXSpawnReference.Target, manager.Owner, false);
            foreach (PoolableVFX vfx in list)
            {
                vfx.AdjustTransform();
                ApplyEffect += vfx.PlayDotVFX;
                RemoveEffect += vfx.RemoveVFX;
            }
        }
    }
}