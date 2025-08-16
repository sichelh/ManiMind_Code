using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IReuseScrollData<T>
{
    int  DataIndex { get; }
    void UpdateSlot(ScrollData<T> data);
}

public class ScrollData<T>
{
    public int  DataIndex  { get; private set; }
    public T    Data       { get; private set; }
    public bool IsSelected { get; set; }

    public ScrollData(int dataIndex, T data)
    {
        DataIndex = dataIndex;
        Data = data;
    }
}

public class ReuseScrollview<T> : MonoBehaviour where T : class
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewportRect;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Vector2 prefabSize = new(100f, 100f);
    [SerializeField] private Vector2 spacing = new(15f, 15f);
    [SerializeField] private Vector2 gridStartOffset = new(0f, 0f);
    [SerializeField] private bool isVertical = true;


    private float itemWidth;
    private float itemHeight;
    private int visibleItemCount;
    private int currentStartIndex;
    private Vector2 lastContentPosition;

    private int columnCount;
    private int rowCount;
    private bool isInitialized;

    private readonly List<RectTransform> itemList = new();
    private readonly List<ScrollData<T>> dataList = new();
    private readonly Dictionary<T, int> dataToIndexMap = new();

    public List<RectTransform> ItemList   => itemList;
    public List<ScrollData<T>> DataList   => dataList;
    public ScrollRect          ScrollRect => scrollRect;
    public bool                IsVertical => isVertical;

    private void Initialize()
    {
        Canvas.ForceUpdateCanvases();


        itemWidth = prefabSize.x;
        itemHeight = prefabSize.y;

        float viewWidth  = viewportRect.rect.width;
        float viewHeight = viewportRect.rect.height;

        columnCount = Mathf.FloorToInt((viewWidth + spacing.x) / (itemWidth + spacing.x));
        rowCount = Mathf.FloorToInt((viewHeight + spacing.y) / (itemHeight + spacing.y));

        int extraBuffer = 2;
        visibleItemCount = ((isVertical ? rowCount : columnCount) + 1 + extraBuffer) * (isVertical ? columnCount : rowCount);

        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    public void SetData(List<T> items)
    {
        if (!isInitialized)
        {
            Initialize();
            isInitialized = true;
        }

        dataList.Clear();
        dataToIndexMap.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            ScrollData<T> scrollData = new(i, items[i]);
            dataList.Add(scrollData);
            dataToIndexMap[items[i]] = i;
        }

        SetContentSize();
        // if (itemList.Count == 0)
        CreateItems();

        currentStartIndex = -1;
        lastContentPosition = content.anchoredPosition;
        RecycleItems();
    }

    private void SetContentSize()
    {
        int rowOrColCount = Mathf.CeilToInt((float)dataList.Count / (isVertical ? columnCount : rowCount));

        float contentWidth  = (columnCount * (itemWidth + spacing.x)) - spacing.x;
        float contentHeight = (rowOrColCount * (itemHeight + spacing.y)) + spacing.y;

        content.sizeDelta = isVertical
            ? new Vector2(contentWidth, contentHeight)
            : new Vector2(contentHeight, contentWidth);
    }

    private void CreateItems()
    {
        int requiredCount = Mathf.Min(visibleItemCount, dataList.Count);

        // 현재보다 부족하면 새로 생성
        while (itemList.Count < requiredCount)
        {
            GameObject    item     = Instantiate(itemPrefab, content);
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.sizeDelta = prefabSize;
            itemList.Add(itemRect);
        }

        // 필요 없는 슬롯은 비활성화
        for (int i = requiredCount; i < itemList.Count; i++)
        {
            itemList[i].gameObject.SetActive(false);
        }

        // 위치 및 데이터 갱신
        for (int i = 0; i < requiredCount; i++)
        {
            UpdateItemPosition(i, currentStartIndex + i);
            itemList[i].gameObject.SetActive(true);
        }
    }

    private void OnScrollValueChanged(Vector2 value)
    {
        Vector2 delta = content.anchoredPosition - lastContentPosition;
        if ((isVertical && Mathf.Abs(delta.y) >= itemHeight + spacing.y) ||
            (!isVertical && Mathf.Abs(delta.x) >= itemWidth + spacing.x))
        {
            RecycleItems();
            lastContentPosition = content.anchoredPosition;
        }
    }

    private void RecycleItems()
    {
        float position      = isVertical ? content.anchoredPosition.y : content.anchoredPosition.x;
        int   newStartIndex = Mathf.FloorToInt(position / (isVertical ? itemHeight + spacing.y : itemWidth + spacing.x)) * (isVertical ? columnCount : rowCount);

        int maxStartIndex = Mathf.Max(0, dataList.Count - visibleItemCount);
        newStartIndex = Mathf.Clamp(newStartIndex, 0, maxStartIndex);

        if (newStartIndex != currentStartIndex)
        {
            currentStartIndex = newStartIndex;

            for (int i = 0; i < itemList.Count; i++)
            {
                UpdateItemPosition(i, currentStartIndex + i);
            }
        }
    }

    private void UpdateItemPosition(int itemIndex, int dataIndex)
    {
        RectTransform item = itemList[itemIndex];

        if (dataIndex >= dataList.Count)
        {
            item.gameObject.SetActive(false); // 남은 슬롯 비활성화
            return;
        }

        int row, col;
        if (isVertical)
        {
            row = dataIndex / columnCount;
            col = dataIndex % columnCount;
        }
        else
        {
            col = dataIndex / rowCount;
            row = dataIndex % rowCount;
        }

        float x = gridStartOffset.x + (col * (itemWidth + spacing.x));
        float y = gridStartOffset.y - (row * (itemHeight + spacing.y));

        item.anchoredPosition = new Vector2(x, y);

        if (item.TryGetComponent(out IReuseScrollData<T> scrollData))
        {
            scrollData.UpdateSlot(dataList[dataIndex]);
        }
    }

    public int GetDataIndexFromItem(T slot)
    {
        return dataToIndexMap.GetValueOrDefault(slot, -1);
    }

    public ScrollData<T> GetDataFromItem(T slot)
    {
        return dataList[GetDataIndexFromItem(slot)];
    }

    public void RefreshAllVisibleSlots()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            int dataIndex = currentStartIndex + i;
            if (dataIndex >= dataList.Count)
            {
                itemList[i].gameObject.SetActive(false);
                continue;
            }

            itemList[i].gameObject.SetActive(true);
            if (itemList[i].TryGetComponent<IReuseScrollData<T>>(out IReuseScrollData<T> scrollData))
            {
                scrollData.UpdateSlot(dataList[dataIndex]);
            }
        }
    }

    public void RefreshSlotAt(int dataIndex)
    {
        if (dataIndex < 0 || dataIndex >= dataList.Count)
        {
            return;
        }

        int relativeIndex = dataIndex - currentStartIndex;

        if (relativeIndex < 0 || relativeIndex >= itemList.Count)
        {
            return;
        }

        if (itemList[relativeIndex].TryGetComponent<IReuseScrollData<T>>(out IReuseScrollData<T> scrollData))
        {
            scrollData.UpdateSlot(dataList[dataIndex]);
        }
    }

    public void ResetScrollviewPosition()
    {
        if (isVertical)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
        else
        {
            scrollRect.horizontalNormalizedPosition = 0;
        }
    }
}