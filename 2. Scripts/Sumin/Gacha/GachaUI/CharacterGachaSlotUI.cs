using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterGachaSlotUI : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private List<GameObject> unitTierStar;
    [SerializeField] private Image tierFrameImage;
    [SerializeField] private List<Sprite> unitTierFrame;
    [SerializeField] private Image jobIconImage;
    [SerializeField] private List<Sprite> unitJobIcon;

    public void Initialize(PlayerUnitSO character)
    {
        characterImage.sprite = character.UnitIcon;
        characterNameText.text = character.UnitName;
        tierFrameImage.sprite = unitTierFrame[(int)character.Tier];
        jobIconImage.sprite = unitJobIcon[(int)character.JobType];
        for (int i = 0; i < unitTierStar.Count; i++)
        {
            unitTierStar[i].SetActive(i <= (int)character.Tier);
        }
    }
}
