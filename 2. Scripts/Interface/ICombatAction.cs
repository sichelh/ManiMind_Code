using System;

public interface ICombatAction
{
    event Action OnActionComplete;
    void         Execute(Unit attacker);
}