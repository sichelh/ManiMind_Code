using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIEquipmentCombine : UIBase
{
    [Header("Inventory")]
    [SerializeField] private EquipmentCombineInventoryUI inventoryUI;

    [SerializeField] private RectTransform inventoryRect;

    [SerializeField] private List<InventorySlot> materialItemSlotList;
    [SerializeField] private InventorySlot resultItemSlot;

    [SerializeField] private TextMeshProUGUI requireCombineGoldTxt;
    private CombineManager combineManager;
    private InventoryManager inventoryManager;

    private List<EquipmentItem> MaterialItems = new()
    {
        null,
        null,
        null
    };

    private Vector2 originalPos;

    private EquipmentItem resultItem;
    private int RequierCombineItemGold => Define.RequierCombineItemGold;

    private bool IsItemInCombine(EquipmentItem item)
    {
        return MaterialItems.Any(i => i == item);
    }

    private bool CanAddItemToCombine()
    {
        return MaterialItems.Any(i => i == null);
    }

    private void Awake()
    {
        originalPos = inventoryRect.anchoredPosition;
    }

    private void Start()
    {
        combineManager = CombineManager.Instance;
        inventoryManager = InventoryManager.Instance;

        for (int i = 0; i < materialItemSlotList.Count; i++)
        {
            materialItemSlotList[i].Initialize(null, false);
            materialItemSlotList[i].OnClickSlot += RemoveCombineItem;
        }

        resultItemSlot.Initialize(null, false);
    }

    public void ToggleCombineItem(EquipmentItem item)
    {
        if (IsItemInCombine(item))
        {
            RemoveCombineItem(item);
        }
        else if (CanAddItemToCombine())
        {
            AddCombineItem(item);
        }
    }

    private void AddCombineItem(EquipmentItem item)
    {
        resultItemSlot.Initialize(null, false);

        if (item.IsEquipped)
        {
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("장착 중인 장비는 합성할 수 없습니다.");
            return;
        }

        // 티어 검사
        if (MaterialItems.Any(e => e != null && e.ItemSo.Tier != item.ItemSo.Tier))
        {
            PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("같은 티어의 장비만 합성 할 수 있습니다.");
            return;
        }

        int emptyIndex = MaterialItems.FindIndex(i => i == null);
        if (emptyIndex == -1)
        {
            return;
        }

        MaterialItems[emptyIndex] = item;
        materialItemSlotList[emptyIndex].Initialize(item, false);
    }

    private void RemoveCombineItem(EquipmentItem item)
    {
        int index = MaterialItems.FindIndex(i => i == item);
        if (index == -1)
        {
            return;
        }

        MaterialItems[index] = null;
        materialItemSlotList[index].Initialize(null, false);
    }

    public void OnClickCombine()
    {
        // 최고 티어 장비 포함 여부 확인
        Tier maxTier = System.Enum.GetValues(typeof(Tier)).Cast<Tier>().Max();
        bool hasMaxTier = MaterialItems.Any(i => i != null && i.ItemSo.Tier == maxTier);

        if (hasMaxTier)
        {
            PopupManager.Instance.GetUIComponent<TwoChoicePopup>().SetAndOpenPopupUI(
                "합성 확인",
                $"{(int)maxTier + 1}성 장비가 포함되어 있습니다.\n정말 합성하시겠습니까?",
                leftAct: ExecuteCombine, // 확인 시 실행
                rightAct: null,
                leftBtnTxt: "진행",
                rightBtnTxt: "취소"
            );
        }
        else
        {
            ExecuteCombine(); // 최고티어 없으면 바로 합성
        }

    }

    private void ExecuteCombine()
    {
        resultItem = combineManager.TryCombine(MaterialItems);
        if (resultItem == null)
        {
            return;
        }

        for (int i = 0; i < MaterialItems.Count; i++)
        {
            inventoryManager.RemoveItem(MaterialItems[i].InventoryId);
            materialItemSlotList[i].EmptySlot(false);
        }

        inventoryManager.AddItem(resultItem);
        for (int i = 0; i < MaterialItems.Count; i++)
        {
            MaterialItems[i] = null;
        }

        resultItemSlot.Initialize(resultItem, true);


        inventoryUI.Initialize(
            () => inventoryManager.GetInventoryItems(),
            (slot) =>
            {
                slot.OnClickSlot -= ToggleCombineItem;
                slot.OnClickSlot += ToggleCombineItem;
            });

        resultItem = null;
        requireCombineGoldTxt.text = AccountManager.Instance.Gold >= RequierCombineItemGold ? $"<color=#ffffffff>{RequierCombineItemGold:N0}G</color>" : $"<color=#ff0000ff>{RequierCombineItemGold:N0}G</color>";
    }


    public override void Close()
    {
        inventoryUI.ReuseScrollview.ResetScrollviewPosition();
        base.Close();
    }

    public override void Open()
    {
        base.Open();
        inventoryUI.Initialize(
            () => inventoryManager.GetInventoryItems(),
            (slot) =>
            {
                slot.OnClickSlot -= ToggleCombineItem;
                slot.OnClickSlot += ToggleCombineItem;
            });

        inventoryRect.DOKill();
        inventoryRect.DOAnchorPos(originalPos, 0.3f).From(originalPos + (Vector2.down * 300f)).SetEase(Ease.OutBack);

        requireCombineGoldTxt.text = AccountManager.Instance.Gold >= RequierCombineItemGold ? $"<color=#ffffffff>{RequierCombineItemGold:N0}G</color>" : $"<color=#ff0000ff>{RequierCombineItemGold:N0}G</color>";
    }
}