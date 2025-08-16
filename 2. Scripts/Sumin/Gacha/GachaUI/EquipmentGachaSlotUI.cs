using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EquipmentGachaSlotUI : MonoBehaviour
{
    [SerializeField] private Image equipmentImage;
    [SerializeField] private TextMeshProUGUI equipmentNameText;
    [SerializeField] private Image itemSlotFrame;
    [SerializeField] private List<GameObject> itemGradeStars;
    [SerializeField] private List<Sprite> itemGradeSprites;
    [SerializeField] private Image jobTypeImage;
    [SerializeField] private List<Sprite> jobTypeSprites;

    public void Initialize(EquipmentItemSO equipment)
    {
        equipmentImage.sprite = equipment.ItemSprite;
        equipmentNameText.text = equipment.ItemName;
        itemSlotFrame.sprite = itemGradeSprites[(int)equipment.Tier];

        for (int i = 0; i < itemGradeStars.Count; i++)
        {
            itemGradeStars[i].SetActive(i <= (int)equipment.Tier);
        }

        if (equipment.IsEquipableByAllJobs)
        {
            jobTypeImage.gameObject.SetActive(false);
        }
        else
        {
            jobTypeImage.gameObject.SetActive(true);
            jobTypeImage.sprite = jobTypeSprites[(int)equipment.JobType];
        }
    }
}
