using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class TutorialStateTimer : MonoBehaviour
{
    [SerializeField] private StateMachine m_StateMachine;

    [Header("Persistence")]
    [SerializeField] private bool m_DontDestroyOnLoad = true;
    [SerializeField] private bool m_AutoBindStateMachine = true;

    [Header("Validation")]
    [SerializeField] private bool m_StrictTutorialValidation = true;

    [System.Serializable]
    public class StateTimingEntry
    {
        public int StateIndex;
        public string StateName;
        public string StateLabel;
        public float EnterTime;
        public float ExitTime;
        public float CompletionProgress;
        public bool Succeeded = true;

        public float Duration => ExitTime > EnterTime ? ExitTime - EnterTime : 0f;
    }

    [System.Serializable]
    public class SessionSnapshot
    {
        public string SceneName;
        public string StateMachineName;
        public List<string> ExpectedOrder = new List<string>();
        public List<StateTimingEntry> Entries = new List<StateTimingEntry>();

        public bool IsInExpectedOrder()
        {
            int lastIndex = -1;

            for (int i = 0; i < Entries.Count; i++)
            {
                int index = Entries[i].StateIndex;
                if (index < lastIndex)
                {
                    return false;
                }

                lastIndex = index;
            }

            return true;
        }
    }

    private static TutorialStateTimer s_Instance;

    private readonly Dictionary<State, StateTimingEntry> m_ActiveEntries = new Dictionary<State, StateTimingEntry>();
    private readonly List<SessionSnapshot> m_Sessions = new List<SessionSnapshot>();

    private SessionSnapshot m_CurrentSession;

    public static TutorialStateTimer Instance => s_Instance;
    public IReadOnlyList<SessionSnapshot> Sessions => m_Sessions;

    public bool StrictTutorialValidation => m_StrictTutorialValidation;

    private void Awake()
    {
        if (s_Instance != null && s_Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        s_Instance = this;

        if (m_DontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        TryAutoBind();
    }

    private void Start()
    {
        TryAutoBind();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnbindFromStateMachine();
    }

    private void OnDestroy()
    {
        if (s_Instance == this)
        {
            s_Instance = null;
        }

        UnbindFromStateMachine();
    }

    public void BindToStateMachine(StateMachine stateMachine)
    {
        if (stateMachine == null)
        {
            return;
        }

        if (m_StateMachine == stateMachine && m_CurrentSession != null)
        {
            return;
        }

        if (m_StateMachine != null && m_StateMachine != stateMachine)
        {
            UnsubscribeFromStateMachine();
        }

        m_StateMachine = stateMachine;
        BeginSession(m_StateMachine);

        foreach (var state in m_StateMachine.StatesToExecute)
        {
            if (state == null)
            {
                continue;
            }

            state.OnEnter += HandleStateEnter;
            state.OnExit += HandleStateExit;
        }

        m_StateMachine.OnStateResolved += HandleStateResolved;
    }
    
    public void SetStrictTutorialValidation(bool strictValidation)
    {
        m_StrictTutorialValidation = strictValidation;
    }

    public string GetLatestComparisonSummary()
    {
        if (m_CurrentSession == null)
        {
            return "No tutorial session recorded yet.";
        }

        var actualOrder = new List<string>();
        foreach (var entry in m_CurrentSession.Entries)
        {
            actualOrder.Add(entry.StateLabel);
        }

        var missing = new List<string>();
        var remainingActual = new List<string>(actualOrder);
        var failed = new List<string>();
        var progressParts = new List<string>();

        foreach (var entry in m_CurrentSession.Entries)
        {
            progressParts.Add($"{entry.StateLabel}: {(entry.CompletionProgress * 100f):0}%");

            if (!entry.Succeeded)
            {
                failed.Add(entry.StateLabel);
            }
        }

        foreach (var expected in m_CurrentSession.ExpectedOrder)
        {
            if (remainingActual.Contains(expected))
            {
                remainingActual.Remove(expected);
                continue;
            }

            missing.Add(expected);
        }

        return $"Expected: {string.Join(" -> ", m_CurrentSession.ExpectedOrder)}\nActual: {string.Join(" -> ", actualOrder)}\nOrder: {(m_CurrentSession.IsInExpectedOrder() ? "yes" : "no")}\nFailed: {(failed.Count > 0 ? string.Join(", ", failed) : "none")}\nProgress: {(progressParts.Count > 0 ? string.Join(", ", progressParts) : "none")}\nMissing: {(missing.Count > 0 ? string.Join(", ", missing) : "none")}";
    }

    private void TryAutoBind()
    {
        if (!m_AutoBindStateMachine)
        {
            return;
        }

        if (m_StateMachine != null)
        {
            BindToStateMachine(m_StateMachine);
            return;
        }

        var foundMachine = FindAnyObjectByType<StateMachine>();
        if (foundMachine != null)
        {
            BindToStateMachine(foundMachine);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!m_AutoBindStateMachine)
        {
            return;
        }

        var foundMachine = FindAnyObjectByType<StateMachine>();
        if (foundMachine != null)
        {
            BindToStateMachine(foundMachine);
        }
    }

    private void UnbindFromStateMachine()
    {
        if (m_StateMachine == null)
        {
            return;
        }

        UnsubscribeFromStateMachine();
        m_StateMachine = null;
    }

    private void UnsubscribeFromStateMachine()
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

        m_StateMachine.OnStateResolved -= HandleStateResolved;
    }

    private void BeginSession(StateMachine stateMachine)
    {
        m_CurrentSession = new SessionSnapshot
        {
            SceneName = SceneManager.GetActiveScene().name,
            StateMachineName = stateMachine != null ? stateMachine.name : "Unknown"
        };

        m_Sessions.Add(m_CurrentSession);

        if (stateMachine == null)
        {
            return;
        }

        foreach (var state in stateMachine.StatesToExecute)
        {
            if (state == null)
            {
                continue;
            }

            m_CurrentSession.ExpectedOrder.Add(GetStateLabel(state));
        }
    }

    private void HandleStateEnter(State state)
    {
        if (state == null || m_CurrentSession == null)
        {
            return;
        }

        var entry = new StateTimingEntry
        {
            StateIndex = m_StateMachine != null ? m_StateMachine.StatesToExecute.IndexOf(state) : -1,
            StateName = state.name,
            StateLabel = GetStateLabel(state),
            EnterTime = Time.time,
            CompletionProgress = 0f
        };

        m_ActiveEntries[state] = entry;
        m_CurrentSession.Entries.Add(entry);
    }

    private void HandleStateExit(State state)
    {
        if (state == null)
        {
            return;
        }

        if (!m_ActiveEntries.TryGetValue(state, out var entry))
        {
            return;
        }

        entry.ExitTime = Time.time;
        entry.CompletionProgress = m_StrictTutorialValidation
            ? (entry.Succeeded ? 1f : 0f)
            : Mathf.Clamp01(state.GetCompletionProgress());

        string label = string.IsNullOrWhiteSpace(state.StateLabel) ? state.name : state.StateLabel;
        Debug.Log($"[TutorialStateTimer] State '{label}' completed in {entry.Duration:0.000}s. Success={entry.Succeeded}", this);

        m_ActiveEntries.Remove(state);
    }

    private void HandleStateResolved(State state, bool succeeded)
    {
        if (state == null)
        {
            return;
        }

        if (m_ActiveEntries.TryGetValue(state, out var entry))
        {
            entry.Succeeded = succeeded;
        }
    }

    private static string GetStateLabel(State state)
    {
        if (state == null)
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(state.StateLabel) ? state.name : state.StateLabel;
    }
}
