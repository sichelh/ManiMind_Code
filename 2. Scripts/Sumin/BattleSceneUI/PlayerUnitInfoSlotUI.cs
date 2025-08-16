using UnityEngine;
using UnityEngine.UI;

public class PlayerUnitInfoSlotUI : MonoBehaviour
{
    [Header("HPBar와 커맨드 슬롯")]
    [SerializeField] private PlayerUnitInfoSlotHpBarUI hpBarUI;

    [SerializeField] private GameObject commandSlot;

    [Header("유닛, 스킬, 타겟 이미지")]
    [SerializeField] private Image unitIcon;

    [SerializeField] private Image combatActionIcon;
    [SerializeField] private Image targetIcon;

    [Header("기본공격 아이콘")]
    [SerializeField] private Sprite baseAtkIcon;

    [Header("스킬 팝업")]
    [SerializeField] private SkillDetailPopupUI skillDetailPopup;

    [SerializeField] private SkillForDetailButton skillForDetailButton;

    private IActionCommand command;

    public void UpdateUnitInfo(Unit playerUnit)
    {
        unitIcon.sprite = AtlasLoader.Instance.GetSpriteFromAtlas(AtlasType.CharacterSmall, playerUnit.UnitSo.UnitIcon.name);
    }

    public void UpdateUnitDead()
    {
        Color newColor = unitIcon.color;
        newColor.a = 0.5f;
        unitIcon.color = newColor;
    }

    public void UpdateUnitSelect(Unit playerUnit)
    {
        if (CommandPlanner.Instance.HasPlannedCommand(playerUnit))
        {
            commandSlot.SetActive(true);
            command = CommandPlanner.Instance.GetPlannedCommand(playerUnit);
            if (command.SkillData != null)
            {
                combatActionIcon.sprite = command.SkillData.skillSo.SkillIcon;
                skillDetailPopup.Initialize(command.SkillData);
                skillForDetailButton.SetInteractable(true);
            }
            else
            {
                combatActionIcon.sprite = baseAtkIcon;
                skillForDetailButton.SetInteractable(false);
            }

            targetIcon.sprite = command.Target.UnitSo.UnitIcon;
        }
        else
        {
            commandSlot.SetActive(false);
        }
    }

    public void UpdateHpBar(IDamageable owner)
    {
        hpBarUI.Initialize(owner);
    }
}