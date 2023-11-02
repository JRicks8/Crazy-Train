using UnityEngine;

public interface IState
{
    public void Enter();
    public void Execute();
    public void Exit();
}

public class StateMachine
{
    IState currentState;

    public void ChangeState(IState newState)
    {
        currentState?.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Execute();
    }
}