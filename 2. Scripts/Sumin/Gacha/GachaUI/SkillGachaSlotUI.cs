using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillGachaSlotUI : MonoBehaviour
{
    [SerializeField] private Image skillIamge;
    [SerializeField] private Image skillTierFrame;
    [SerializeField] private List<GameObject> skillGradeStars;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private GameObject duplicatedGO;
    [SerializeField] private TextMeshProUGUI duplicatedText;
    [SerializeField] private List<Sprite> itemGradeSprites;
    [SerializeField] private Image jobTypeImage;
    [SerializeField] private List<Sprite> jobTypeSprites;

    // 스킬 슬롯 내용 업데이트
    public void Initialize(ActiveSkillSO skill)
    {
        duplicatedGO.SetActive(false);
        skillIamge.sprite = skill.SkillIcon;
        skillNameText.text = skill.skillName;
        skillTierFrame.sprite = itemGradeSprites[(int)skill.activeSkillTier];
        for (int i = 0; i < skillGradeStars.Count; i++)
        {
            skillGradeStars[i].SetActive(i <= (int)skill.activeSkillTier);
        }
        jobTypeImage.sprite = jobTypeSprites[(int)skill.jobType];
    }

    // 스킬 슬롯 중복 보상 업데이트
    public void ShowCompensation(int compensation)
    {
        duplicatedGO.SetActive(true);
        duplicatedText.text = $"{compensation} Opal";
    }
}