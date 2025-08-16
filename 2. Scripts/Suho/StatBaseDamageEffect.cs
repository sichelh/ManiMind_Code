//대미지 주는 class 하나

using System.Collections;
using UnityEngine;

[System.Serializable]
public class StatBaseDamageEffect
{
    [Header("공격 횟수")]
    public int attackCount = 1; // 공격 횟수 => weight * attackCount = 실제 weight

    private float attackDelay = 0.2f;

    [Header("어떤 스텟을 기준으로 데미지를 줄까")]
    public StatType ownerStatType; // 사용자에 의해 강해지거나 약해지는 사용자의 스텟타입 => ex) 남기사 실드스킬에서 남기사의 최대체력

    [Header("기준스텟에 대한 가중치와 고정 값")]
    public float weight; // 계수 => ownerStatType의 value값에서 몇 배율을 적용할 것인가, weight = 0.1이면 value * 0.1 

    public float value; // 기본 고정 값 : weight와 관계없이 고정으로 부여할 수 있는 값

    // 데미지 계산 식 => value + (weight * attackCount * statValue) 
    public void DamageEffect(IDamageable target, Unit owner)
    {
        float finalValue = value + (weight * owner.StatManager.GetValue(ownerStatType));
        target.TakeDamage(finalValue);
    }

    public IEnumerator DamageEffectCoroutine(IDamageable target, Unit owner)
    {
        int currentCount = 0;

        while (currentCount < attackCount)
        {
            DamageEffect(target, owner);
            currentCount++;
            // yield return null;

            // yield return new WaitForSeconds(attackDelay);
        }

        currentCount = 0;
        yield return null;
    }
}