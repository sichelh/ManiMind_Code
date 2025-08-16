using System;
using System.Collections.Generic;

// 인풋 상태머신
public class InputStateMachine
{
    private readonly Dictionary<Type, IInputState> states = new();
    private IInputState currentState;
    public IInputState CurrentState { get { return currentState; } }

    // state 새로 등록
    public void CreateInputState<T>(IInputState state) where T : IInputState
    {
        states[typeof(T)] = state;
    }

    // state 변경
    public void ChangeState<T>() where T : IInputState
    {
        if (states.TryGetValue(typeof(T), out var newState))
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }
        else
        {
            UnityEngine.Debug.LogError($"InputState {typeof(T)} 가 등록되지 않았습니다.");
        }
    }

    public void Update() => currentState?.HandleInput();
}

