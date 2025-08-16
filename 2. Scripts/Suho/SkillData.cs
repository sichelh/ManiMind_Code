using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveSkillData
{
    public int Id;
    public bool IsEquipped;

    public SaveSkillData(SkillData skillData)
    {
        Id = skillData.skillSo.ID;
        IsEquipped = skillData.IsEquipped;
    }

    public SaveSkillData(int id)
    {
        Id = id;
        IsEquipped = false;
    }

    public SaveSkillData() { }

    public SkillData ToRuntime()
    {
        return new SkillData(this);
    }
}

public class SkillData
{
    public SkillData(ActiveSkillSO skillSo)
    {
        this.skillSo = skillSo;
        Effect = skillSo.effect;
        reuseCount = skillSo.reuseMaxCount;
        coolDown = 0;
        coolTime = skillSo.coolTime;
    }

    public SkillData(SaveSkillData saveData)
    {
        skillSo = TableManager.Instance.GetTable<ActiveSkillTable>().GetDataByID(saveData.Id);
        Effect = skillSo.effect;
        reuseCount = saveData.IsEquipped ? skillSo.reuseMaxCount : 0;
        coolDown = 0;
        coolTime = skillSo.coolTime;
        IsEquipped = saveData.IsEquipped;
    }

    public ActiveSkillSO skillSo;
    public StatBaseEffect Effect;
    public int reuseCount;
    public int coolDown = 0;
    public int coolTime;


    public EntryDeckData EquippedUnit;
    public bool IsEquipped { get; private set; }

    public void RegenerateCoolDown(int value)
    {
        coolDown = Mathf.Max(0, coolDown - value);
    }

    public bool CheckCanUseSkill()
    {
        if (0 < coolDown)
        {
            return false;
        }

        if (reuseCount <= 0)
        {
            return false;
        }

        return true;
    }

    public void EquippedSkill(EntryDeckData unit)
    {
        IsEquipped = true;
        EquippedUnit = unit;
    }

    public void UnEquippedSkill()
    {
        IsEquipped = false;
        EquippedUnit = null;
    }
}