using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetailPopupUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillTier;
    [SerializeField] private TextMeshProUGUI skillDescription;

    public void Initialize(SkillData skillData)
    {
        skillIcon.sprite = skillData.skillSo.SkillIcon;
        skillName.text = skillData.skillSo.skillName;
        skillTier.text = $"{skillData.skillSo.activeSkillTier}";
        skillDescription.text = skillData.skillSo.skillDescription;
    }
}
