using System.Collections.Generic;
using UnityEngine;

// 가챠 확률 로직 추상화.
public interface IGachaStrategy<T>
{
    T Pull(List<T> candidates, Dictionary<Tier, float> tierRates);
}

// 캐릭터 가챠
public class RandomCharacterGachaStrategy : IGachaStrategy<PlayerUnitSO>
{
    public PlayerUnitSO Pull(List<PlayerUnitSO> candidates,  Dictionary<Tier, float> tierRates)
    {
        Dictionary<Tier, List<PlayerUnitSO>> characterTierGroups = new();
        
        foreach(PlayerUnitSO character in candidates)
        {
            if (!characterTierGroups.ContainsKey(character.Tier))
            {
                characterTierGroups[character.Tier] = new List<PlayerUnitSO>();
            }
            characterTierGroups[character.Tier].Add(character);
        }

        float rand = Random.Range(0f, 100f);
        float accumulated = 0;

        foreach (var pair in tierRates)
        {
            accumulated += pair.Value;
            if (rand <= accumulated)
            {
                if (characterTierGroups.TryGetValue(pair.Key, out var group) &&  group.Count > 0)
                {
                    return group[Random.Range(0, group.Count)];
                }

                Debug.LogWarning($"{pair.Key} 티어에 해당되는 유닛이 존재하지 않습니다. rand={rand}, 누적확률={accumulated}");
                break;
            }
        }
        return null;
    }
}

// 쌩랜덤 가챠.
// 나중에 픽업이나 천장 등 시스템 구현하게 되면 더 늘릴수도.
public class RandoomSkillGachaStrategy : IGachaStrategy<ActiveSkillSO>
{
    public ActiveSkillSO Pull(List<ActiveSkillSO> candidates, Dictionary<Tier, float> tierRates)
    {
        // 티어별로 데이터 후보 분리
        Dictionary<Tier, List<ActiveSkillSO>> skillTierGroups = new();

        foreach(ActiveSkillSO skill in candidates)
        {
            if (!skillTierGroups.ContainsKey(skill.activeSkillTier))
            {
                skillTierGroups[skill.activeSkillTier] = new List<ActiveSkillSO>();
            }
            skillTierGroups[skill.activeSkillTier].Add(skill);
        }

        float rand = Random.Range(0f, 100f); // 0~100 사이 무작위 뽑기
        float accumulated = 0; // 누적 확률 값
        
        // 티어를 확률에 따라 뽑음
        foreach(var pair in tierRates)
        {
            accumulated += pair.Value; // Dic의 확률을 앞에서부터 계속 더해가면서 누적 확률 값을 지정
            if (rand <= accumulated) // 누적 확률 값이 계속 더해지면서 Random값보다 커지면 해당 티어가 선택됨.
            {
                if(skillTierGroups.TryGetValue(pair.Key, out var group) && group.Count > 0)
                {
                    return group[Random.Range(0, group.Count)]; // 해당 티어 그룹 중에서도 하나를 뽑음
                }

                Debug.LogWarning($"{pair.Key} 티어에 해당되는 스킬이 존재하지 않습니다. rand={rand}, 누적확률={accumulated}");
                break; // 해당 티어가 선택되면 루프 종료
            }
        }
        return null; // 아무 티어도 선택되지 않았을 경우의 예외
    }
}

// Equipment용. 위와 동일함. 나중에 제너릭 등으로 하나로 정리.
public class RandoomEquipmentGachaStrategy : IGachaStrategy<EquipmentItemSO>
{
    public EquipmentItemSO Pull(List<EquipmentItemSO> candidates, Dictionary<Tier, float> tierRates)
    {
        Dictionary<Tier, List<EquipmentItemSO>> equipmentTierGroups = new();

        foreach (EquipmentItemSO equipment in candidates)
        {
            if (!equipmentTierGroups.ContainsKey(equipment.Tier))
            {
                equipmentTierGroups[equipment.Tier] = new List<EquipmentItemSO>();
            }
            equipmentTierGroups[equipment.Tier].Add(equipment);
        }

        float rand = Random.Range(0f, 100f);
        float accumulated = 0;

        foreach(var pair in tierRates)
        {
            accumulated += pair.Value;
            if (rand <= accumulated)
            {
                if (equipmentTierGroups.TryGetValue(pair.Key, out var group) && group.Count > 0)
                {
                    return group[Random.Range(0, group.Count)];
                }

                Debug.Log($"{pair.Key} 티어에 해당되는 장비 아이템이 존재하지 않습니다. rand={rand}, 누적확률={accumulated}");
                break;
            }
        }
        return null;
    }
}
