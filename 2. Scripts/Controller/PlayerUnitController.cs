using UnityEngine;
using PlayerState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Random = UnityEngine.Random;

public enum ActionType
{
    None,
    Attack,
    Skill
}

[RequireComponent(typeof(PlayerSkillController))]
public class PlayerUnitController : BaseController<PlayerUnitController, PlayerUnitState>
{
    [SerializeField] private PlayerUnitIncreaseSo playerUnitIncreaseSo;
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip moveClip;
    [SerializeField] private AnimationClip victoryClip;
    [SerializeField] private AnimationClip readyActionClip;
    [SerializeField] private AnimationClip deadClip;
    [SerializeField] private AnimationClip hitClip;

    public EquipmentManager EquipmentManager { get; private set; }
    public Vector3          StartPostion     { get; private set; }
    public PlayerUnitSO     PlayerUnitSo     { get; private set; }
    public PassiveSO        PassiveSo        { get; private set; }

    private HPBarUI hpBar;

    public override event Action OnDead;

    protected override IState<PlayerUnitController, PlayerUnitState> GetState(PlayerUnitState state)
    {
        return state switch
        {
            PlayerUnitState.Idle        => new IdleState(),
            PlayerUnitState.Move        => new MoveState(),
            PlayerUnitState.Return      => new PlayerState.ReturnState(),
            PlayerUnitState.Attack      => new AttackState(),
            PlayerUnitState.Die         => new DeadState(),
            PlayerUnitState.Skill       => new SkillState(),
            PlayerUnitState.Victory     => new VictoryState(),
            PlayerUnitState.ReadyAction => new ReadyAction(),
            PlayerUnitState.Hit         => new HitState(),
            _                           => null
        };
    }

    protected override void Awake()
    {
        SkillController = GetComponent<PlayerSkillController>();
        base.Awake();
        EquipmentManager = new EquipmentManager(this);
    }

    protected override void Start()
    {
        base.Start();
        hpBar = HealthBarManager.Instance.SpawnHealthBar(this);
        StartPostion = transform.position;
    }

    public override void ChangeUnitState(Enum newState)
    {
        stateMachine.ChangeState(states[Convert.ToInt32(newState)]);
        CurrentState = (PlayerUnitState)newState;
    }

    protected override Enum GetHitStateEnum()
    {
        return PlayerUnitState.Hit;
    }

    // === 공통 헬퍼 구현 ===
    public override void EnterMoveState()
    {
        ChangeUnitState(PlayerUnitState.Move);
    }

    public override void EnterAttackState()
    {
        ChangeUnitState(PlayerUnitState.Attack);
    }

    public override void EnterReturnState()
    {
        ChangeUnitState(PlayerUnitState.Return);
    }

    public override void EnterSkillState()
    {
        ChangeUnitState(PlayerUnitState.Skill);
    }

    public override void Initialize(UnitSpawnData deckData)
    {
        UnitSo = deckData.UnitSo;
        if (UnitSo is PlayerUnitSO playerUnitSo)
        {
            PlayerUnitSo = playerUnitSo;
        }

        if (PlayerUnitSo == null)
        {
            return;
        }

        PassiveSo = PlayerUnitSo.PassiveSkill.CloneForRuntime(this);

        foreach (SkillData skillData in deckData.DeckData.SkillDatas)
        {
            if (skillData == null)
            {
                SkillManager.AddActiveSkill(null);
                continue;
            }

            SkillManager.AddActiveSkill(skillData.skillSo);
        }

        StatManager.Initialize(PlayerUnitSo, this, deckData.DeckData.EquippedItems.Values.ToList(), deckData.DeckData.Level, playerUnitIncreaseSo);

        SkillManager.InitializeSkillManager(this);
        AnimationEventListener.Initialize(this);
        AnimatorOverrideController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
        ChangeClip(Define.AttackClipName, UnitSo.AttackAniClip);
        ChangeClip(Define.IdleClipName, idleClip);
        ChangeClip(Define.MoveClipName, moveClip);
        ChangeClip(Define.VictoryClipName, victoryClip);
        ChangeClip(Define.ReadyActionClipName, readyActionClip);
        ChangeClip(Define.DeadClipName, deadClip);
        ChangeClip(Define.HitClipName, hitClip);
    }

    public override void PlayAttackVoiceSound()
    {
        if (PlayerUnitSo.AttackVoiceSound != SFXName.None)
        {
            AudioManager.Instance.PlaySFX(PlayerUnitSo.AttackVoiceSound.ToString());
        }
        else
        {
            Gender        gender = Define.GetGender(PlayerUnitSo.JobType);
            StringBuilder sb     = new();
            sb.Append(gender.ToString());
            sb.Append("AttackSound");
            AudioManager.Instance.PlaySFX(sb.ToString());
        }
    }

    public override void PlayHitVoiceSound()
    {
        if (PlayerUnitSo.HitVoiceSound != SFXName.None)
        {
            AudioManager.Instance.PlaySFX(PlayerUnitSo.HitVoiceSound.ToString());
        }
        else
        {
            Gender        gender = Define.GetGender(PlayerUnitSo.JobType);
            StringBuilder sb     = new();
            sb.Append(gender.ToString());
            sb.Append("HitSound");
            AudioManager.Instance.PlaySFX(sb.ToString());
        }
    }

    public override void PlayDeadSound()
    {
        if (PlayerUnitSo.DeadSound != SFXName.None)
        {
            AudioManager.Instance.PlaySFX(PlayerUnitSo.DeadSound.ToString());
        }
        else
        {
            Gender        gender = Define.GetGender(PlayerUnitSo.JobType);
            StringBuilder sb     = new();
            sb.Append(gender.ToString());
            sb.Append("DeadSound");
            AudioManager.Instance.PlaySFX(sb.ToString());
        }
    }

    public override void Dead()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        OnDead?.Invoke();
        if (LastAttacker != null)
        {
            bool isTimelineAttack =
                LastAttacker.CurrentAction == ActionType.Skill
                && LastAttacker.SkillController?.CurrentSkillData?.skillSo?.skillTimeLine != null;

            if (!isTimelineAttack)
            {
                if (LastAttacker.CurrentAttackAction.DistanceType == AttackDistanceType.Melee)
                {
                    LastAttacker.OnMeleeAttackFinished += InvokeHitFinished;
                }
                else
                {
                    LastAttacker.OnRangeAttackFinished += InvokeHitFinished;
                }
            }
        }

        ChangeUnitState(PlayerUnitState.Die);
        StatusEffectManager.RemoveAllEffects();
        hpBar.UnLink();

        Agent.enabled = false;
        Obstacle.carving = false;
        Obstacle.enabled = false;

        List<IPassiveAllyDeathTrigger> allyDeathPassives = BattleManager.Instance.GetAllies(this)
            .Select(u => (u as PlayerUnitController)?.PassiveSo)
            .OfType<IPassiveAllyDeathTrigger>()
            .ToList();
        foreach (IPassiveAllyDeathTrigger unit in allyDeathPassives)
        {
            unit.OnAllyDead();
        }
    }

    public override void StartTurn()
    {
        List<Unit> enemies = BattleManager.Instance.GetEnemies(this);
        if (!IsDead && PassiveSo is IPassiveTurnStartTrigger turnStart)
        {
            turnStart.OnTurnStart(this);
        }

        bool unable = enemies.Count == 0 || IsDead || IsStunned || Target == null || Target.IsDead || CurrentAction == ActionType.None;
        if (unable)
        {
            EndTurn();
            return;
        }

        ChangeTurnState(TurnStateType.StartTurn);
    }

    public override void EndTurn()
    {
        if (PassiveSo is IPassiveTurnEndTrigger turnEndTrigger)
        {
            turnEndTrigger.OnTurnEnd(this);
        }

        if (PassiveSo is IPassiveEmotionStackTrigger stackPassive)
        {
            stackPassive.OnEmotionStackIncreased(CurrentEmotion);
        }

        Target = null;
        ChangeAction(ActionType.None);
        ChangeUnitState(PlayerUnitState.ReadyAction);
        SkillController.EndTurn();
        BattleManager.Instance.TurnHandler.OnUnitTurnEnd();
    }
}