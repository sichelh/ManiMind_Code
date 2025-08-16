using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipButton : MonoBehaviour
{
    [Header("이미지 / 타입")]
    [SerializeField] private Image icon;

    [SerializeField] private TMP_Text typeText;
    [SerializeField] private Button button;

    private EquipmentItem equip;
    private Action<EquipButton, bool> onClick;
    private bool isEquipped;
    private bool isSlotButton;

    public void Initialize(EquipmentItem item, bool isEquipped, bool isSlotButton, Action<EquipButton, bool> callback)
    {
        if (item == null)
        {
            EmptySlot();
            return;
        }

        equip = item;
        this.isEquipped = isEquipped;
        this.isSlotButton = isSlotButton;
        onClick = callback;

        icon.sprite = item.EquipmentItemSo.ItemSprite;
        typeText.text = item.EquipmentItemSo.EquipmentType.ToString();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void EmptySlot()
    {
        equip = null;
        icon.sprite = null;
        button.onClick.RemoveAllListeners();
        typeText.text = string.Empty;
        isEquipped = false;
    }

    private void OnClick()
    {
        onClick?.Invoke(this, isEquipped);
    }

    #region

    public EquipmentItem GetEquipmentItem()
    {
        return equip;
    }

    public bool IsEquipped   => isEquipped;
    public bool IsSlotButton => isSlotButton;

    #endregion
}