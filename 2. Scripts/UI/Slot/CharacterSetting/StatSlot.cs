using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatSlot : MonoBehaviour
{
    [SerializeField] private StatType statType;
    [SerializeField] private TextMeshProUGUI statName;
    [SerializeField] private TextMeshProUGUI statValue;

    private float equipmentStatValue;
    public StatType StatType => statType;

    private void Start()
    {
        statName.text = Define.GetStatName(statType);
    }

    public void Initialize(float statValue, float equipValue)
    {
        equipmentStatValue = equipValue;
        UpdateStatValue(statValue);
    }

    public void Initialize(StatType type, float statValue)
    {
        statType = type;
        statName.text = Define.GetStatName(type);
        UpdateStatValue(statValue);
    }

    public void UpdateStatValue(float value)
    {
        Debug.Log($"StatType : {Define.GetStatName(statType)}, baseStat : {value} , equipValue {equipmentStatValue}");


        string statvalue = equipmentStatValue == 0 ? $"{value:N2}" : $"{value:N2} (+{equipmentStatValue:N2})";
        statValue.text = statvalue;
    }
}