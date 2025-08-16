using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [Header("이미지 / 코스트")]
    [SerializeField] private Image icon;

    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private Button button;

    // 현재 버튼에 할당된 스킬 데이터
    private ActiveSkillSO activeActiveSkill;
    private PassiveSO passiveSkill;

    // 현재 버튼의 역할이 패시브인지 액티브인지
    private bool isPassive;

    // 선택된 스킬인지
    private bool isSelected;

    private Action<SkillButton, bool> onClick;


    public void OnClick()
    {
        onClick?.Invoke(this, isSelected);
    }

    #region

    public ActiveSkillSO GetActiveSkill()
    {
        return activeActiveSkill;
    }

    public PassiveSO GetPassiveSkill()
    {
        return passiveSkill;
    }

    public bool IsPassive => isPassive;

    #endregion
}