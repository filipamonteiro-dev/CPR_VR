using System.Collections.Generic;
using UnityEngine;

public class TutorialStateTimer : MonoBehaviour
{
    [SerializeField] private StateMachine m_StateMachine;

    private readonly Dictionary<State, float> m_StateStartTimes = new();
    private readonly Dictionary<State, float> m_StateDurations = new();

    private void OnEnable()
    {
        if (m_StateMachine == null)
        {
            Debug.LogWarning("TutorialStateTimer requires a StateMachine reference.", this);
            return;
        }

        foreach (var state in m_StateMachine.StatesToExecute)
        {
            if (state == null)
            {
                continue;
            }

            state.OnEnter += HandleStateEnter;
            state.OnExit += HandleStateExit;
        }
    }

    private void OnDisable()
    {
        if (m_StateMachine == null)
        {
            return;
        }

        foreach (var state in m_StateMachine.StatesToExecute)
        {
            if (state == null)
            {
                continue;
            }

            state.OnEnter -= HandleStateEnter;
            state.OnExit -= HandleStateExit;
        }
    }

    private void HandleStateEnter(State state)
    {
        if (state == null)
        {
            return;
        }

        m_StateStartTimes[state] = Time.time;
    }

    private void HandleStateExit(State state)
    {
        if (state == null)
        {
            return;
        }

        if (!m_StateStartTimes.TryGetValue(state, out var startTime))
        {
            return;
        }

        float elapsed = Time.time - startTime;
        m_StateDurations[state] = elapsed;

        string label = string.IsNullOrWhiteSpace(state.StateLabel) ? state.name : state.StateLabel;
        Debug.Log($"[TutorialStateTimer] State '{label}' completed in {elapsed:0.000}s.", this);
    }
}
