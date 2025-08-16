using System;
using System.Collections;
using UnityEngine;

public class RangeCombatAction : ICombatAction
{
    private readonly RangeActionSo attackData;
    private readonly IDamageable target;

    public event Action OnActionComplete;

    private bool isTimeLinePlaying;

    public RangeCombatAction(RangeActionSo so, IDamageable target)
    {
        attackData = so;
        this.target = target;
    }

    public void Execute(Unit attacker)
    {
    }
}