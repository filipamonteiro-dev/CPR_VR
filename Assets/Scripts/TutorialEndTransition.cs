using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialEndTransition : MonoBehaviour
{
    [SerializeField] private StateMachine m_StateMachine;
    [SerializeField] private FadeScript m_FadeScript;
    [SerializeField] private string m_MainLevelScene = "MainLevel";
    [SerializeField] private float m_LoadDelay = 0.25f;
    [SerializeField] private Camera m_IntroCamera;
    [SerializeField] private float m_IntroDistanceFromPlayer = 1.5f;
    [SerializeField] private float m_IntroVerticalOffset = -0.15f;

    [Header("Main Level Intro")]
    [SerializeField] private bool m_ShowIntroMessage = true;
    [TextArea(3, 6)]
    [SerializeField] private string m_IntroMessage = "Completaste o tutorial, agora vais poder testar as tuas habilidades numa situação real, boa sorte";
    [SerializeField] private float m_IntroHoldDuration = 2.2f;
    [SerializeField] private float m_IntroFadeDuration = 0.75f;
    [SerializeField] private bool m_MainLevelStrictValidation = false;
    [SerializeField] private TMP_FontAsset m_FontAsset;

    [Header("Main Level Execution")]
    [SerializeField] private bool m_RunMainLevelMachineInThisScript = true;

    [Header("Main Level Results")]
    [SerializeField] private bool m_ShowResultsScreen = true;
    [SerializeField] private float m_ResultsHoldDuration = 0f;
    [SerializeField] private float m_ResultsFadeDuration = 0.75f;

    private bool m_HasStarted;
    private bool m_HasTriggered;
    private bool m_MainLevelStarted;
    private bool m_ResultsRequested;
    private bool m_DrivingMainLevelMachine;
    private GameObject m_IntroRoot;
    private CanvasGroup m_IntroGroup;
    private RectTransform m_IntroRect;
    private GameObject m_ResultsRoot;
    private CanvasGroup m_ResultsGroup;
    private RectTransform m_ResultsRect;
    private TextMeshProUGUI m_ResultsTitleText;
    private TextMeshProUGUI m_ResultsBodyText;
    private StateMachine m_MainLevelMachine;
    private CPRCompressionRhythmValidator m_RhythmValidator;
    private TutorialStateTimer m_TutorialTimer;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Update()
    {
        PositionIntroOverlay();
        PositionResultsOverlay();

        if (m_DrivingMainLevelMachine && m_MainLevelMachine != null)
        {
            if (!m_MainLevelMachine.IsFinished())
            {
                m_MainLevelMachine.Execute();
            }
            else
            {
                m_DrivingMainLevelMachine = false;
            }
        }

        if (m_ShowResultsScreen && m_MainLevelStarted && m_MainLevelMachine != null && !m_ResultsRequested && m_MainLevelMachine.IsFinished())
        {
            StartCoroutine(ShowMainLevelResults());
        }

        if (m_HasTriggered || m_StateMachine == null)
        {
            return;
        }

        if (!m_HasStarted)
        {
            if (m_StateMachine.CurrentState != null || !m_StateMachine.IsFinished())
            {
                m_HasStarted = true;
            }
            else
            {
                return;
            }
        }

        if (!m_StateMachine.IsFinished())
        {
            return;
        }

        BeginTransition();
    }

    private void BeginTransition()
    {
        m_HasTriggered = true;
        m_MainLevelStarted = false;
        m_ResultsRequested = false;
        m_DrivingMainLevelMachine = false;

        if (m_ShowIntroMessage)
        {
            CreateIntroOverlay();
            if (m_IntroGroup != null)
            {
                m_IntroGroup.alpha = 1f;
            }
        }

        if (m_FadeScript != null)
        {
            m_FadeScript.Fade(true);
        }

        DontDestroyOnLoad(gameObject);

        if (m_LoadDelay > 0f)
        {
            StartCoroutine(LoadMainLevelAfterDelay());
            return;
        }

        LoadMainLevel();
    }

    private IEnumerator LoadMainLevelAfterDelay()
    {
        yield return new WaitForSeconds(m_LoadDelay);
        LoadMainLevel();
    }

    private void LoadMainLevel()
    {
        if (string.IsNullOrWhiteSpace(m_MainLevelScene))
        {
            return;
        }

        SceneManager.LoadScene(m_MainLevelScene);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!m_HasTriggered || !string.Equals(scene.name, m_MainLevelScene, System.StringComparison.Ordinal))
        {
            return;
        }

        StartCoroutine(PlayMainLevelIntro());
    }

    private IEnumerator PlayMainLevelIntro()
    {
        yield return null;

        PositionIntroOverlay();

        m_MainLevelMachine = FindMainLevelStateMachine();
        var mainLevelStartup = FindMainLevelAppStartup();
        m_RhythmValidator = FindAnyObjectByType<CPRCompressionRhythmValidator>();
        m_TutorialTimer = TutorialStateTimer.Instance != null
            ? TutorialStateTimer.Instance
            : FindAnyObjectByType<TutorialStateTimer>();

        if (m_ShowIntroMessage && m_IntroGroup != null)
        {
            m_IntroGroup.alpha = 1f;
        }

        if (m_TutorialTimer != null)
        {
            m_TutorialTimer.SetStrictTutorialValidation(m_MainLevelStrictValidation);
            if (m_MainLevelMachine != null)
            {
                m_TutorialTimer.BindToStateMachine(m_MainLevelMachine);
            }
        }

        yield return new WaitForSeconds(m_IntroHoldDuration);

        if (mainLevelStartup != null)
        {
            mainLevelStartup.StartTutorialExternal();
            m_MainLevelStarted = true;
            m_DrivingMainLevelMachine = m_RunMainLevelMachineInThisScript && !HasExternalMainLevelRunner();
        }
        else if (m_MainLevelMachine != null)
        {
            m_MainLevelMachine.Enter();
            m_MainLevelStarted = true;
            m_DrivingMainLevelMachine = m_RunMainLevelMachineInThisScript && !HasExternalMainLevelRunner();
        }

        if (m_ShowIntroMessage && m_IntroGroup != null)
        {
            yield return FadeIntroOut();
        }

        DestroyIntroOverlay();
    }

    private IEnumerator ShowMainLevelResults()
    {
        m_ResultsRequested = true;

        if (m_ResultsHoldDuration > 0f)
        {
            yield return new WaitForSeconds(m_ResultsHoldDuration);
        }

        CreateResultsOverlay();
        UpdateResultsOverlay();

        if (m_ResultsGroup != null)
        {
            m_ResultsGroup.alpha = 1f;
        }

        if (m_ResultsFadeDuration > 0f)
        {
            yield return FadeResultsIn();
        }
    }

    private void CreateIntroOverlay()
    {
        if (m_IntroRoot != null)
        {
            return;
        }

        var root = new GameObject("MainLevelIntroCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        DontDestroyOnLoad(root);

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 5000;

        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500f, 220f);

        m_IntroGroup = root.GetComponent<CanvasGroup>();
        m_IntroGroup.alpha = 0f;
        m_IntroGroup.interactable = false;
        m_IntroGroup.blocksRaycasts = false;
        m_IntroRect = rect;

        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);

        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var background = panel.AddComponent<Image>();
        background.color = new Color(0.02f, 0.03f, 0.06f, 0.94f);

        var messageBox = new GameObject("MessageBox");
        messageBox.transform.SetParent(panel.transform, false);

        var boxRect = messageBox.AddComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(820f, 260f);
        boxRect.anchoredPosition = Vector2.zero;

        var boxImage = messageBox.AddComponent<Image>();
        boxImage.color = new Color(1f, 1f, 1f, 0.04f);
        var outline = messageBox.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.12f);
        outline.effectDistance = new Vector2(1f, -1f);

        CreateText(messageBox.transform, "MAIN LEVEL", 9f, new Color(1f, 1f, 1f, 0.35f), new Vector2(0f, 72f), new Vector2(560f, 18f), TextAlignmentOptions.Center);
        CreateText(messageBox.transform, m_IntroMessage, 24f, Color.white, new Vector2(0f, -6f), new Vector2(720f, 120f), TextAlignmentOptions.Center);

        PositionIntroOverlay();

        m_IntroRoot = root;
    }

    private void CreateResultsOverlay()
    {
        if (m_ResultsRoot != null)
        {
            return;
        }

        var root = new GameObject("MainLevelResultsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        DontDestroyOnLoad(root);

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 5000;

        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(1400f, 860f);

        m_ResultsGroup = root.GetComponent<CanvasGroup>();
        m_ResultsGroup.alpha = 0f;
        m_ResultsGroup.interactable = false;
        m_ResultsGroup.blocksRaycasts = false;
        m_ResultsRect = rect;

        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);

        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var background = panel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.98f);

        var header = new GameObject("Header");
        header.transform.SetParent(panel.transform, false);

        var headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 1f);
        headerRect.anchorMax = new Vector2(0.5f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.sizeDelta = new Vector2(1120f, 112f);
        headerRect.anchoredPosition = new Vector2(0f, -56f);

        var headerBg = header.AddComponent<Image>();
        headerBg.color = new Color(1f, 1f, 1f, 0.03f);

        var outline = header.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.12f);
        outline.effectDistance = new Vector2(1f, -1f);

        m_ResultsTitleText = CreateText(header.transform, "NIVEL COMPLETADO", 32f, Color.white, new Vector2(0f, -32f), new Vector2(920f, 40f), TextAlignmentOptions.Center);
        if (m_ResultsTitleText != null)
        {
            m_ResultsTitleText.characterSpacing = 4f;
        }

        CreateText(header.transform, "// ESTATÍSTICAS //", 11f, new Color(1f, 1f, 1f, 0.35f), new Vector2(0f, -80f), new Vector2(620f, 20f), TextAlignmentOptions.Center);

        var body = new GameObject("Body");
        body.transform.SetParent(panel.transform, false);

        var bodyRect = body.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.sizeDelta = new Vector2(1240f, 720f);
        bodyRect.anchoredPosition = Vector2.zero;

        var bodyBg = body.AddComponent<Image>();
        bodyBg.color = new Color(1f, 1f, 1f, 0.025f);

        var bodyOutline = body.AddComponent<Outline>();
        bodyOutline.effectColor = new Color(1f, 1f, 1f, 0.10f);
        bodyOutline.effectDistance = new Vector2(1f, -1f);

        m_ResultsBodyText = CreateText(body.transform, string.Empty, 17f, new Color(1f, 1f, 1f, 0.86f), new Vector2(0f, 0f), new Vector2(1080f, 640f), TextAlignmentOptions.TopLeft);
        if (m_ResultsBodyText != null)
        {
            m_ResultsBodyText.lineSpacing = 10f;
            m_ResultsBodyText.textWrappingMode = TextWrappingModes.Normal;
        }

        PositionResultsOverlay();

        m_ResultsRoot = root;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, float size, Color color, Vector2 position, Vector2 dimensions, TextAlignmentOptions alignment)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;

        if (m_FontAsset != null)
        {
            tmp.font = m_FontAsset;
        }

        return tmp;
    }

    private void PositionIntroOverlay()
    {
        if (m_IntroRoot == null)
        {
            return;
        }

        if (m_IntroCamera == null)
        {
            m_IntroCamera = Camera.main;
        }

        if (m_IntroCamera == null)
        {
            return;
        }

        Transform cam = m_IntroCamera.transform;
        Vector3 forward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        Vector3 targetPos = cam.position + forward * m_IntroDistanceFromPlayer + Vector3.up * m_IntroVerticalOffset;
        m_IntroRoot.transform.position = Vector3.Lerp(m_IntroRoot.transform.position, targetPos, Time.deltaTime * 3f);

        Quaternion targetRot = Quaternion.LookRotation(m_IntroRoot.transform.position - cam.position);
        m_IntroRoot.transform.rotation = Quaternion.Slerp(m_IntroRoot.transform.rotation, targetRot, Time.deltaTime * 3f);
        m_IntroRoot.transform.localScale = Vector3.one * 0.001f;

        var canvas = m_IntroRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = m_IntroCamera;
        }

        if (m_IntroRect != null)
        {
            m_IntroRect.sizeDelta = new Vector2(500f, 220f);
        }
    }

    private void PositionResultsOverlay()
    {
        if (m_ResultsRoot == null)
        {
            return;
        }

        if (m_IntroCamera == null)
        {
            m_IntroCamera = Camera.main;
        }

        if (m_IntroCamera == null)
        {
            return;
        }

        Transform cam = m_IntroCamera.transform;
        Vector3 forward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        Vector3 targetPos = cam.position + forward * m_IntroDistanceFromPlayer + Vector3.up * m_IntroVerticalOffset;
        m_ResultsRoot.transform.position = Vector3.Lerp(m_ResultsRoot.transform.position, targetPos, Time.deltaTime * 3f);

        Quaternion targetRot = Quaternion.LookRotation(m_ResultsRoot.transform.position - cam.position);
        m_ResultsRoot.transform.rotation = Quaternion.Slerp(m_ResultsRoot.transform.rotation, targetRot, Time.deltaTime * 3f);
        m_ResultsRoot.transform.localScale = Vector3.one * 0.001f;

        var canvas = m_ResultsRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = m_IntroCamera;
        }

        if (m_ResultsRect != null)
        {
            m_ResultsRect.sizeDelta = new Vector2(1400f, 860f);
        }
    }

    private void UpdateResultsOverlay()
    {
        if (m_ResultsBodyText == null)
        {
            return;
        }

        m_ResultsBodyText.text = BuildResultsText();
    }

    private string BuildResultsText()
    {
        var builder = new StringBuilder();

        builder.AppendLine("// ESTATÍSTICAS DE CPR //");

        if (m_RhythmValidator != null)
        {
            int total = m_RhythmValidator.TotalCompressions;
            int target = Mathf.Max(1, m_RhythmValidator.TargetCompressions);
            float accuracy = total > 0 ? (float)m_RhythmValidator.SuccessfulCompressions / total : 0f;

            builder.AppendLine($"Compressões   : {total} / {target}");
            builder.AppendLine($"Com Sucesso      : {m_RhythmValidator.SuccessfulCompressions}");
            builder.AppendLine($"Perfeitos         : {m_RhythmValidator.PerfectCompressions}");
            builder.AppendLine($"Falhas          : {m_RhythmValidator.MissCount}");
            builder.AppendLine($"Melhor sequência     : {m_RhythmValidator.BestStreak}");
            builder.AppendLine($"Precisão        : {Mathf.RoundToInt(accuracy * 100f)}%");
        }
        else
        {
            builder.AppendLine("CPR data unavailable.");
        }

        builder.AppendLine();
        builder.AppendLine("// PASSOS COMPLETADOS //");

        if (m_MainLevelMachine != null)
        {
            int resolvedCount = m_MainLevelMachine.StateResolutionHistory.Count;
            int successCount = 0;
            for (int i = 0; i < m_MainLevelMachine.StateResolutionHistory.Count; i++)
            {
                if (m_MainLevelMachine.StateResolutionHistory[i].Succeeded)
                {
                    successCount++;
                }
            }

            int failedCount = resolvedCount - successCount;
            int totalStates = m_MainLevelMachine.StatesToExecute != null ? m_MainLevelMachine.StatesToExecute.Count : 0;

            builder.AppendLine($"Completados        : {resolvedCount} / {totalStates}");
            builder.AppendLine($"Com sucesso       : {successCount}");
            builder.AppendLine($"Falhados          : {failedCount}");
        }
        else
        {
            builder.AppendLine("State data unavailable.");
        }

        builder.AppendLine();
        builder.AppendLine("// TEMPO DE SESSÃO //");
        builder.AppendLine(m_TutorialTimer != null ? m_TutorialTimer.GetLatestComparisonSummary() : "No session summary available.");

        return builder.ToString();
    }

    private IEnumerator FadeResultsIn()
    {
        if (m_ResultsGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        float start = m_ResultsGroup.alpha;

        while (elapsed < m_ResultsFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, m_ResultsFadeDuration));
            m_ResultsGroup.alpha = Mathf.Lerp(start, 1f, t);
            yield return null;
        }

        m_ResultsGroup.alpha = 1f;
    }

    private IEnumerator FadeIntroOut()
    {
        if (m_IntroGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        float start = m_IntroGroup.alpha;

        while (elapsed < m_IntroFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, m_IntroFadeDuration));
            m_IntroGroup.alpha = Mathf.Lerp(start, 0f, t);
            yield return null;
        }

        m_IntroGroup.alpha = 0f;
    }

    private void DestroyIntroOverlay()
    {
        if (m_IntroRoot != null)
        {
            Destroy(m_IntroRoot);
            m_IntroRoot = null;
        }

        m_IntroGroup = null;
        m_IntroRect = null;
    }

    private void OnDestroy()
    {
        DestroyIntroOverlay();
        DestroyResultsOverlay();
    }

    private void DestroyResultsOverlay()
    {
        if (m_ResultsRoot != null)
        {
            Destroy(m_ResultsRoot);
            m_ResultsRoot = null;
        }

        m_ResultsGroup = null;
        m_ResultsRect = null;
        m_ResultsTitleText = null;
        m_ResultsBodyText = null;
    }

    private StateMachine FindMainLevelStateMachine()
    {
        var machines = FindObjectsByType<StateMachine>(FindObjectsSortMode.None);
        if (machines == null || machines.Length == 0)
        {
            return null;
        }

        var targetScene = SceneManager.GetSceneByName(m_MainLevelScene);
        if (targetScene.IsValid())
        {
            for (int i = 0; i < machines.Length; i++)
            {
                if (machines[i] != null && machines[i].gameObject.scene == targetScene)
                {
                    return machines[i];
                }
            }
        }

        return machines[0];
    }

    private AppStartup FindMainLevelAppStartup()
    {
        var startups = FindObjectsByType<AppStartup>(FindObjectsSortMode.None);
        if (startups == null || startups.Length == 0)
        {
            return null;
        }

        var targetScene = SceneManager.GetSceneByName(m_MainLevelScene);
        if (targetScene.IsValid())
        {
            for (int i = 0; i < startups.Length; i++)
            {
                if (startups[i] != null && startups[i].gameObject.scene == targetScene)
                {
                    return startups[i];
                }
            }
        }

        return startups[0];
    }

    private bool HasExternalMainLevelRunner()
    {
        return FindAnyObjectByType<AppStartup>() != null || FindAnyObjectByType<TutorialManager>() != null;
    }
}
