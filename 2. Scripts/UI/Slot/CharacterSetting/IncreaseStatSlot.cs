using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IncreaseStatSlot : MonoBehaviour
{
    [SerializeField] private StatType statType;
    [SerializeField] private TextMeshProUGUI statName;
    [SerializeField] private TextMeshProUGUI statValueTxt;
    [SerializeField] private TextMeshProUGUI nextValueTxt;

    public StatType StatType => statType;

    public void SetStatSlot(float value, float nextValue)
    {
        statName.text = Define.GetStatName(statType);
        UpdateStatValue(value, nextValue);
    }

    private void UpdateStatValue(float value, float nextValue)
    {
        statValueTxt.text = $"{value:N0}";
        nextValueTxt.text = $"{nextValue:N0}";
    }
}