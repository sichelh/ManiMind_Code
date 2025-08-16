public static class BuffFactory
{
    public static StatusEffect CreateBuff(StatusEffectData data)
    {
        StatusEffect effect = data.EffectType switch
        {
            StatusEffectType.InstantBuff           => new InstantBuff(),
            StatusEffectType.OverTimeBuff          => new OverTimeBuff(),
            StatusEffectType.InstantDebuff         => new InstantDebuff(),
            StatusEffectType.OverTimeDebuff        => new OverTimeDebuff(),
            StatusEffectType.TimedModifierBuff     => new TimedModifierBuff(),
            StatusEffectType.Recover               => new RecoverEffect(),
            StatusEffectType.RecoverOverTime       => new RecoverOverTime(),
            StatusEffectType.PeriodicDamageDebuff  => new PeriodicDamageDebuff(),
            StatusEffectType.TurnBasedModifierBuff => new TurnBasedModifierBuff(),
            StatusEffectType.Trigger               => new TriggerBuff(),
            StatusEffectType.TurnBasedPeriodicDamageDebuff => new TurnBasedPeriodicDamageDebuff(),
            StatusEffectType.TurnBasedStunDebuff => new StunDebuff(),
            _                                      => null
        };
        if (effect == null)
            return null;

        effect.StatusEffectID = data.ID;
        effect.StatType = data.Stat.StatType;
        effect.Duration = data.Duration;
        effect.ModifierType = data.Stat.ModifierType;
        effect.Value = data.Stat.Value;
        effect.TickInterval = data.TickInterval;
        effect.BuffVFX = data.VFX;
        return effect;
    }
}