using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

/*
 * TargetSelect => 메인타겟을 기준으로 SelectTargetType에 따라 실제로 목표로 정할 타겟들을 리턴해주는 클래스
 * attacker => 스킬을 사용하는 유닛도 타겟으로 지정가능해야 하므로 추가
 */
public class TargetSelect
{
    private IDamageable mainTargetUnit;
    private IAttackable attacker;
    private int column = 3;

    public TargetSelect(IDamageable mainTarget, IAttackable attacker)
    {
        mainTargetUnit = mainTarget;
        this.attacker = attacker;
    }

    public bool IsValidSector(int tempTargetindex, int column, int length)
    {
        if (tempTargetindex / column <= 0)
        {
            return false;
        }

        if (tempTargetindex >= length)
        {
            return false;
        }

        return true;
    }

    public List<IDamageable> TransLateUnitToIDamagable(List<Unit> units)
    {
        List<IDamageable> targets = new();
        foreach (Unit unit in units)
        {
            targets.Add(unit);
        }

        return targets;
    }

    public List<IDamageable> FindTargets(SelectTargetType type, SelectCampType camp)
    {
        List<IDamageable> targets = new();
        if (mainTargetUnit == null)
        {
            return null;
        }

        Unit attackerUnit = attacker as Unit;

        //전부 나를 제외하고 가져옴
        List<Unit> combinedUnits = camp switch
        {
            SelectCampType.Enemy     => BattleManager.Instance.GetEnemies(attackerUnit),
            SelectCampType.Colleague => BattleManager.Instance.GetAllies(attackerUnit),
            SelectCampType.BothSide  => BattleManager.Instance.GetAllUnits(attackerUnit),
            _                        => null
        };

        if (combinedUnits == null)
        {
            return null;
        }

        if (camp == SelectCampType.Colleague || camp == SelectCampType.BothSide)
        {
            combinedUnits.Add(attackerUnit);
        }

        //combineUnits = 


        switch (type)
        {
            case SelectTargetType.MainTarget:
                targets.Add(mainTargetUnit);
                return targets;
            case SelectTargetType.AllExceptMainTarget:
                if (combinedUnits.Count == 0)
                {
                    return targets; // 선택 가능한 유닛이 없을 경우
                }

                return TransLateUnitToIDamagable(combinedUnits);

            case SelectTargetType.RandomOneExceptMainTarget:
                if (combinedUnits.Count == 0)
                {
                    return targets; // 선택 가능한 유닛이 없을 경우
                }

                Unit randomTarget = combinedUnits[Random.Range(0, combinedUnits.Count)];
                targets.Add(randomTarget);
                return targets;

            case SelectTargetType.Sector:
                combinedUnits = camp switch
                {
                    SelectCampType.Enemy     => BattleManager.Instance.EnemyUnits,
                    SelectCampType.Colleague => BattleManager.Instance.PartyUnits,
                    _                        => null
                };
                if (combinedUnits == null)
                {
                    return null;
                }

                int mainTargetIndex       = combinedUnits.IndexOf(mainTargetUnit as Unit);
                int tempTargetIndex       = mainTargetIndex + column - 1;
                int secondTempTargetIndex = mainTargetIndex + column;
                targets.Add(mainTargetUnit);
                if (IsValidSector(tempTargetIndex, column, combinedUnits.Count) && !combinedUnits[tempTargetIndex].IsDead)
                {
                    targets.Add(combinedUnits[tempTargetIndex]);
                }

                if (IsValidSector(secondTempTargetIndex, column, combinedUnits.Count) && !combinedUnits[secondTempTargetIndex].IsDead)
                {
                    targets.Add(combinedUnits[secondTempTargetIndex]);
                }

                return targets;

            case SelectTargetType.OnSelf:
                targets.Add(attacker as IDamageable);
                return targets;

            case SelectTargetType.All:
                targets = TransLateUnitToIDamagable(combinedUnits);
                return targets;


            default:
                return null;
        }
    }
}