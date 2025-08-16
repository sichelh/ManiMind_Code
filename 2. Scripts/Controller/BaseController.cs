using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(StatusEffectManager))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AnimationEventListener))]
[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseController<TController, TState> : Unit where TController : BaseController<TController, TState>
    where TState : Enum

{
    protected StateMachine<TController, TState> stateMachine;
    protected IState<TController, TState>[] states;
    public TState CurrentState { get; protected set; }

    protected virtual void Awake()
    {
        StatManager = GetComponent<StatManager>();
        StatusEffectManager = GetComponent<StatusEffectManager>();
        Collider = GetComponent<CapsuleCollider>();
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        Obstacle = GetComponent<NavMeshObstacle>();
        SkillManager = GetComponent<SkillManager>();
        stateMachine = new StateMachine<TController, TState>();
        AnimationEventListener = GetComponent<AnimationEventListener>();
        if (unitIndicator == null)
        {
            unitIndicator = GetComponentInChildren<BattleSceneUnitIndicator>();
        }

        CreateEmotion();
        SetupState();

        TurnStateMachine = new TurnStateMachine();
        CreateTurnStates();
    }

    protected virtual void Start()
    {
        // SkillManager.InitializeSkillManager(this);
    }

    protected virtual void Update()
    {
        // TryStateTransition();
        TurnStateMachine?.Update();
        stateMachine?.Update();
    }

    private void SetupState()
    {
        Array values = Enum.GetValues(typeof(TState));
        states = new IState<TController, TState>[values.Length];
        for (int i = 0; i < states.Length; i++)
        {
            TState state = (TState)values.GetValue(i);
            states[i] = GetState(state);
        }

        CurrentState = (TState)values.GetValue(0);
        stateMachine.Setup((TController)this, states[0]);
    }


    protected abstract IState<TController, TState> GetState(TState state);
}