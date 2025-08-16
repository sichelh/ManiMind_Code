using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StatManager : MonoBehaviour
{
    public Dictionary<StatType, StatBase> Stats { get; private set; } = new();

    public IDamageable Owner { get; private set; }


    /// <summary>
    /// 스탯 매니저를 초기화하고 기본 스탯들을 설정하는 메서드
    /// </summary>
    /// <param name="statProvider">스탯 정보를 제공하는 객체</param>
    /// <param name="owner">스탯의 소유자</param>
    public void Initialize(IStatProvider statProvider, IDamageable owner, List<EquipmentItem> items, int level, IIncreaseStat increaseStat)
    {
        Owner = owner;

        Dictionary<StatType, float> equipmentStatMap = new();
        Dictionary<StatType, float> levelStatMap     = new();

        foreach (EquipmentItem item in items)
        {
            foreach (StatData stat in item.EquipmentItemSo.Stats)
            {
                if (!equipmentStatMap.ContainsKey(stat.StatType))
                {
                    equipmentStatMap[stat.StatType] = 0;
                }

                equipmentStatMap[stat.StatType] += stat.Value;
            }
        }

        if (level > 1)
        {
            foreach (StatData stat in increaseStat.Stats)
            {
                if (!levelStatMap.ContainsKey(stat.StatType))
                {
                    levelStatMap[stat.StatType] = 0;
                }

                levelStatMap[stat.StatType] += (stat.Value * level) - 1;
            }
        }

        foreach (StatData baseStat in statProvider.Stats)
        {
            float finalValue = baseStat.Value;

            if (equipmentStatMap.TryGetValue(baseStat.StatType, out float equipBonus))
            {
                finalValue += equipBonus;
            }

            if (levelStatMap.TryGetValue(baseStat.StatType, out float levelBonus))
            {
                finalValue += levelBonus;
            }

            Stats[baseStat.StatType] = BaseStatFactory(baseStat.StatType, finalValue);
        }
    }

    //Initialize
    public void Initialize(IStatProvider statProvider, IDamageable owner, int level, IIncreaseStat increaseStat)
    {
        Owner = owner;
        foreach (StatData stat in statProvider.Stats)
        {
            float finalStatValue = stat.Value;
            foreach (StatData data in increaseStat.Stats)
            {
                if (data.StatType == stat.StatType && level > 1) //1일땐 적용 안되도록
                {
                    finalStatValue += (data.Value * level) - 1;
                }
            }

            Stats[stat.StatType] = BaseStatFactory(stat.StatType, finalStatValue);
        }
    }

    /// <summary>
    /// Stat을 생성해주는 팩토리
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private StatBase BaseStatFactory(StatType type, float value)
    {
        return type switch
        {
            StatType.CurHp  => new ResourceStat(type, Stats[StatType.MaxHp].Value, Stats[StatType.MaxHp].Value),
            StatType.CurMp  => new ResourceStat(type, value, value),
            StatType.Shield => new ResourceStat(type, value, int.MaxValue),
            ///////////////////////////////////////////////////////////////////////////////////
            _ => new CalculatedStat(type, value)
        };
    }

    /// <summary>
    /// 특정 타입의 스탯을 가져오는 메서드
    /// </summary>
    /// <typeparam name="T">반환할 스탯의 타입</typeparam>
    /// <param name="type">가져올 스탯의 종류</param>
    /// <returns>요청한 타입의 스탯 객체</returns>
    public T GetStat<T>(StatType type) where T : StatBase
    {
        return Stats[type] as T;
    }

    /// <summary>
    /// 특정 스탯의 현재 값을 반환하는 메서드
    /// </summary>
    /// <param name="type">값을 확인할 스탯의 종류</param>
    /// <returns>해당 스탯의 현재 값</returns>
    public float GetValue(StatType type)
    {
        return Stats[type].GetCurrent();
    }

    /// <summary>
    /// 리소스 타입 스탯(HP, MP 등)을 회복시키는 메서드
    /// </summary>
    /// <param name="statType">회복할 스탯의 종류</param>
    /// <param name="modifierType">회복량 적용 방식(기본값 또는 퍼센트)</param>
    /// <param name="value">회복량</param>
    public void Recover(StatType statType, StatModifierType modifierType, float value)
    {
        if (Stats[statType] is ResourceStat res)
        {
            if (res.CurrentValue < res.MaxValue)
            {
                switch (statType)
                {
                    case StatType.CurHp:
                        DamageFontManager.Instance.SetDamageNumber(Owner, value, DamageType.Heal);
                        break;
                }

                switch (modifierType)
                {
                    case StatModifierType.Base:
                        res.Recover(value);
                        break;
                    case StatModifierType.BasePercent:
                        res.RecoverPercent(value);
                        break;
                }

                Debug.Log($"Recover : {statType} : {value} RemainValue: {res.CurrentValue}");
            }
        }
    }

    /// <summary>
    /// 리소스 타입 스탯(HP, MP 등)을 소모시키는 메서드
    /// </summary>
    /// <param name="statType">소모할 스탯의 종류</param>
    /// <param name="modifierType">소모량 적용 방식(기본값 또는 퍼센트)</param>
    /// <param name="value">소모량</param>
    public void Consume(StatType statType, StatModifierType modifierType, float value)
    {
        if (Stats[statType] is ResourceStat res)
        {
            if (res.CurrentValue > 0)
            {
                switch (modifierType)
                {
                    case StatModifierType.Base:
                        value = Mathf.Round(value);
                        res.Consume(value);
                        break;
                    case StatModifierType.BasePercent:
                        res.ConsumePercent(value);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 증가되는 스탯에 따라 해당 스탯을 증감시켜주는 메서드
    /// </summary>
    /// <param name="type">증감 시켜줄 스탯의 종류</param>
    /// <param name="valueType">적용 방식(베이스, 버프(상수 or 퍼센트),장비)</param>
    /// <param name="value">적용 값</param>
    public void ApplyStatEffect(StatType type, StatModifierType valueType, float value)
    {
        if (Stats[type] is not CalculatedStat stat)
        {
            value = Mathf.Round(value);
            if (value >= 0)
            {
                Recover(type, valueType, value);
            }
            else
            {
                Consume(type, valueType, -value);
            }

            return;
        }

        switch (valueType)
        {
            case StatModifierType.Base:
                stat.ModifyBaseValue(value);
                break;
            case StatModifierType.BuffFlat:
                stat.ModifyBuffFlat(value);
                break;
            case StatModifierType.BuffPercent:
                stat.ModifyBuffPercent(value);
                break;
            case StatModifierType.Equipment:
                stat.ModifyEquipmentValue(value);
                break;
        }

        switch (type)
        {
            case StatType.MaxHp:
                SyncCurrentWithMax(StatType.CurHp, stat);
                break;
            case StatType.MaxMp:
                SyncCurrentWithMax(StatType.CurMp, stat);
                break;
        }

        Debug.Log($"Stat : {type} Modify Value {value}, FinalValue : {stat.Value}");
    }

    /// <summary>
    /// ResourceStat의 Max값을 동기화 시켜주는 메서드
    /// </summary>
    /// <param name="curStatType"></param>
    /// <param name="stat"></param>
    private void SyncCurrentWithMax(StatType curStatType, CalculatedStat stat)
    {
        if (Stats.TryGetValue(curStatType, out StatBase res) && res is ResourceStat curStat)
        {
            curStat.SetMax(stat.FinalValue);
        }
    }
}