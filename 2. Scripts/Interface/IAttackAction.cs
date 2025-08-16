public interface IAttackAction
{
    AttackDistanceType DistanceType { get; }
    CombatActionSo     ActionSo     { get; }
}