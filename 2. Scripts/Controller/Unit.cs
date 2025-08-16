using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public abstract class Unit : MonoBehaviour, IDamageable, IAttackable, ISelectable, IUnitFsmControllable, IEffectProvider
{
    private const float ResistancePerStack = 0.08f;

    [SerializeField]
    protected BattleSceneUnitIndicator unitIndicator;


    protected BattleManager BattleManager => BattleManager.Instance;

    public BaseEmotion               CurrentEmotion { get; private set; }
    public BaseEmotion[]             Emotions       { get; private set; }
    public event Action<BaseEmotion> EmotionChanged; // 감정이 바뀌었을 때 알리는 이벤트
    public ActionType                CurrentAction    { get; private set; } = ActionType.None;
    public TurnStateMachine          TurnStateMachine { get; protected set; }
    public ITurnState[]              TurnStates       { get; private set; }
    public TurnStateType             CurrentTurnState { get; private set; }
    public StatManager               StatManager      { get; protected set; }

    public StatusEffectManager        StatusEffectManager        { get; protected set; }
    public SkillManager               SkillManager               { get; protected set; }
    public Animator                   Animator                   { get; protected set; }
    public BaseSkillController        SkillController            { get; protected set; }
    public AnimatorOverrideController AnimatorOverrideController { get; protected set; }
    public Collider                   Collider                   { get; protected set; }
    public NavMeshAgent               Agent                      { get; protected set; }
    public NavMeshObstacle            Obstacle                   { get; protected set; }
    public UnitSO                     UnitSo                     { get; protected set; }
    public AnimationEventListener     AnimationEventListener     { get; protected set; }
    public Unit                       CounterTarget              { get; private set; }
    public Unit                       LastAttacker               { get; private set; }

    public IDamageable Target { get; protected set; } //MainTarget, SubTarget => SkillController

    public IAttackAction CurrentAttackAction =>
        CurrentAction == ActionType.Skill
            ? SkillController?.CurrentSkillData?.skillSo?.SkillType
            : UnitSo?.AttackType;

    public         bool IsDead            { get; protected set; }
    public         bool IsCompletedAttack { get; protected set; }
    public         bool IsStunned         { get; private set; }
    public         bool IsCounterAttack   { get; private set; }
    public virtual bool IsAnimationDone   { get; set; }
    public virtual bool IsTimeLinePlaying { get; set; }

    public event Action OnHitFinished;
    public event Action OnMeleeAttackFinished;
    public event Action OnRangeAttackFinished;
    public event Action OnSkillFinished;
    public IDamageable  ResolvedActionTarget { get; private set; }
    public IDamageable  CurrentRawTarget     => IsCounterAttack ? CounterTarget : Target;
    public IDamageable  CurrentActionTarget  => ResolvedActionTarget ?? CurrentRawTarget;

    public event Action<Unit> FinalTargetLocked;

    public abstract event Action OnDead;


    public Unit SelectedUnit => this;


    // === 상태 전이 헬퍼(플레이어/적 분기 제거) ===
    public abstract void EnterMoveState();
    public abstract void EnterAttackState();
    public abstract void EnterReturnState();
    public abstract void EnterSkillState();

    public abstract void StartTurn();
    public abstract void EndTurn();

    public void UseSkill()
    {
        SkillController.UseSkill();
    }


    public abstract void Dead();


    public void SetStunned(bool isStunned)
    {
        IsStunned = isStunned;
    }

    protected void CreateEmotion()
    {
        Emotions = new BaseEmotion[Enum.GetValues(typeof(EmotionType)).Length];
        for (int i = 0; i < Emotions.Length; i++)
        {
            Emotions[i] = EmotionFactory.CreateEmotion((EmotionType)i);
        }

        CurrentEmotion = Emotions[(int)EmotionType.Neutral];

        // 스택 변경에 대한 반응 등록
        CurrentEmotion.StackChanged += OnEmotionStackChanged;
    }

    protected void CreateTurnStates()
    {
        TurnStates = new ITurnState[Enum.GetValues(typeof(TurnStateType)).Length];
        for (int i = 0; i < TurnStates.Length; i++)
        {
            TurnStates[i] = CreateTurnState((TurnStateType)i);
        }

        TurnStateMachine.Initialize(this, TurnStates[(int)TurnStateType.StartTurn]);
    }

    public void ChangeTurnState(TurnStateType state)
    {
        TurnStateMachine.ChangeState(TurnStates[(int)state]);
        CurrentTurnState = state;
    }

    private ITurnState CreateTurnState(TurnStateType state)
    {
        return state switch
        {
            TurnStateType.StartTurn    => new StartTurnState(),
            TurnStateType.MoveToTarget => new MoveToTargetState(),
            TurnStateType.Return       => new ReturnState(),
            TurnStateType.Act          => new ActState(),
            TurnStateType.EndTurn      => new EndTurnState(),
            _                          => null
        };
    }

    public void ChangeEmotion(EmotionType newType)
    {
        if (newType == EmotionType.None)
        {
            return;
        }

        if (CurrentEmotion.EmotionType != newType)
        {
            if (Random.value < CurrentEmotion.Stack * ResistancePerStack)
            {
                return;
            }

            // 이전 감정 스택 이벤트 제거
            CurrentEmotion.StackChanged -= OnEmotionStackChanged;

            CurrentEmotion?.Exit(this);
            CurrentEmotion = Emotions[(int)newType];
            CurrentEmotion.Enter(this);

            // 새 감정 스택 이벤트 연결
            CurrentEmotion.StackChanged += OnEmotionStackChanged;

            // 외부 알림
            EmotionChanged?.Invoke(CurrentEmotion);

            // 감정이 새로 바뀐 경우에 즉시 1스택에서 시작
            CurrentEmotion.AddStack(this);

            if (this is PlayerUnitController playerUnit && playerUnit.PassiveSo is IPassiveChangeEmotionTrigger passiveChangeEmotion)
            {
                passiveChangeEmotion.OnChangeEmotion();
            }
        }
        else
        {
            CurrentEmotion.AddStack(this);
        }
    }

    // 감정 스택이 바뀔 때마다 호출됨
    private void OnEmotionStackChanged(int newStack)
    {
        // Debug.Log($"{name}의 감정 스택이 {newStack}로 변경됨");
    }


    public abstract void PlayAttackVoiceSound();
    public abstract void PlayHitVoiceSound();
    public abstract void PlayDeadSound();

    public virtual void Attack()
    {
        IsCompletedAttack = false;
        IDamageable finalTarget = CurrentRawTarget;

        if (finalTarget == null || finalTarget.IsDead)
        {
            return;
        }

        float hitRate = StatManager.GetValue(StatType.HitRate);

        if (CurrentEmotion is IEmotionOnAttack emotionOnAttack)
        {
            emotionOnAttack.OnBeforeAttack(this, ref finalTarget);
        }
        else if (CurrentEmotion is IEmotionOnHitChance emotionOnHit)
        {
            emotionOnHit.OnCalculateHitChance(this, ref hitRate);
        }

        ResolvedActionTarget = finalTarget;
        if (ResolvedActionTarget is Unit finalUnit)
        {
            FinalTargetLocked?.Invoke(finalUnit);
        }

        bool isHit = Random.value < hitRate;


        if (!isHit)
        {
            DamageFontManager.Instance.SetDamageNumber(this, 0, DamageType.Miss);
            if (CurrentAttackAction.DistanceType == AttackDistanceType.Melee)
            {
                OnMeleeAttackFinished += InvokeHitFinished;
            }
            else
            {
                OnRangeAttackFinished += InvokeHitFinished;
                InvokeRangeAttackFinished();
            }

            return;
        }

        ResolvedActionTarget.SetLastAttacker(this);
        UnitSo.AttackType.Execute(this, finalTarget);
        IsCompletedAttack = true;
    }

    public virtual void TakeDamage(float amount, StatModifierType modifierType = StatModifierType.Base, bool isCritical = false)
    {
        if (IsDead)
        {
            return;
        }

        if (CurrentEmotion is IEmotionOnTakeDamage emotionOnTakeDamage)
        {
            emotionOnTakeDamage.OnBeforeTakeDamage(this, out bool isIgnore);
            if (isIgnore)
            {
                if (LastAttacker != null)
                {
                    if (LastAttacker.CurrentAction == ActionType.Attack)
                    {
                        if (LastAttacker.CurrentAttackAction.DistanceType == AttackDistanceType.Melee)
                        {
                            LastAttacker.OnMeleeAttackFinished += LastAttacker.InvokeHitFinished;
                        }
                        else
                        {
                            LastAttacker.InvokeHitFinished();
                        }
                    }
                }

                DamageFontManager.Instance.SetDamageNumber(this, 0, DamageType.Immune);
                return;
            }
        }

        StatusEffectManager?.TryTriggerAll(TriggerEventType.OnAttacked);

        float finalDam = amount;

        if (modifierType == StatModifierType.Base)
        {
            float defense   = StatManager.GetValue(StatType.Defense);
            float reduction = defense / (defense + Define.DefenseReductionBase);

            finalDam *= 1f - reduction;
        }

        ResourceStat curHp  = StatManager.GetStat<ResourceStat>(StatType.CurHp);
        ResourceStat shield = StatManager.GetStat<ResourceStat>(StatType.Shield);

        if (shield.CurrentValue > 0)
        {
            float shieldUsed = Mathf.Min(shield.CurrentValue, finalDam);
            StatManager.Consume(StatType.Shield, modifierType, shieldUsed);
            DamageFontManager.Instance.SetDamageNumber(this, shieldUsed, DamageType.Shield);
            finalDam -= shieldUsed;
        }

        if (finalDam > 0)
        {
            DamageFontManager.Instance.SetDamageNumber(this, finalDam, isCritical ? DamageType.Critical : DamageType.Normal);
            StatManager.Consume(StatType.CurHp, modifierType, finalDam);
        }

        if (curHp.Value <= 0)
        {
            Dead();
            return;
        }

        ChangeUnitState(GetHitStateEnum());
    }

    public void MoveTo(Vector3 destination)
    {
        Agent.SetDestination(destination);
    }

    protected abstract Enum GetHitStateEnum();


    public void SetTarget(IDamageable target)
    {
        Target = target;
        SkillController.SelectSkillSubTargets(target);
    }

    // 유닛 선택 가능 토글
    public void ToggleSelectableIndicator(bool toggle)
    {
        if (unitIndicator == null)
        {
            Debug.LogError("유닛에 unitIndicator을 추가해주세요.");
        }

        unitIndicator.ToggleSelectableIndicator(toggle);
    }

    // 유닛 선택됨 표시 토글
    public void ToggleSelectedIndicator(bool toggle)
    {
        if (unitIndicator == null)
        {
            Debug.LogError("유닛에 unitIndicator을 추가해주세요.");
        }

        unitIndicator.ToggleSelectedIndicator(toggle);
    }

    // 유닛 선택 파티클 재생
    public void PlaySelectEffect()
    {
        if (unitIndicator == null)
        {
            Debug.LogError("유닛에 unitIndicator을 추가해주세요.");
        }

        unitIndicator.PlaySelectEffect();
    }

    public void ChangeAction(ActionType action)
    {
        CurrentAction = action;
    }

    public void ExecuteCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public abstract void ChangeUnitState(Enum newState);

    public abstract void Initialize(UnitSpawnData spawnData);

    public void ChangeClip(string changedClipName, AnimationClip changeClip)
    {
        AnimatorOverrideController[changedClipName] = changeClip;
        Animator.runtimeAnimatorController = AnimatorOverrideController;
    }

    public Vector3 GetCenter()
    {
        return Collider.bounds.center;
    }

    public bool CanCounterAttack(Unit attacker)
    {
        if (IsDead || IsStunned)
        {
            return false;
        }

        if (attacker == null)
        {
            return false;
        }

        if (attacker.CurrentAttackAction.DistanceType == AttackDistanceType.Range)
        {
            return false;
        }

        if (attacker.CurrentAction == ActionType.Skill)
        {
            return false;
        }

        if (Random.value > StatManager.GetValue(StatType.Counter))
        {
            return false;
        }


        return true;
    }


    public void StartCounterAttack(Unit attacker)
    {
        CounterTarget = attacker;
        IsCounterAttack = true;
        attacker.SetLastAttacker(this);
        ChangeUnitState(this is PlayerUnitController ? PlayerUnitState.Attack : EnemyUnitState.Attack);


        if (CurrentAttackAction.DistanceType == AttackDistanceType.Melee)
        {
            OnMeleeAttackFinished += EndCounterAttack;
            OnMeleeAttackFinished += attacker.InvokeHitFinished;
        }
        else
        {
            OnRangeAttackFinished += EndCounterAttack;
            OnRangeAttackFinished += attacker.InvokeHitFinished;
        }
    }

    private void EndCounterAttack()
    {
        IsCounterAttack = false;
        CounterTarget = null;
    }

    public void OnToggleNavmeshAgent(bool isOn)
    {
        if (isOn)
        {
            Obstacle.carving = false;
            Obstacle.enabled = false;
            Agent.enabled = true;
        }
        else
        {
            Agent.enabled = false;
            Obstacle.enabled = true;
            Obstacle.carving = true;
        }
    }

    public void SetLastAttacker(IAttackable attacker)
    {
        LastAttacker = attacker as Unit;
    }

    private void ClearResolvedTarget()
    {
        ResolvedActionTarget = null;
    }

    public void InvokeHitFinished()
    {
        IsAnimationDone = true;
        OnHitFinished?.Invoke();
        OnHitFinished = null;
        ClearResolvedTarget();

        if (IsDead && LastAttacker != null)
        {
            bool attackerWaitingTimeline = LastAttacker.BattleManager
                                           || (LastAttacker.CurrentAction == ActionType.Skill &&
                                               LastAttacker.SkillController?.CurrentSkillData?.skillSo?.skillTimeLine != null);

            if (!attackerWaitingTimeline)
            {
                LastAttacker?.InvokeHitFinished();
            }
        }

        SetLastAttacker(null);
    }

    public void InvokeMeleeAttackFinished()
    {
        IsAnimationDone = true;
        OnMeleeAttackFinished?.Invoke();
        OnMeleeAttackFinished = null;
        ClearResolvedTarget();
    }

    public void InvokeRangeAttackFinished()
    {
        IsAnimationDone = true;
        OnRangeAttackFinished?.Invoke();
        OnRangeAttackFinished = null;
        ClearResolvedTarget();
    }

    public void InvokeSkillFinished()
    {
        IsAnimationDone = true;
        OnSkillFinished?.Invoke();
        OnSkillFinished = null;
        ClearResolvedTarget();
        CameraManager.Instance.ChangeMainCamera();
    }
}