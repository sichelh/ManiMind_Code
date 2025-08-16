using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class StatusEffectManager : MonoBehaviour
{
    private List<StatusEffect> activeEffects = new();
    private List<TriggerBuff> triggerBuffs = new();
    private StatManager statManager;
    public Unit Owner { get; private set; }

    private void Start()
    {
        statManager = GetComponent<StatManager>();
        Owner = GetComponent<Unit>();
    }

    /// <summary>
    /// 상태 효과를 적용합니다. 중첩 불가능한 효과의 경우 기존 효과와 비교하여 더 강한 효과만 적용됩니다.
    /// </summary>
    /// <param name="effect">적용할 상태 효과</param>
    public void ApplyEffect(StatusEffect effect)
    {
        if (!effect.IsStackable)
        {
            StatusEffect existing = activeEffects.Find(x =>
                x.EffectType == effect.EffectType &&
                x.StatType == effect.StatType &&
                x.ModifierType == effect.ModifierType);
            if (existing != null)
            {
                if (effect.Value > 0 && Mathf.Abs(effect.Value) >= Mathf.Abs(existing.Value))
                {
                    RemoveEffect(existing);
                }
                else
                {
                    return;
                }
            }
        }

        Coroutine co = StartCoroutine(effect.Apply(this));
        effect.CoroutineRef = co;
        activeEffects.Add(effect);
    }

    /// <summary>
    /// 스탯에 버프 효과를 적용합니다.
    /// </summary>
    /// <param name="statType">대상 스탯 타입</param>
    /// <param name="modifierType">수정자 타입</param>
    /// <param name="value">적용할 값</param>
    public void ModifyBuffStat(StatType statType, StatModifierType modifierType, float value)
    {
        statManager.ApplyStatEffect(statType, modifierType, value);
    }

    /// <summary>
    /// 스탯을 회복시키는 효과를 적용합니다.
    /// </summary>
    /// <param name="statType">회복할 스탯 타입</param>
    /// <param name="modifierType">수정자 타입</param>
    /// <param name="value">회복량</param>
    public void RecoverEffect(StatType statType, StatModifierType modifierType, float value)
    {
        statManager.Recover(statType, modifierType, value);
    }

    /// <summary>
    /// 스탯을 소모하는 효과를 적용합니다.
    /// </summary>
    /// <param name="statType">소모할 스탯 타입</param>
    /// <param name="modifierType">수정자 타입</param>
    /// <param name="value">소모량</param>
    public void ConsumeEffect(StatType statType, StatModifierType modifierType, float value)
    {
        statManager.Consume(statType, modifierType, value);
    }

    /// <summary>
    /// 특정 상태 효과를 제거합니다.
    /// </summary>
    /// <param name="effect">제거할 상태 효과</param>
    public void RemoveEffect(StatusEffect effect)
    {
        activeEffects.Remove(effect);
        if (effect.CoroutineRef != null)
        {
            StopCoroutine(effect.CoroutineRef);
        }

        if (effect is TriggerBuff trigger)
        {
            triggerBuffs.Remove(trigger);
        }

        effect.OnEffectRemoved(this);
    }

    /// <summary>
    /// TriggerBuff를 등록해주는 메서드
    /// </summary>
    /// <param name="effect"></param>
    public void RegisterTriggerBuff(TriggerBuff effect)
    {
        triggerBuffs.Add(effect);
    }

    public void TryTriggerAll(TriggerEventType eventType)
    {
        foreach (TriggerBuff trigger in triggerBuffs.ToList())
        {
            trigger.TryTrigger(this, eventType);
        }
    }


    /// <summary>
    /// 모든 상태 효과를 제거합니다.
    /// </summary>
    public void RemoveAllEffects()
    {
        foreach (StatusEffect effect in activeEffects)
        {
            if (effect.CoroutineRef != null)
            {
                StopCoroutine(effect.CoroutineRef);
            }

            effect.OnEffectRemoved(this);
        }

        activeEffects.Clear();
    }

    public void OnTurnPassed()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i] is TurnBasedBuff turnEffect)
            {
                turnEffect.OnTurnPassed(this);
            }
        }
    }
}