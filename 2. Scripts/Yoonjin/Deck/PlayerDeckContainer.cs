using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeckContainer : Singleton<PlayerDeckContainer>
{
    // 현재 덱
    public PlayerDeck CurrentDeck { get; private set; } = new();

    public StageSO SelectedStage { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if (isDuplicated)
        {
            return;
        }
    }

    public void SetStage(StageSO stage)
    {
        SelectedStage = stage;
    }
}