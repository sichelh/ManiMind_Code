using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour, IReuseScrollData<SkillData>
{
    public int DataIndex { get; private set; }


    [SerializeField] private Image skillIcon;
    [SerializeField] private Image skillTier;
    [SerializeField] private List<GameObject> skillGradeStars;
    [SerializeField] private Image itemEquipmentImg;
    [SerializeField] private Image itemEquippedUnitIconImg;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private GameObject selectedSlotImg;
    [SerializeField] private GameObject emptySkillImg;
    [SerializeField] private List<Sprite> itemGradeSprites;
    [SerializeField] private Image jobTypeImage;

    public SkillData SkillData { get; private set; }
    private ActiveSkillSO activeSkillSo;
    private PassiveSO passiveSo;

    public event Action<SkillData> OnClickSlot;
    private Action<SkillSlot> onClickCallback;


    public void SetSkillIcon(SkillData skillData, bool isHide)
    {
        if (skillData == null)
        {
            EmptySlot(isHide);
            return;
        }

        gameObject.SetActive(true);
        emptySkillImg.SetActive(false);

        activeSkillSo = skillData.skillSo;
        ShowEquipMark(skillData.IsEquipped);
        if (skillData.IsEquipped && skillData.EquippedUnit != null && skillData.skillSo is ActiveSkillSO)
        {
            itemEquippedUnitIconImg.sprite = skillData.EquippedUnit.CharacterSo.UnitCircleIcon;
        }

        skillTier.gameObject.SetActive(true);
        skillTier.sprite = itemGradeSprites[(int)skillData.skillSo.activeSkillTier];
        skillIcon.gameObject.SetActive(true);
        skillIcon.sprite = activeSkillSo.SkillIcon;
        jobTypeImage.sprite = AtlasLoader.Instance.GetSpriteFromAtlas(AtlasType.JobOrCharacterIcon, $"{(int)activeSkillSo.jobType}_icon");
        skillName.text = activeSkillSo.skillName;


        gameObject.name = $"SkillSlot_{skillData.skillSo.ID}";

        for (int i = 0; i < skillGradeStars.Count; i++)
        {
            skillGradeStars[i].SetActive(i <= (int)skillData.skillSo.activeSkillTier);
        }


        SkillData = skillData;
    }


    public void SetSkillIcon(PassiveSO skillSo, bool isHide)
    {
        if (skillSo == null)
        {
            EmptySlot(isHide);
            return;
        }

        gameObject.SetActive(true);
        emptySkillImg.SetActive(false);

        passiveSo = skillSo;
        ShowEquipMark(false);
        skillTier.gameObject.SetActive(true);
        skillIcon.gameObject.SetActive(true);
        skillIcon.sprite = skillSo.SkillIcon;
        skillName.text = skillSo.skillName;
    }

    public void ShowEquipMark(bool isEquip)
    {
        if (itemEquipmentImg != null)
        {
            itemEquipmentImg.gameObject.SetActive(isEquip);
        }
    }

    public void EmptySlot(bool isHide)
    {
        // 아이템 아이콘 비우기
        skillIcon.sprite = null;
        skillIcon.gameObject.SetActive(false);
        jobTypeImage.gameObject.SetActive(false);

        // 아이템 프레임 비활성화
        skillTier.gameObject.SetActive(false);
        foreach (GameObject go in skillGradeStars)
        {
            go.SetActive(false);
        }

        SkillData = null;
        skillName.text = "";
        gameObject.SetActive(!isHide);
        emptySkillImg.SetActive(true);
    }

    public void OnClickSlotBtn()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        OnClickSlot?.Invoke(SkillData);
    }

    public void SetSelectedSlot(bool isSelected)
    {
        if (selectedSlotImg == null)
        {
            return;
        }

        selectedSlotImg.gameObject.SetActive(isSelected);
    }

    public void SetOnClickCallback(Action<SkillSlot> callback)
    {
        onClickCallback = callback;
        onClickCallback?.Invoke(this);
    }

    public void UpdateSlot(ScrollData<SkillData> data)
    {
        DataIndex = data.DataIndex;
        SetSkillIcon(data.Data, false);
        SetSelectedSlot(data.IsSelected);
    }
}