using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StagePanelHeroSlot : MonoBehaviour
{
    [SerializeField] private Image heroIcon;
    [SerializeField] private Image heroTierFrame;
    [SerializeField] private List<GameObject> heroTierStars;
    [SerializeField] private List<Sprite> heroTierFrameSprites;
    [SerializeField] private Sprite emptyFrameSprite;
    [SerializeField] private Image jobTypeImage;
    [SerializeField] private List<Sprite> jobTypeSprites;

    private PlayerUnitSO playerUnitSo;


    public void SetHeroSlot(PlayerUnitSO data)
    {
        if (data == null)
        {
            EmptySlot();
            return;
        }

        this.gameObject.SetActive(true);
        heroIcon.gameObject.SetActive(true);
        playerUnitSo = data;
        heroIcon.sprite = data.UnitIcon;
        heroTierFrame.sprite = heroTierFrameSprites[(int)data.Tier];
        jobTypeImage.sprite = jobTypeSprites[(int)data.JobType];

        for (int i = 0; i < heroTierStars.Count; i++)
        {
            heroTierStars[i].SetActive(i <= (int)data.Tier);
        }
    }


    private void EmptySlot()
    {
        heroIcon.sprite = null;
        heroIcon.gameObject.SetActive(false);
        this.gameObject.SetActive(false);

        heroTierFrame.sprite = emptyFrameSprite;
        for (int i = 0; i < heroTierStars.Count; i++)
        {
            heroTierStars[i].SetActive(false);
        }

        playerUnitSo = null;
    }
}