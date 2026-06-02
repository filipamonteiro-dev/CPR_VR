using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class TutorialStateTimer : MonoBehaviour
{
    private const string BaselinePrefsKey = "TutorialStateTimer.Baseline";

    [SerializeField] private StateMachine m_StateMachine;

    [Header("Persistence")]
    [SerializeField] private bool m_DontDestroyOnLoad = true;
    [SerializeField] private bool m_AutoBindStateMachine = true;

    [Header("Validation")]
    [SerializeField] private bool m_StrictTutorialValidation = true;

    [Header("Baseline")]
    [SerializeField] private string m_BaselineSceneName = "TutorialRoom";

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

    [System.Serializable]
    private class BaselineEntry
    {
        public string StateLabel;
        public float Duration;
    }

    [System.Serializable]
    private class BaselineSnapshot
    {
        public string SceneName;
        public string StateMachineName;
        public List<BaselineEntry> Entries = new List<BaselineEntry>();
    }

    private static TutorialStateTimer s_Instance;

    private readonly Dictionary<State, StateTimingEntry> m_ActiveEntries = new Dictionary<State, StateTimingEntry>();
    private readonly List<SessionSnapshot> m_Sessions = new List<SessionSnapshot>();

    private SessionSnapshot m_CurrentSession;
    private BaselineSnapshot m_BaselineSnapshot;
    private bool m_BaselineSavedForSession;

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

        LoadBaselineFromPrefs();
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

        var baselineSession = GetBaselineSession();
        if ((baselineSession == null || baselineSession.Entries.Count == 0) && (m_BaselineSnapshot == null || m_BaselineSnapshot.Entries.Count == 0))
        {
            return "No tutorial baseline available.";
        }

        var baselineTimes = new Dictionary<string, float>();
        if (m_BaselineSnapshot != null && m_BaselineSnapshot.Entries.Count > 0)
        {
            foreach (var entry in m_BaselineSnapshot.Entries)
            {
                if (!baselineTimes.ContainsKey(entry.StateLabel))
                {
                    baselineTimes[entry.StateLabel] = entry.Duration;
                }
            }
        }
        else if (baselineSession != null)
        {
            foreach (var entry in baselineSession.Entries)
            {
                if (!baselineTimes.ContainsKey(entry.StateLabel))
                {
                    baselineTimes[entry.StateLabel] = entry.Duration;
                }
            }
        }

        if (m_CurrentSession.Entries.Count == 0)
        {
            return "No steps recorded yet.";
        }

        var builder = new StringBuilder();
        builder.AppendLine("// COMPARACAO COM TUTORIAL //");

        foreach (var entry in m_CurrentSession.Entries)
        {
            float current = entry.Duration;
            if (baselineTimes.TryGetValue(entry.StateLabel, out float tutorial))
            {
                float diff = current - tutorial;
                string diffText = diff >= 0f ? $"+{diff:0.00}s" : $"{diff:0.00}s";
                builder.AppendLine($"{entry.StateLabel}: {current:0.00}s (tutorial {tutorial:0.00}s, {diffText})");
            }
            else
            {
                builder.AppendLine($"{entry.StateLabel}: {current:0.00}s (tutorial n/a)");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private SessionSnapshot GetBaselineSession()
    {
        if (m_Sessions == null || m_Sessions.Count < 2)
        {
            return null;
        }

        for (int i = m_Sessions.Count - 2; i >= 0; i--)
        {
            var session = m_Sessions[i];
            if (session != null && session.Entries.Count > 0)
            {
                return session;
            }
        }

        return null;
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

        m_BaselineSavedForSession = false;

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

        if (ShouldSaveBaseline() && !m_BaselineSavedForSession && m_StateMachine != null && m_StateMachine.IsFinished())
        {
            SaveBaselineFromCurrentSession();
            m_BaselineSavedForSession = true;
        }
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

    private bool ShouldSaveBaseline()
    {
        if (string.IsNullOrWhiteSpace(m_BaselineSceneName))
        {
            return true;
        }

        return string.Equals(SceneManager.GetActiveScene().name, m_BaselineSceneName, System.StringComparison.Ordinal);
    }

    private void SaveBaselineFromCurrentSession()
    {
        if (m_CurrentSession == null || m_CurrentSession.Entries.Count == 0)
        {
            return;
        }

        var snapshot = new BaselineSnapshot
        {
            SceneName = m_CurrentSession.SceneName,
            StateMachineName = m_CurrentSession.StateMachineName
        };

        foreach (var entry in m_CurrentSession.Entries)
        {
            snapshot.Entries.Add(new BaselineEntry
            {
                StateLabel = entry.StateLabel,
                Duration = entry.Duration
            });
        }

        m_BaselineSnapshot = snapshot;
        string json = JsonUtility.ToJson(snapshot);
        PlayerPrefs.SetString(BaselinePrefsKey, json);
        PlayerPrefs.Save();
    }

    private void LoadBaselineFromPrefs()
    {
        if (!PlayerPrefs.HasKey(BaselinePrefsKey))
        {
            return;
        }

        string json = PlayerPrefs.GetString(BaselinePrefsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        var snapshot = JsonUtility.FromJson<BaselineSnapshot>(json);
        if (snapshot != null && snapshot.Entries != null && snapshot.Entries.Count > 0)
        {
            m_BaselineSnapshot = snapshot;
        }
    }
}
