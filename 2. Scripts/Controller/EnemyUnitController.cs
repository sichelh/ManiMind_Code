using DissolveExample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EnemyState;
using System.Text;
using Random = UnityEngine.Random;


[RequireComponent(typeof(EnemySkillContorller))]
public class EnemyUnitController : BaseController<EnemyUnitController, EnemyUnitState>
{
    public EnemyUnitSO MonsterSo { get; private set; }
    // Start is called before the first frame update

    private DissolveChilds dissolveChilds;
    private HPBarUI hpBar;
    public override bool IsTimeLinePlaying => TimeLineManager.Instance.isPlaying;

    public Vector3 StartPosition { get; private set; }
    private WeightedSelector<Unit> mainTargetSelector;

    public override event Action OnDead;

    protected override void Awake()
    {
        SkillController = GetComponent<EnemySkillContorller>();
        base.Awake();
        dissolveChilds = GetComponentInChildren<DissolveChilds>();
    }

    protected override void Start()
    {
        hpBar = HealthBarManager.Instance.SpawnHealthBar(this);
        StartPosition = transform.position;

        Agent.speed = 15f;
        Agent.acceleration = 100f;
        Agent.angularSpeed = 1000f;
    }

    protected override void Update()
    {
        if (IsDead)
        {
            return;
        }

        base.Update();
    }


    public override void ChangeUnitState(Enum newState)
    {
        stateMachine.ChangeState(states[Convert.ToInt32(newState)]);
        CurrentState = (EnemyUnitState)newState;
    }

    public override void Initialize(UnitSpawnData spawnData)
    {
        UnitSo = spawnData.UnitSo;
        if (UnitSo is EnemyUnitSO enemyUnitSo)
        {
            MonsterSo = enemyUnitSo;
        }

        if (MonsterSo == null)
        {
            return;
        }


        StatManager.Initialize(MonsterSo, this, PlayerDeckContainer.Instance.SelectedStage.MonsterLevel, PlayerDeckContainer.Instance.SelectedStage.MonsterIncrease);

        AnimatorOverrideController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
        AnimationEventListener.Initialize(this);
        ChangeClip(Define.IdleClipName, MonsterSo.IdleAniClip);
        ChangeClip(Define.MoveClipName, MonsterSo.MoveAniClip);
        ChangeClip(Define.AttackClipName, MonsterSo.AttackAniClip);
        ChangeClip(Define.DeadClipName, MonsterSo.DeadAniClip);
        ChangeClip(Define.HitClipName, MonsterSo.HitAniClip);
        foreach (EnemySkillData skillData in MonsterSo.SkillDatas)
        {
            SkillManager.AddActiveSkill(skillData.skillSO);
        }

        SkillManager.InitializeSkillManager(this);
        if (SkillController is EnemySkillContorller sc)
        {
            sc.InitSkillSelector();
        }


        InitTargetSelector();
        ChangeEmotion(MonsterSo.StartEmotion);
    }

    protected override IState<EnemyUnitController, EnemyUnitState> GetState(EnemyUnitState unitState)
    {
        return unitState switch
        {
            EnemyUnitState.Idle   => new IdleState(),
            EnemyUnitState.Move   => new MoveState(),
            EnemyUnitState.Return => new EnemyState.ReturnState(),
            EnemyUnitState.Attack => new EnemyState.AttackState(),
            EnemyUnitState.Skill  => new SkillState(),
            EnemyUnitState.Stun   => new StunState(),
            EnemyUnitState.Die    => new EnemyState.DeadState(),
            EnemyUnitState.Hit    => new HitState(),

            _ => null
        };
    }

    public override void PlayHitVoiceSound()
    {
        if (MonsterSo.HitVoiceSound != SFXName.None)
        {
            AudioManager.Instance.PlaySFX(MonsterSo.HitVoiceSound.ToString());
        }
        else
        {
            MonsterType monsterType = MonsterSo.monsterType;
            if (monsterType == MonsterType.None)
            {
                return;
            }

            StringBuilder sb = new();
            sb.Append(monsterType.ToString());
            sb.Append("HitSound");
            AudioManager.Instance.PlaySFX(sb.ToString());
        }
    }

    public override void PlayDeadSound()
    {
        if (MonsterSo.DeadSound != SFXName.None)
        {
            AudioManager.Instance.PlaySFX(MonsterSo.DeadSound.ToString());
        }
        else
        {
            MonsterType monsterType = MonsterSo.monsterType;
            if (monsterType == MonsterType.None)
            {
                return;
            }

            StringBuilder sb = new();
            sb.Append(monsterType.ToString());
            sb.Append("DeadSound");
            AudioManager.Instance.PlaySFX(sb.ToString());
        }
    }

    public override void PlayAttackVoiceSound()
    {
        if (MonsterSo.AttackVoiceSound != SFXName.None)
        {
            AudioManager.Instance.PlaySFX(MonsterSo.AttackVoiceSound.ToString());
        }
        else
        {
            MonsterType monsterType = MonsterSo.monsterType;
            if (monsterType == MonsterType.None)
            {
                return;
            }

            StringBuilder sb = new();
            sb.Append(monsterType.ToString());
            sb.Append("AttackSound");
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
        ChangeUnitState(EnemyUnitState.Die);
        StatusEffectManager.RemoveAllEffects();
        hpBar.UnLink();

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

        BattleManager.Instance.OnTurnEnded -= ChoiceAction;
        OnDead = null;
        Agent.enabled = false;
        Obstacle.carving = false;
        Obstacle.enabled = false;
        dissolveChilds.PlayDissolve(Animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
    }

    public bool ShouldUseSkill()
    {
        if (SkillController.CheckAllSkills() && Random.value < MonsterSo.skillActionProbability)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void InitTargetSelector()
    {
        mainTargetSelector = new WeightedSelector<Unit>();
        List<Unit> playerUnits = BattleManager.Instance.PartyUnits;
        foreach (Unit playerUnit in playerUnits)
        {
            mainTargetSelector.Add(
                playerUnit,
                () => playerUnit.StatManager.GetValue(StatType.Aggro),
                () => !playerUnit.IsDead
            );
        }
    }

    public void WeightedMainTargetSelector(SelectCampType campType)
    {
        if (campType == SelectCampType.Enemy)
        {
            SetTarget(mainTargetSelector.Select());
        }
        else if (campType == SelectCampType.Colleague)
        {
            List<Unit> allies = BattleManager.Instance.GetAllies(this);
            if (allies.Count > 0)
            {
                SetTarget(allies[Random.Range(0, allies.Count)]);
            }
        }
    }

    public void SelectMainTarget(ActionType actionType)
    {
        if (actionType == ActionType.Skill)
        {
            EnemySkillContorller sc = SkillController as EnemySkillContorller;
            if (sc != null)
            {
                WeightedMainTargetSelector(sc.CurrentSkillData.skillSo.selectCamp);
            }
        }
        else if (actionType == ActionType.Attack)
        {
            WeightedMainTargetSelector(SelectCampType.Enemy);
        }
    }

    public void ChoiceAction()
    {
        if (IsDead)
        {
            return;
        }

        if (ShouldUseSkill())
        {
            EnemySkillContorller sc = SkillController as EnemySkillContorller;
            if (sc != null)
            {
                sc.WeightedSelectSkill();
                ChangeAction(ActionType.Skill);
                SelectMainTarget(ActionType.Skill);
            }
        }
        else
        {
            ChangeAction(ActionType.Attack);
            SelectMainTarget(ActionType.Attack);
        }
    }


    public override void EnterMoveState()
    {
        ChangeUnitState(EnemyUnitState.Move);
    }

    public override void EnterAttackState()
    {
        ChangeUnitState(EnemyUnitState.Attack);
    }

    public override void EnterReturnState()
    {
        ChangeUnitState(EnemyUnitState.Return);
    }

    public override void EnterSkillState()
    {
        ChangeUnitState(EnemyUnitState.Skill);
    }

    protected override Enum GetHitStateEnum()
    {
        return EnemyUnitState.Hit;
    }

    public override void StartTurn()
    {
        List<Unit> enemies = BattleManager.Instance.GetEnemies(this);


        if (enemies.Count == 0 || IsDead || IsStunned || CurrentAction == ActionType.None || Target == null || Target.IsDead)
        {
            EndTurn();
            return;
        }

        ChangeTurnState(TurnStateType.StartTurn);
    }

    public override void EndTurn()
    {
        Target = null;
        ChangeAction(ActionType.None);
        ChangeUnitState(EnemyUnitState.Idle);
        SkillController.EndTurn();
        BattleManager.Instance.TurnHandler.OnUnitTurnEnd();
    }
}