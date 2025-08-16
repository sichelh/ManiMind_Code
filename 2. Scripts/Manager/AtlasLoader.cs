using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;


public enum AtlasType
{
    WeaponMelee,
    WeaponRange,
    Equipment,
    JobOrCharacterIcon,
    SlotFrame,
    CharacterSmall,
}

[Serializable]
public struct AtlasEntry
{
    public AtlasType Type;
    public SpriteAtlas Atlas;
}

public class AtlasLoader : Singleton<AtlasLoader>
{
    [SerializeField] private List<AtlasEntry> atlasEntries = new();

    private Dictionary<AtlasType, SpriteAtlas> atlasDict = new();

    protected override void Awake()
    {
        base.Awake();
        if (!isDuplicated)
        {
            Initialize();
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    private void Initialize()
    {
        atlasDict.Clear();
        foreach (AtlasEntry entry in atlasEntries)
        {
            if (!atlasDict.ContainsKey(entry.Type) && entry.Atlas != null)
            {
                atlasDict.Add(entry.Type, entry.Atlas);
            }
        }
    }


    public Sprite GetSpriteFromAtlas(AtlasType type, string spriteName)
    {
        if (!atlasDict.TryGetValue(type, out SpriteAtlas atlas))
        {
            Debug.LogError($"[AtlasLoader] Atlas not found for type: {type}");
            return null;
        }

        Sprite sprite = atlas.GetSprite(spriteName);
        if (sprite == null)
        {
            Debug.LogWarning($"[AtlasLoader] Sprite not found: {spriteName} in atlas {type}");
        }

        return sprite;
    }
}