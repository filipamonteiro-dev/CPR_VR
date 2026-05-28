using System.Collections;
using UnityEngine;

/// <summary>
/// Small runtime tester to exercise the tutorial flow in Play Mode.
/// Attach to any GameObject in the scene (or leave unassigned) and use
/// the component context menu "Run Tutorial Test" while in Play Mode.
/// It will advance steps with delays and log visible UI text via the
/// getters exposed on `TutorialFlowController`.
/// </summary>
public class TestTutorialRunner : MonoBehaviour
{
    [Tooltip("Reference to the StateMachine that drives the tutorial. If null, will find one automatically.")]
    [SerializeField] private StateMachine stateMachine;

    [Tooltip("Reference to the TutorialFlowController in the scene. If null, will find one automatically.")]
    public TutorialFlowController tutorialFlow;

    [Tooltip("Delay between steps while testing (seconds).")]
    public float stepDelay = 0.8f;

    private StateMachine subscribedStateMachine;

    private void OnEnable()
    {
        SubscribeToStateMachine();
    }

    private void Start()
    {
        if (subscribedStateMachine != null && subscribedStateMachine.CurrentState != null)
        {
            SyncTutorialToCurrentState();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromStateMachine();
    }

    [ContextMenu("Run Tutorial Test")]
    public void RunTest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[TestTutorialRunner] Enter Play Mode to run the test.");
            return;
        }

        StartCoroutine(RunSequence());
    }

    private void SubscribeToStateMachine()
    {
        UnsubscribeFromStateMachine();

        if (stateMachine == null)
        {
            stateMachine = FindAnyObjectByType<StateMachine>();
        }

        if (stateMachine == null)
        {
            Debug.LogWarning("[TestTutorialRunner] No StateMachine found in scene for tutorial sync.");
            return;
        }

        subscribedStateMachine = stateMachine;
        subscribedStateMachine.OnEnter += HandleStateMachineEnter;
        subscribedStateMachine.OnExit += HandleStateMachineExit;

        foreach (var state in subscribedStateMachine.StatesToExecute)
        {
            if (state != null)
            {
                state.OnEnter += HandleStateEnter;
            }
        }
    }

    private void UnsubscribeFromStateMachine()
    {
        if (subscribedStateMachine == null)
        {
            return;
        }

        subscribedStateMachine.OnEnter -= HandleStateMachineEnter;
        subscribedStateMachine.OnExit -= HandleStateMachineExit;

        foreach (var state in subscribedStateMachine.StatesToExecute)
        {
            if (state != null)
            {
                state.OnEnter -= HandleStateEnter;
            }
        }

        subscribedStateMachine = null;
    }

    private void HandleStateMachineEnter(State _)
    {
        SyncTutorialToCurrentState();
    }

    private void HandleStateMachineExit(State _)
    {
        if (tutorialFlow != null)
        {
            tutorialFlow.SetVisible(false);
        }
    }

    private void HandleStateEnter(State state)
    {
        SyncTutorialToState(state);
    }

    private void SyncTutorialToCurrentState()
    {
        if (subscribedStateMachine == null)
        {
            return;
        }

        if (subscribedStateMachine.CurrentState != null)
        {
            SyncTutorialToState(subscribedStateMachine.CurrentState);
        }
        else if (tutorialFlow != null)
        {
            tutorialFlow.ResetFlow();
            tutorialFlow.SetVisible(true);
        }
    }

    private void SyncTutorialToState(State state)
    {
        if (tutorialFlow == null)
        {
            tutorialFlow = FindAnyObjectByType<TutorialFlowController>();
        }

        if (tutorialFlow == null || subscribedStateMachine == null || state == null)
        {
            return;
        }

        int stepIndex = subscribedStateMachine.StatesToExecute.IndexOf(state);
        if (stepIndex < 0)
        {
            return;
        }

        tutorialFlow.SetVisible(true);
        tutorialFlow.SetStep(stepIndex);
    }

    private IEnumerator RunSequence()
    {
        if (tutorialFlow == null)
            tutorialFlow = FindAnyObjectByType<TutorialFlowController>();

        if (tutorialFlow == null)
        {
            Debug.LogError("[TestTutorialRunner] No TutorialFlowController found in scene.");
            yield break;
        }

        tutorialFlow.ResetFlow();
        yield return null; // allow one frame for UI to update

        int total = tutorialFlow.StepsCount;
        Debug.Log($"[TestTutorialRunner] StepsCount={total}");

        for (int i = 0; i < total; i++)
        {
            Debug.Log($"[TestTutorialRunner] Step {i + 1}/{total} — Index={tutorialFlow.CurrentStepIndex}");
            Debug.Log($"  Title: {tutorialFlow.GetCurrentTitle()}");
            Debug.Log($"  Label: {tutorialFlow.GetCurrentLabel()}");
            Debug.Log($"  Instr: {tutorialFlow.GetCurrentInstruction()}");
            Debug.Log($"  Counter: {tutorialFlow.GetCurrentCounter()}");

            yield return new WaitForSeconds(stepDelay);

            // Advance unless last
            if (i < total - 1)
                tutorialFlow.NextStep();
        }

        Debug.Log("[TestTutorialRunner] Completed sequence.");
    }
}
