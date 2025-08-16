using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerDeck
{
    // 선택한 카드덱
    public List<EntryDeckData> DeckDatas { get; private set; } = new List<EntryDeckData>();


    public void AddDeckData(EntryDeckData deckData)
    {
        DeckDatas.Add(deckData);
    }
}