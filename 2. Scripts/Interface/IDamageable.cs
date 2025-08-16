using System;
using System.Collections;
using UnityEngine;

public interface IDamageable
{
    public Collider    Collider       { get; }
    public BaseEmotion CurrentEmotion { get; }
    public bool        IsDead         { get; }
    public void        TakeDamage(float amount, StatModifierType modifierType = StatModifierType.Base, bool isCritical = false);
    public void        SetLastAttacker(IAttackable attacker);

    public StatusEffectManager StatusEffectManager { get; }


    public void Dead();
    public void ChangeEmotion(EmotionType emotion);
    public void ExecuteCoroutine(IEnumerator damageEffect);
}