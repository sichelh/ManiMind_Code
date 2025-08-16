using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public int InventoryId { get; set; } // 추가
    public ItemSO ItemSo;
    public int Quantity;

    public InventoryItem(ItemSO itemSo, int quantity)
    {
        ItemSo = itemSo;
        Quantity = quantity;
    }

    public InventoryItem(int id)
    {
        ItemSo = TableManager.Instance.GetTable<ItemTable>().GetDataByID(id);
    }

    public virtual InventoryItem Clone()
    {
        return new InventoryItem(ItemSo, Quantity);
    }
}

[Serializable]
public class SaveInventoryItem
{
    public int InventoryId;
    public int Id;
    public int Quantity;

    public SaveInventoryItem(InventoryItem item)
    {
        InventoryId = item.InventoryId;
        Id = item.ItemSo.ID;
        Quantity = item.Quantity;
    }

    public SaveInventoryItem(int id)
    {
        InventoryId = id;
    }

    public SaveInventoryItem() { }
}

public class InventoryManager : Singleton<InventoryManager>
{
    public event Action<int> OnInventorySlotUpdate;
    private GameManager gameManager;


    private Dictionary<int, InventoryItem> inventory = new();
    public IReadOnlyDictionary<int, InventoryItem> Inventory    => inventory;
    public Dictionary<JobType, List<int>>          JobInventory { get; private set; } = new();
    private int nextId = 0;

    protected override void Awake()
    {
        base.Awake();
        if (isDuplicated)
        {
            return;
        }

        gameManager = GameManager.Instance;
    }


    public void AddItem(InventoryItem item, int amount = 1)
    {
        AddNonStackableItem(item, amount);
    }

    public void RemoveItem(int id)
    {
        if (!inventory.Remove(id))
        {
            return;
        }

        foreach (List<int> jobList in JobInventory.Values)
        {
            jobList.Remove(id);
        }

        OnInventorySlotUpdate?.Invoke(id);
        SaveLoadManager.Instance.SaveModuleData(SaveModule.InventoryItem);
    }

    /// <summary>
    /// 비스택형 아이템을 추가하는 함수
    /// </summary>
    /// <param name="itemSo"></param>
    /// <param name="amount"></param>
    private void AddNonStackableItem(InventoryItem item, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            InventoryItem clonedItem = item.Clone();
            clonedItem.InventoryId = nextId;
            inventory[nextId] = clonedItem;

            if (clonedItem is EquipmentItem equipmentItem)
            {
                if (equipmentItem.EquipmentItemSo.IsEquipableByAllJobs)
                {
                    foreach (JobType job in Enum.GetValues(typeof(JobType)))
                    {
                        AddEquipmentItem(job, nextId);
                    }
                }
                else
                {
                    AddEquipmentItem(equipmentItem.EquipmentItemSo.JobType, nextId);
                }
            }

            OnInventorySlotUpdate?.Invoke(nextId);
            SaveLoadManager.Instance.SaveModuleData(SaveModule.InventoryItem);
            nextId++;
        }
    }

    private void AddEquipmentItem(JobType jobType, int itemId)
    {
        if (!JobInventory.TryGetValue(jobType, out List<int> inventoryList))
        {
            inventoryList = new List<int>();
        }

        inventoryList.Add(itemId);

        JobInventory[jobType] = inventoryList;
    }

    public List<InventoryItem> GetInventoryItems(JobType jobType)
    {
        if (!JobInventory.TryGetValue(jobType, out List<int> idList))
        {
            return new List<InventoryItem>();
        }

        List<InventoryItem> items = idList.Where(id => inventory.ContainsKey(id)).Select(id => inventory[id]).ToList();
        return items;
    }

    public List<InventoryItem> GetInventoryItems()
    {
        return inventory.Values.OrderBy(item => item.InventoryId).ToList();
    }

    public void ApplyLoadedInventory(List<SaveInventoryItem> loadedItems)
    {
        inventory.Clear();
        JobInventory.Clear();
        if (loadedItems.Count == 0)
        {
            return;
        }

        foreach (SaveInventoryItem savedItem in loadedItems)
        {
            ItemSO itemSo = TableManager.Instance.GetTable<ItemTable>().GetDataByID(savedItem.Id);
            if (itemSo == null)
            {
                Debug.LogWarning($"아이템 ID {savedItem.Id}를 찾을 수 없습니다.");
                continue;
            }

            EquipmentItem equipmentItem = new(itemSo as EquipmentItemSO);
            equipmentItem.InventoryId = savedItem.InventoryId;
            inventory[savedItem.InventoryId] = equipmentItem;
            if (equipmentItem.EquipmentItemSo.IsEquipableByAllJobs)
            {
                foreach (JobType job in Enum.GetValues(typeof(JobType)))
                {
                    AddEquipmentItem(job, equipmentItem.InventoryId);
                }
            }
            else
            {
                AddEquipmentItem(equipmentItem.EquipmentItemSo.JobType, equipmentItem.InventoryId);
            }

            nextId = equipmentItem.InventoryId;
        }

        nextId++;
    }
}