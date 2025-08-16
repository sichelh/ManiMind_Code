using System;

public static class CombatActionFactory
{
    public static ICombatAction Create(Unit unit)
    {
        
        return unit.CurrentAttackAction.DistanceType  switch
        {
            AttackDistanceType.Melee    => new MeleeCombatAction(),
            AttackDistanceType.Range    => new RangeCombatAction(unit.CurrentAttackAction.ActionSo as RangeActionSo, unit.Target),
            _ => throw new InvalidOperationException("Invalid Action Type")
        };

    }
}