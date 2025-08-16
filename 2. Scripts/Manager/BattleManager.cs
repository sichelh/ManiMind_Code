using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class BattleManager : SceneOnlySingleton<BattleManager>
{
    public List<Transform> PartyUnitsTrans;
    public List<Transform> EnemyUnitsTrans;
    public List<Unit>  PartyUnits  { get; private set; } = new();
    public List<Unit>  EnemyUnits  { get; private set; } = new();
    public TurnHandler TurnHandler { get; private set; }

    private StageSO currentStage;
    public List<Unit>   AllUnits { get; private set; } = new();
    public event Action OnTurnEnded;

    private UIReward uiReward;

    public int TurnCount { get; private set; } = 1;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        currentStage = PlayerDeckContainer.Instance.SelectedStage;

        SetAlliesUnit(PlayerDeckContainer.Instance.CurrentDeck);

        SetEnemiesUnit(currentStage.Monsters);

        TurnHandler = new TurnHandler();
        SetAllUnits(PartyUnits.Concat(EnemyUnits).ToList());
        BattleSceneLoader.Instance.LoadAssets();
    }

    /// <summary>
    /// 아군 유닛의 리스트를 설정하고 초기화합니다.
    /// </summary>
    /// <param name="playerDeck">플레이어 덱 데이터</param>
    private void SetAlliesUnit(PlayerDeck playerDeck)
    {
        for (int i = 0; i < playerDeck.DeckDatas.Count; i++)
        {
            EntryDeckData deckData = playerDeck.DeckDatas[i];
            if (deckData == null)
            {
                continue;
            }

            Transform  parent = i < PartyUnitsTrans.Count ? PartyUnitsTrans[i] : transform;
            GameObject go     = Instantiate(deckData.CharacterSo.UnitPrefab, parent, false);
            Unit       unit   = go.GetComponent<Unit>();
            unit.Initialize(new UnitSpawnData { UnitSo = deckData.CharacterSo, DeckData = deckData });
            PartyUnits.Add(unit as PlayerUnitController);
        }
    }

    /// <summary>
    /// 적 유닛의 리스트를 설정하고 초기화합니다.
    /// </summary>
    /// <param name="units">적 유닛의 스크립터블 오브젝트 리스트</param>
    private void SetEnemiesUnit(List<EnemyUnitSO> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            EnemyUnitSO so     = units[i];
            Transform   parent = i < EnemyUnitsTrans.Count ? EnemyUnitsTrans[i] : transform;

            GameObject go = Instantiate(so.UnitPrefab, parent, true);
            go.transform.localPosition = Vector3.zero;

            Unit unit = go.GetComponent<Unit>();
            unit.Initialize(new UnitSpawnData { UnitSo = so, DeckData = null });

            if (unit is EnemyUnitController eu)
            {
                EnemyUnits.Add(eu);
                OnTurnEnded += eu.ChoiceAction;
                eu.ChoiceAction();
            }
        }
    }

    /// <summary>
    /// 게임 내 모든 유닛의 리스트를 설정하고, 턴 핸들러를 초기화합니다.
    /// </summary>
    /// <param name="units">모든 유닛의 리스트</param>
    public void SetAllUnits(List<Unit> units)
    {
        AllUnits = units;
        TurnHandler.Initialize(AllUnits);
    }

    /// <summary>
    /// 현재 턴을 시작하기 위한 작업을 처리하는 메서드
    /// </summary>
    /// <remarks>
    /// 턴 핸들러를 통해 다음 턴에 진행할 유닛을 결정하고, 해당 유닛의 턴 시작 작업을 초기화합니다.
    /// 카메라를 현재 턴의 유닛에 맞게 변경하며, 턴 시작에 필요한 추가 작업을 실행합니다.
    /// </remarks>
    public void StartTurn()
    {
        TurnHandler.StartNextTurn();
    }

    /// <summary>
    /// 현재 턴을 종료하고 다음 턴으로 넘어가기 위한 작업을 처리하는 메서드
    /// </summary>
    /// <remarks>
    /// 모든 아군과 적의 상태를 업데이트하며, 스테이지 클리어 또는 실패 여부를 확인하고 다음 턴을 준비합니다.
    /// 턴 종료 시 관련 매니저 및 상태를 초기화하고, 턴 카운트를 증가시킵니다.
    /// </remarks>
    public void EndTurn()
    {
        OnTurnEnded?.Invoke();
        for (int i = 0; i < AllUnits.Count; i++)
        {
            Unit u = AllUnits[i];
            if (u.IsDead)
            {
                continue;
            }

            u.CurrentEmotion.AddStack(u);
            u.StatusEffectManager?.OnTurnPassed();
        }

        for (int i = 0; i < PartyUnits.Count; i++)
        {
            Unit p = PartyUnits[i];
            if (!p.IsDead)
            {
                p.ChangeUnitState(PlayerUnitState.Idle);
            }
        }

        bool allEnemiesDead = true;
        for (int i = 0; i < EnemyUnits.Count; i++)
        {
            if (!EnemyUnits[i].IsDead)
            {
                allEnemiesDead = false;
                break;
            }
        }

        if (allEnemiesDead)
        {
            OnStageClear();
            return;
        }

        bool allPartyDead = true;
        for (int i = 0; i < PartyUnits.Count; i++)
        {
            if (!PartyUnits[i].IsDead)
            {
                allPartyDead = false;
                break;
            }
        }

        if (allPartyDead)
        {
            OnStageFail();
            return;
        }


        TurnHandler.RefillTurnQueue();


        CommandPlanner.Instance.Clear();    // 턴 종료되면 전략 플래너도 초기화
        InputManager.Instance.Initialize(); // 턴 종료되면 인풋매니저도 초기화
        TurnCount++;                        // 턴 종료되면 턴 수 +1
    }


    /// <summary>
    /// 특정 유닛의 아군 목록을 반환하는 메서드
    /// </summary>
    /// <param name="unit">아군 리스트를 확인하려는 기준 유닛</param>
    /// <returns>기준 유닛이 속한 진영에서 살아 있는 아군 유닛 목록</returns>
    public List<Unit> GetAllies(Unit unit)
    {
        if (PartyUnits.Contains(unit))
        {
            return PartyUnits.Where(u => !u.IsDead && u != unit).ToList();
        }

        if (EnemyUnits.Contains(unit))
        {
            return EnemyUnits.Where(u => !u.IsDead && u != unit).ToList();
        }

        return new List<Unit>(0);
    }

    /// <summary>
    /// 특정 유닛의 적 유닛 목록을 반환하는 메서드
    /// </summary>
    /// <param name="unit">적 리스트를 확인하려는 기준 유닛</param>
    /// <returns>기준 유닛으로부터 살아 있는 적 유닛 목록</returns>
    public List<Unit> GetEnemies(Unit unit)
    {
        if (PartyUnits.Contains(unit))
        {
            return EnemyUnits.Where(u => !u.IsDead && u != unit).ToList();
        }
        else
        {
            return PartyUnits.Where(u => !u.IsDead && u != unit).ToList();
        }
    }

    public List<Unit> GetAllUnits(Unit unit)
    {
        return AllUnits.Where(u => !u.IsDead && u != unit).ToList();
    }

    /// <summary>
    /// 스테이지 클리어 시 호출되는 메서드
    /// </summary>
    private void OnStageClear()
    {
        for (int i = 0; i < PartyUnits.Count; i++)
        {
            Unit p = PartyUnits[i];
            if (!p.IsDead)
            {
                p.ChangeUnitState(PlayerUnitState.Victory);
            }
        }


        // 튜토리얼에서 BattleVictory 이벤트 발행
        TutorialManager tutorial = TutorialManager.Instance;

        if (tutorial != null && tutorial.IsActive &&
            tutorial.CurrentStep?.Actions
                .OfType<TriggerWaitActionData>()
                .Any(x => x.triggerEventName == "BattleVictory") == true)
        {
            EventBus.Publish("BattleVictory");
        }


        AudioManager.Instance.PlayBGM(BGMName.VictoryBGM.ToString());
        PartyUnits.Where(x => !x.IsDead).ToList().ForEach(x => x.ChangeUnitState(PlayerUnitState.Victory));
        string rewardKey = $"{currentStage.ID}_Clear_Reward";
        RewardManager.Instance.AddReward(rewardKey);
        AccountManager.Instance.UpdateBestStage(currentStage);
        RewardManager.Instance.GiveRewardAndOpenUI(() =>
        {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsActive)
            {
                LoadSceneManager.Instance.LoadScene("DeckBuildingScene");
                return;
            }

            LoadSceneManager.Instance.LoadScene("DeckBuildingScene", () => UIManager.Instance.Open(UIManager.Instance.GetUIComponent<UIStageSelect>()));
        });
        InputManager.Instance.BattleEnd();
    }

    /// 스테이지 실패 시 호출되는 메서드
    private void OnStageFail()
    {
        UIManager.Instance.Open(PopupManager.Instance.GetUIComponent<UIDefeat>(), false);
        AudioManager.Instance.PlayBGM(BGMName.DefeatBGM.ToString());
        InputManager.Instance.BattleEnd();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}

public class UnitSpawnData
{
    public UnitSO UnitSo;
    public EntryDeckData DeckData; // null이면 Enemy
}