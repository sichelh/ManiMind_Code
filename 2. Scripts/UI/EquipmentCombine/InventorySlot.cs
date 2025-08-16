using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IReuseScrollData<InventoryItem>
{
    public int DataIndex { get; private set; }


    [SerializeField] private Image itemIcon;
    [SerializeField] private Image itemSlotFrame;
    [SerializeField] private Image itemEquipmentImg;
    [SerializeField] private Image itemEquippedUnitIconImg;
    [SerializeField] private Image selectedSlotImg;
    [SerializeField] private List<GameObject> itemGradeStars;

    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private TextMeshProUGUI amountTxt;

    [SerializeField] private Sprite opalSprite;
    [SerializeField] private Sprite goldSprite;

    [SerializeField] private Image jobTypeImage;
    public EquipmentItem Item { get; private set; }

    public event Action<EquipmentItem> OnClickSlot;

    private Action<InventorySlot> onClickCallback;

    private InventoryManager inventoryManager => InventoryManager.Instance;

    public void Initialize(EquipmentItem item, bool isHide)
    {
        if (item == null)
        {
            EmptySlot(isHide);
            return;
        }

        gameObject.SetActive(true);

        // 이름
        gameObject.name = $"InventorySlot_{item.InventoryId}";
        ShowEquipMark(item.IsEquipped);
        if (item.IsEquipped)
        {
            itemEquippedUnitIconImg.sprite = AtlasLoader.Instance.GetSpriteFromAtlas(AtlasType.JobOrCharacterIcon, item.EquippedUnit.CharacterSo.UnitCircleIcon.name);
        }

        itemSlotFrame.sprite = AtlasLoader.Instance.GetSpriteFromAtlas(AtlasType.SlotFrame, $"ItemSlot_{(int)item.ItemSo.Tier}");

        itemIcon.gameObject.SetActive(true);

        AtlasType atlasType = AtlasType.Equipment;
        switch (item.EquipmentItemSo.EquipmentType)
        {
            case EquipmentType.Weapon:
                switch (item.EquipmentItemSo.JobType)
                {
                    case JobType.Archer:
                    case JobType.Mage:
                    case JobType.Priest:
                        atlasType = AtlasType.WeaponRange;
                        break;
                    default:
                        atlasType = AtlasType.WeaponMelee;
                        break;
                }

                break;

            case EquipmentType.Armor:
            case EquipmentType.Accessory:
                atlasType = AtlasType.Equipment;
                break;
        }


        itemIcon.sprite = AtlasLoader.Instance.GetSpriteFromAtlas(atlasType, item.ItemSo.ItemSprite.name);
        if (item.EquipmentItemSo.IsEquipableByAllJobs)
        {
            jobTypeImage.gameObject.SetActive(false);
        }
        else
        {
            jobTypeImage.gameObject.SetActive(true);
            jobTypeImage.sprite = AtlasLoader.Instance.GetSpriteFromAtlas(AtlasType.JobOrCharacterIcon, $"{(int)item.EquipmentItemSo.JobType}_icon");
        }

        for (int i = 0; i < itemGradeStars.Count; i++)
        {
            itemGradeStars[i].SetActive(i <= (int)item.ItemSo.Tier);
        }

        amountTxt.text = item.Quantity > 0 ? $"x{item.Quantity}" : "";
        Item = item;
    }

    public void Initialize(RewardData rewardData)
    {
        EmptySlot(true);
        if (rewardData != null)
        {
            gameObject.SetActive(true);
            amountTxt.gameObject.SetActive(true);
            itemIcon.gameObject.SetActive(true);
            switch (rewardData.RewardType)
            {
                case RewardType.Gold:
                    itemIcon.sprite = goldSprite;
                    break;
                case RewardType.Opal:
                    itemIcon.sprite = opalSprite;
                    break;
                case RewardType.Item:
                    ItemSO item = TableManager.Instance.GetTable<ItemTable>().GetDataByID(rewardData.ItemId);
                    itemIcon.sprite = item.ItemSprite;
                    break;
                case RewardType.Skill:
                    ActiveSkillSO skill = TableManager.Instance.GetTable<ActiveSkillTable>().GetDataByID(rewardData.ItemId);
                    itemIcon.sprite = skill.SkillIcon;
                    break;
                case RewardType.Unit:
                    PlayerUnitSO unit = TableManager.Instance.GetTable<PlayerUnitTable>().GetDataByID(rewardData.ItemId);
                    itemIcon.sprite = unit.UnitIcon;
                    break;
            }

            amountTxt.text = rewardData.Amount > 1 ? $"x{rewardData.Amount}" : "";
        }
    }


    public void EmptySlot(bool isHide)
    {
        // 아이템 아이콘 비우기
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);
        jobTypeImage.gameObject.SetActive(false);

        // 아이템 프레임 비활성화
        itemSlotFrame.sprite = emptySlotSprite;
        // 장비 이미지 비우기
        itemEquipmentImg.gameObject.SetActive(false);
        foreach (GameObject go in itemGradeStars)
        {
            go.SetActive(false);
        }

        Item = null;
        gameObject.SetActive(!isHide);
        amountTxt.gameObject.SetActive(false);
    }

    public void ShowEquipMark(bool isEquip)
    {
        itemEquipmentImg.gameObject.SetActive(isEquip);
    }

    public void OnClickSlotBtn()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        OnClickSlot?.Invoke(Item);
    }

    public void SetOnClickCallback(Action<InventorySlot> callback)
    {
        onClickCallback = callback;
        onClickCallback?.Invoke(this);
    }

    public void SetSelectedSlot(bool isSelected)
    {
        if (selectedSlotImg == null)
        {
            return;
        }

        selectedSlotImg.gameObject.SetActive(isSelected);
    }

    public void UpdateSlot(ScrollData<InventoryItem> data)
    {
        DataIndex = data.DataIndex;
        Initialize(data.Data as EquipmentItem, false);
        SetSelectedSlot(data.IsSelected);
    }
}