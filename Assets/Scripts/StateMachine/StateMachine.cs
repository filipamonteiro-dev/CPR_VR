using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : State
{
    [SerializeField] protected List<State> m_statesToExecute = new List<State>();

    [Serializable]
    public class StateResolutionEntry
    {
        public int StateIndex;
        public string StateName;
        public string StateLabel;
        public bool Succeeded;
    }

    private readonly List<StateResolutionEntry> stateResolutionHistory = new List<StateResolutionEntry>();

    private State currentState;
    private int currentStateIndex = -1;
    private bool currentStateOutcome = true;
    private bool currentStateOutcomeExplicitlySet;
    private bool currentStateOutcomeRecorded;

    public event Action<State, bool> OnStateResolved;

    public List<State> StatesToExecute => m_statesToExecute;
    public IReadOnlyList<StateResolutionEntry> StateResolutionHistory => stateResolutionHistory;
    public bool ResolutionsWereInExpectedOrder => CheckResolutionOrder();
    public int CurrentStateIndex => currentStateIndex;
    public State CurrentState => currentState;

    public override State GetState()
    {
        if (currentState)
        {
            return currentState;
        }

        return this;
    }

    public override void Enter()
    {
        base.Enter();

        foreach (State state in m_statesToExecute)
        {
            if (state != null)
            {
                state.OnAskToBeActive += State_OnAskToBeActive;
            }
        }

        currentStateIndex = -1;
        MoveToNextState();
    }

    private void State_OnAskToBeActive(State obj)
    {
        AskToBeActive();

        int index = m_statesToExecute.IndexOf(obj);
        if (index >= 0)
        {
            SetState(index);
        }
    }

    public override void Execute()
    {
        if (currentState == null)
        {
            return;
        }

        if (currentState.IsFinished())
        {
            if (!currentStateOutcomeExplicitlySet)
            {
                currentStateOutcome = currentState.IsSuccessfulCompletion();
            }

            RecordCurrentStateOutcome();
            MoveToNextState();
            return;
        }

        currentState.Execute();
    }

    protected virtual void MoveToNextState()
    {
        if (m_statesToExecute == null || m_statesToExecute.Count == 0)
        {
            Exit();
            return;
        }

        int nextIndex = currentStateIndex + 1;
        if (nextIndex < m_statesToExecute.Count)
        {
            SetState(nextIndex);
            return;
        }

        Exit();
    }

    public override void Exit()
    {
        base.Exit();

        foreach (State state in m_statesToExecute)
        {
            if (state != null)
            {
                state.OnAskToBeActive -= State_OnAskToBeActive;
            }
        }

        SetState(-1);
    }

    public override bool IsFinished()
    {
        if (m_statesToExecute == null || m_statesToExecute.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < m_statesToExecute.Count; i++)
        {
            if (m_statesToExecute[i] != null && !m_statesToExecute[i].IsFinished())
            {
                return false;
            }
        }

        return true;
    }

    public virtual void SetState(int index)
    {
        if (currentState != null && (index < 0 || index >= m_statesToExecute.Count || m_statesToExecute[index] != currentState))
        {
            RecordCurrentStateOutcome();
            currentState.SetExitOutcome(currentStateOutcome);
            currentState.Exit();
        }

        if (index < 0)
        {
            currentState = null;
            currentStateIndex = -1;
            return;
        }

        if (index >= m_statesToExecute.Count)
        {
            return;
        }

        if (m_statesToExecute[index] == currentState)
        {
            return;
        }

        currentStateIndex = index;
        currentState = m_statesToExecute[currentStateIndex];
        ResetCurrentStateOutcome();
        currentState?.Enter();
    }

    public void SkipState()
    {
        FailCurrentState();
    }

    public void FailCurrentState()
    {
        CompleteCurrentState(false);
    }

    public void CompleteCurrentState()
    {
        CompleteCurrentState(true);
    }

    public void CompleteCurrentState(bool succeeded)
    {
        if (currentState == null)
        {
            return;
        }

        currentStateOutcome = succeeded;
        currentStateOutcomeRecorded = false;
        currentState.ForceFinished();
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/Complete Current State")]
    private void DebugCompleteCurrentState()
    {
        SkipState();
    }

    [ContextMenu("Debug/Complete All States")]
    private void DebugCompleteAllStates()
    {
        ForceFinished();
    }
#endif

    public override void ForceFinished()
    {
        foreach (var state in m_statesToExecute)
        {
            if (state != null)
            {
                state.ForceFinished();
            }
        }
    }

    private void ResetCurrentStateOutcome()
    {
        currentStateOutcome = true;
        currentStateOutcomeExplicitlySet = false;
        currentStateOutcomeRecorded = false;
    }

    private void RecordCurrentStateOutcome()
    {
        if (currentState == null || currentStateOutcomeRecorded)
        {
            return;
        }

        currentStateOutcomeRecorded = true;

        var entry = new StateResolutionEntry
        {
            StateIndex = currentStateIndex,
            StateName = currentState.name,
            StateLabel = string.IsNullOrWhiteSpace(currentState.StateLabel) ? currentState.name : currentState.StateLabel,
            Succeeded = currentStateOutcome
        };

        stateResolutionHistory.Add(entry);
        OnStateResolved?.Invoke(currentState, currentStateOutcome);
    }

    private bool CheckResolutionOrder()
    {
        int lastIndex = -1;

        foreach (var entry in stateResolutionHistory)
        {
            if (entry.StateIndex < lastIndex)
            {
                return false;
            }

            lastIndex = entry.StateIndex;
        }

        return true;
    }

#if UNITY_EDITOR
    [ContextMenu("Add States from Child")]
    void AddStatesFromChild()
    {
        StatesToExecute.Clear();
        foreach (Transform t in transform)
        {
            if (t.TryGetComponent(out State state))
            {
                StatesToExecute.Add(state);
            }
        }
    }
#endif
}
