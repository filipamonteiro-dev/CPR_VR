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
    [Tooltip("Reference to the AppStartup that begins the tutorial. If null, will find one automatically.")]
    [SerializeField] private AppStartup appStartup;

    [Tooltip("Reference to the StateMachine that drives the tutorial. If null, will find one automatically.")]
    [SerializeField] private StateMachine stateMachine;

    [Tooltip("Reference to the TutorialFlowController in the scene. If null, will find one automatically.")]
    public TutorialFlowController tutorialFlow;

    [Tooltip("Delay between steps while testing (seconds).")]
    public float stepDelay = 0.8f;

    private StateMachine subscribedStateMachine;
    private bool isStateMachineRunning;
    private State lastSyncedState;

    private void OnEnable()
    {
        if (appStartup == null)
        {
            appStartup = FindAnyObjectByType<AppStartup>();
        }

        if (appStartup != null)
        {
            appStartup.TutorialStarted += HandleTutorialStarted;
        }

        SubscribeToStateMachine();

        if (tutorialFlow == null)
        {
            tutorialFlow = FindAnyObjectByType<TutorialFlowController>();
        }

        if (tutorialFlow != null)
        {
            tutorialFlow.SetVisible(false);
        }
    }

    private void Start()
    {
        if (subscribedStateMachine != null && subscribedStateMachine.CurrentState != null)
        {
            isStateMachineRunning = true;
            SyncTutorialToCurrentState();
        }
    }

    private void Update()
    {
        if (!isStateMachineRunning || subscribedStateMachine == null)
        {
            return;
        }

        var currentState = subscribedStateMachine.CurrentState;
        if (currentState != null && currentState != lastSyncedState)
        {
            SyncTutorialToState(currentState);
            lastSyncedState = currentState;
        }
    }

    private void OnDisable()
    {
        if (appStartup != null)
        {
            appStartup.TutorialStarted -= HandleTutorialStarted;
        }

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
            var machines = FindObjectsByType<StateMachine>(FindObjectsSortMode.None);
            if (machines.Length == 1)
            {
                stateMachine = machines[0];
            }
            else if (machines.Length > 1)
            {
                Debug.LogWarning("[TestTutorialRunner] Multiple StateMachine instances found. Assign one explicitly.");
                return;
            }
        }

        if (stateMachine == null)
        {
            Debug.LogWarning("[TestTutorialRunner] No StateMachine found in scene for tutorial sync.");
            return;
        }

        subscribedStateMachine = stateMachine;
        isStateMachineRunning = false;
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
        isStateMachineRunning = false;
        lastSyncedState = null;
    }

    private void HandleStateMachineEnter(State _)
    {
        isStateMachineRunning = true;
        SyncTutorialToCurrentState();
    }

    private void HandleStateMachineExit(State _)
    {
        isStateMachineRunning = false;
        lastSyncedState = null;
        if (tutorialFlow != null)
        {
            tutorialFlow.SetVisible(false);
        }
    }

    private void HandleStateEnter(State state)
    {
        if (!isStateMachineRunning)
        {
            return;
        }

        SyncTutorialToState(state);
        lastSyncedState = state;
    }

    private void HandleTutorialStarted(StateMachine startedMachine)
    {
        if (startedMachine == null || subscribedStateMachine != startedMachine)
        {
            return;
        }

        isStateMachineRunning = true;
        lastSyncedState = null;
        SyncTutorialToCurrentState();
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
