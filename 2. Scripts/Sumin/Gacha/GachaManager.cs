using System.Collections.Generic;
using UnityEngine;

// 뽑기 결과를 저장하는 구조체
public struct GachaResult<T> where T : ScriptableObject
{
    public T GachaReward;           // 뽑기 보상 스킬 or 유닛
    public bool IsDuplicate;        // 이미 보유하고있는지 확인
    public int CompensationAmount;  // 중복 보상
}


public class GachaManager<T>
{
    private readonly IGachaStrategy<T> strategy;

    public GachaManager(IGachaStrategy<T> gachaStrategy)
    {
        strategy = gachaStrategy;
    }

    public T Draw(List<T> candidates, Dictionary<Tier, float> tierRates)
    {
        return strategy.Pull(candidates, tierRates);
    }
}
