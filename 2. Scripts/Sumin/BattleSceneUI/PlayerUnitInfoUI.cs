using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitInfoUI : MonoBehaviour
{
    [SerializeField] private List<PlayerUnitInfoSlotUI> slots;

    private CommandPlanner commandPlanner;
    private List<Unit> units;

    private void OnEnable()
    {
        StartCoroutine(WaitForBattleManagerInit());
    }

    // 호출 순서 문제 때문에 BattleManager 준비되면 참조
    private IEnumerator WaitForBattleManagerInit()
    {
        yield return new WaitUntil(() => BattleManager.Instance != null && BattleManager.Instance.PartyUnits.Count > 0);

        units = BattleManager.Instance.PartyUnits;

        // 유닛 수 만큼 켜주고 정보 업데이트
        for (int i = 0; i < units.Count; i++)
        {
            slots[i].gameObject.SetActive(true);
            slots[i].UpdateUnitInfo(units[i]);
            slots[i].UpdateHpBar(units[i] as IDamageable);
            units[i].OnDead += UpdatePlayerUnitDead;
        }
        commandPlanner = CommandPlanner.Instance;

        commandPlanner.commandUpdated += UpdatePlayerUnitSelect;
    }

    // 커맨드 있으면 업데이트 해주기
    private void UpdatePlayerUnitSelect()
    {
        for (int i=0; i<units.Count; i++)
        {
            slots[i].UpdateUnitSelect(units[i]);
        }
    }

    private void UpdatePlayerUnitDead()
    {
        for (int i=0; i< units.Count; i++)
        {
            if (units[i].SelectedUnit.IsDead)
                slots[i].UpdateUnitDead();
        }
    }

    private void OnDisable()
    {
        if (commandPlanner != null)
            commandPlanner.commandUpdated -= UpdatePlayerUnitSelect;

        if (units != null)
        {
            for (int i = 0; i < units.Count; i++)
            {
                units[i].OnDead -= UpdatePlayerUnitDead;
            }
        }
    }
}
