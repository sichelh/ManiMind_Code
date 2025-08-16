using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUnitInfoUI : MonoBehaviour
{
    [SerializeField] private List<MonsterUnitInfoSlotUI> slots;
    [SerializeField] private RectTransform monsterSlotsContainer;
    [SerializeField] private TextMeshProUGUI buttonArrow;
    [SerializeField] private float animationDuration;

    private bool isOpen = false;
    private float originalHeight;

    private List<Unit> units;
    private BattleManager battleManager;

    private void OnEnable()
    {
        StartCoroutine(WaitForBattleManagerInit());
    }

    // 호출 순서 문제 때문에 BattleManager 준비되면 참조
    private IEnumerator WaitForBattleManagerInit()
    {
        yield return new WaitUntil(() => BattleManager.Instance != null && BattleManager.Instance.EnemyUnits.Count > 0);

        battleManager = BattleManager.Instance;
        units = battleManager.EnemyUnits;

        // 유닛 수 만큼 켜주고 정보 업데이트
        for (int i = 0; i < units.Count; i++)
        {
            slots[i].gameObject.SetActive(true);
            slots[i].Initialize(units[i]);
            units[i].OnDead += UpdateUnits;
        }

        // 전투 종료 후 적 타겟 갱신될때마다 업데이트
        battleManager.OnTurnEnded += UpdateUnits;
    }

    private void Start()
    {
        // rect 원본 높이 저장
        originalHeight = monsterSlotsContainer.rect.height;

        // 처음엔 접어두기
        monsterSlotsContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
    }

    private void UpdateUnits()
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].IsDead) // 유닛 사망 시 UI도 꺼줌
            {
                slots[i].gameObject.SetActive(false);
            }

            slots[i].Initialize(units[i]);
        }
    }

    // 버튼 누르면 몬스터 정보 열고 닫기
    public void OnToggleMonsterList()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        float  targetHeight = isOpen ? 0 : originalHeight;
        string arrow        = isOpen ? ">" : "<";

        buttonArrow.text = arrow;

        monsterSlotsContainer.DOSizeDelta(new Vector2(monsterSlotsContainer.sizeDelta.x, targetHeight), animationDuration).SetEase(Ease.OutCubic);
        isOpen = !isOpen;
    }

    private void OnDisable()
    {
        if (battleManager != null)
        {
            battleManager.OnTurnEnded -= UpdateUnits;
        }

        if (units != null)
        {
            for (int i = 0; i < units.Count; i++)
            {
                units[i].OnDead -= UpdateUnits;
            }
        }
    }
}