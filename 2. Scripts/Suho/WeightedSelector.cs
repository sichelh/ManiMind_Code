using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeightedSelector<T>
{
    public class Entry
    {
        public T Item;
        public Func<float> GetWeight;
        public Func<bool> IsAvailable;

        public Entry(T item, Func<float> getWeight, Func<bool> isAvailable)
        {
            Item = item;
            GetWeight = getWeight;
            IsAvailable = isAvailable;
        }
    }

    private List<Entry> entries = new List<Entry>();

    public void Add(T item, Func<float> getWeight, Func<bool> isAvailable)
    {
        entries.Add(new Entry(item, getWeight, isAvailable));
    }

    public T Select()
    {
        var availableEntries = entries.Where(e => e.IsAvailable()).ToList();
        if (availableEntries.Count == 0)
            return default;

        float totalWeight = availableEntries.Sum(e => e.GetWeight());
        float rand = UnityEngine.Random.Range(0f, totalWeight);
        float accum = 0f;

        foreach (var entry in availableEntries)
        {
            accum += entry.GetWeight();
            if (rand <= accum)
                return entry.Item;
        }

        return availableEntries.Last().Item; // fallback
    }

    public void Clear()
    {
        entries.Clear();
    }
}