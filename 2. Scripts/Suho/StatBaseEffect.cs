using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatBaseEffect
{
    public List<SkillEffectData> skillEffectDatas;
    public bool IsProjectileSkill { get; private set; }

    public void Init()
    {
        foreach (SkillEffectData effect in skillEffectDatas)
        {
            if (!IsProjectileSkill &&
                (effect.projectilePrefab != null || !string.IsNullOrWhiteSpace(effect.projectilePoolID)))
            {
                IsProjectileSkill = true; // 하나라도 맞으면 true
            }

            foreach (StatBaseBuffSkillEffect buffSkillEffect in effect.buffEffects)
            {
                buffSkillEffect.StatusEffect = new StatusEffectData();
                {
                    //불변 데이터

                    buffSkillEffect.StatusEffect.EffectType = buffSkillEffect.statusEffectType;
                    buffSkillEffect.StatusEffect.Duration = buffSkillEffect.lastTurn;
                    buffSkillEffect.StatusEffect.Stat = new StatData();
                    buffSkillEffect.StatusEffect.Stat.StatType = buffSkillEffect.opponentStatType;
                    buffSkillEffect.StatusEffect.Stat.ModifierType = buffSkillEffect.modifierType;
                    buffSkillEffect.StatusEffect.VFX = effect.skillVFX;
                }
            }
        }
    }
}