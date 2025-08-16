using UnityEngine;
using UnityEngine.UI;

public class MonsterUnitInfoSlotUI : MonoBehaviour
{
    [Header("유닛, 스킬, 타겟 이미지")]
    [SerializeField] private Image MonsterIcon;

    [SerializeField] private Image combatActionIcon;
    [SerializeField] private Image TargetIcon;

    [Header("기본공격 아이콘")]
    [SerializeField] private Sprite baseAtkIcon;

    [Header("스킬 팝업")]
    [SerializeField] private SkillDetailPopupUI skillDetailPopup;

    [SerializeField] private SkillForDetailButton skillForDetailButton;

    public void Initialize(Unit monsterUnit)
    {
        MonsterIcon.sprite = monsterUnit.UnitSo.UnitIcon;
        if (monsterUnit.SkillController.CurrentSkillData != null)
        {
            combatActionIcon.sprite = monsterUnit.SkillController.CurrentSkillData.skillSo.SkillIcon;
            skillDetailPopup.Initialize(monsterUnit.SkillController.CurrentSkillData);
            skillForDetailButton.SetInteractable(true);
        }
        else
        {
            combatActionIcon.sprite = baseAtkIcon;
            skillForDetailButton.SetInteractable(false);
        }

        IDamageable target     = monsterUnit.Target;
        Unit        targetUnit = target as Unit;
        if (targetUnit != null)
        {
            TargetIcon.sprite = targetUnit.UnitSo.UnitIcon;
        }
    }
}