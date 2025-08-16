using UnityEngine;

public interface IAttackable
{
    public Collider     Collider        { get; }
    BaseSkillController SkillController { get; }
    public BaseEmotion  CurrentEmotion  { get; }
    public StatManager  StatManager     { get; }
    public IDamageable  Target          { get; }
    public void         Attack();

    public void SetTarget(IDamageable target);
}