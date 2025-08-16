using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PassiveTestScene : MonoBehaviour
{
    [SerializeField] private PlayerUnitSO playerUnit;

    [SerializeField] private PassiveSO passiveSkill;
    [SerializeField] private EmotionType emotionType;

    [SerializeField] private Unit unit;

    [SerializeField] private TextMeshProUGUI currentStackTxt;

    private void Start()
    {
        passiveSkill = playerUnit.PassiveSkill;
        GameObject go = Instantiate(playerUnit.UnitPrefab);
        unit = go.GetComponent<Unit>();
        emotionType = passiveSkill.TriggerEmotion;

        UnitSpawnData data = new();
        data.UnitSo = playerUnit;
        data.DeckData = new EntryDeckData(playerUnit.ID);
        unit.Initialize(data);
    }

    // Update is called once per frame
    private void Update()
    {
        currentStackTxt.text = $"{unit.CurrentEmotion.EmotionType} : {unit.CurrentEmotion.Stack}";
    }


    public void OnClickChangeEmotion()
    {
        Debug.Log(emotionType);
        unit.ChangeEmotion(emotionType);
    }

    public void OnClickAddStack()
    {
        unit.CurrentEmotion.AddStack(unit, 1);
    }

    public void OnClickSubtractStack()
    {
        unit.CurrentEmotion.AddStack(unit, -1);
    }
}