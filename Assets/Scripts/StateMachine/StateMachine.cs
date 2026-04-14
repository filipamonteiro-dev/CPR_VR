using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachine : State
{
    [SerializeField] protected List<State> m_statesToExecute = new();
    public List<State> StatesToExecute => m_statesToExecute;

    public override State GetState()
    {
        if (currentState)
            return currentState;

        return this;
    }

    State currentState;
    int currentStateIndex = -1;

    public int CurrentStateIndex => currentStateIndex;
    public State CurrentState => currentState;

    public override void Enter()
    {
        base.Enter();

        foreach (State m_State in m_statesToExecute)
            m_State.OnAskToBeActive += State_OnAskToBeActive;

        currentStateIndex = -1;
        MoveToNextState();
    }

    private void State_OnAskToBeActive(State obj)
    {
        AskToBeActive();
        int index = m_statesToExecute.IndexOf(obj);
        if (index >= 0)
            SetState(index);
    }

    public override void Execute()
    {
        if (currentState != null)
        {
            if (currentState.IsFinished())
            {
                MoveToNextState();
                return;
            }

            currentState.Execute();
        }
    }

    protected virtual void MoveToNextState()
    {


        if (m_statesToExecute != null || m_statesToExecute.Count > 0)
        {
            if (currentStateIndex + 1 < m_statesToExecute.Count)
            {
                SetState(currentStateIndex + 1);
            }
            else
            {
                Exit();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        foreach (State m_State in m_statesToExecute)
            m_State.OnAskToBeActive -= State_OnAskToBeActive;

        SetState(-1);
    }

    public override bool IsFinished()
    {
        if (m_statesToExecute == null || m_statesToExecute.Count == 0)
            return true;

        for (int i = 0; i < m_statesToExecute.Count; i++)
        {
            if (!m_statesToExecute[i].IsFinished())
                return false;
        }

        return true;
    }

    public virtual void SetState(int index)
    {
        if (currentState != null && (index < 0 || m_statesToExecute[index] != currentState))
            currentState.Exit();

        if (index < 0)
        {
            currentState = null;
            return;
        }

        if (m_statesToExecute[index] == currentState)
            return;

        if (index < m_statesToExecute.Count)
        {
            currentStateIndex = index;
            currentState = m_statesToExecute[currentStateIndex];

            currentState?.Enter();
        }
    }

    public void SkipState()
    {
        if (currentState)
            currentState.ForceFinished();
    }

    public override void ForceFinished()
    {
        foreach (var state in m_statesToExecute)
            state.ForceFinished();
    }


#if UNITY_EDITOR
    [ContextMenu("Add States from Child")]
    void AddStatesFromChild()
    {
        StatesToExecute.Clear();
        foreach (Transform t in transform)
        {
            if (t.TryGetComponent(out State state))
                StatesToExecute.Add(state);
        }
    }
#endif
}
