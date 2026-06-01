using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

[DisallowMultipleComponent]
public class CPRRhythmHudPresenter : MonoBehaviour
{
    [Header("Ligação")]
    [SerializeField] private CPRCompressionRhythmValidator rhythmValidator;
    [SerializeField] private Camera xrCamera;
    [SerializeField] private TMP_FontAsset customFont;

    [Header("Posição fixa")]
    [SerializeField] private Transform placementAnchor;

    [Header("Beat Marker")]
    [SerializeField] private float beatMarkerPadding = 20f;
    [SerializeField] private float beatHitZoneWidth = 100f;

    private const float PanelW = 500f;
    private const float PanelH = 220f;
    private const float BarW = 440f;
    private const float DemoHitCenter01 = 0.12f;
    private const float DemoHitWindow01 = 0.08f;

    private static readonly Color BgColor = new Color(0.02f, 0.03f, 0.06f, 1f);
    private static readonly Color HeaderBg = new Color(1f, 1f, 1f, 0.05f);
    private static readonly Color PanelBg = new Color(1f, 1f, 1f, 0.03f);
    private static readonly Color BorderMain = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color BorderDash = new Color(1f, 1f, 1f, 0.08f);
    private static readonly Color Accent = new Color(1f, 1f, 1f, 0.45f);
    private static readonly Color FillColor = new Color(1f, 1f, 1f, 0.38f);
    private static readonly Color TextDim = new Color(1f, 1f, 1f, 0.22f);
    private static readonly Color TextMed = new Color(1f, 1f, 1f, 0.48f);
    private static readonly Color TextBright = new Color(1f, 1f, 1f, 0.90f);
    private static readonly Color TextFaint = new Color(1f, 1f, 1f, 0.14f);
    private static readonly Color HitZoneColor = new Color(0.9f, 0.9f, 0.9f, 0.18f);
    private static readonly Color HitCenterColor = new Color(1f, 1f, 1f, 0.7f);
    private static readonly Color PerfectColor = new Color(0.24f, 0.68f, 0.28f);
    private static readonly Color GoodColor = new Color(0.95f, 0.74f, 0.2f);
    private static readonly Color MissColor = new Color(0.82f, 0.2f, 0.2f);

    private TextMeshProUGUI titleText;
    private TextMeshProUGUI compressionCountText;
    private TextMeshProUGUI bpmText;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI streakText;
    private TextMeshProUGUI feedbackText;
    private Image progressFill;
    private RectTransform beatTrack;
    private RectTransform beatMarker;
    private RectTransform beatHitZone;
    private RectTransform beatHitCenter;
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    private bool isBuilt;
    private bool isVisible;
    private float feedbackTimer;
    private bool feedbackWasPerfect;
    private float feedbackStrength;

    private bool demoMode;
    private float demoBpm = 110f;
    private int demoTarget = 30;
    private int demoStreak;
    private float demoStartTime;
    private float lastDemoPhase;

    private void Awake()
    {
        EnsureInitialized();
        SetVisible(false);
    }

    private void OnEnable()
    {
        BindStateMachine();
    }

    private void OnDisable()
    {
        UnbindStateMachine();
    }

    private void Start()
    {
        if (isVisible)
            RefreshContent();
    }

    private void Update()
    {
        if (!isVisible)
            return;

        if (demoMode)
            UpdateDemo();
        else if (rhythmValidator != null)
            RefreshFromValidator();
        else
            RefreshIdleState();

        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;

            if (feedbackTimer <= 0f && feedbackText != null)
                feedbackText.text = string.Empty;
        }
    }

    public void SetVisible(bool visible)
    {
        EnsureInitialized();

        if (visible && !gameObject.activeSelf)
            gameObject.SetActive(true);

        isVisible = visible;

        if (visible)
            ApplyPlacementTransform();

        if (canvasGroup != null)
        {
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            if (isActiveAndEnabled)
                FadeTo(visible ? 1f : 0f, visible ? 0.25f : 0.18f);
            else
                canvasGroup.alpha = visible ? 1f : 0f;
        }

        if (visible)
            RefreshContent();
    }

    public void ResetHud()
    {
        EnsureInitialized();

        feedbackTimer = 0f;
        feedbackStrength = 0f;
        feedbackWasPerfect = false;

        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
            feedbackText.color = TextMed;
        }

        RefreshContent();
    }

    public void ShowFeedback(float strength, bool wasPerfect)
    {
        feedbackTimer = 0.6f;
        feedbackStrength = Mathf.Clamp01(strength);
        feedbackWasPerfect = wasPerfect;

        if (feedbackText != null)
        {
            feedbackText.text = feedbackStrength <= 0f
                ? "FORA DO TEMPO"
                : wasPerfect ? "PERFEITO" : "BOM RITMO";

            feedbackText.color = feedbackStrength <= 0f
                ? MissColor
                : wasPerfect ? PerfectColor : GoodColor;
        }
    }

    public void StartDemo(float bpm = 110f, int targetCompressions = 30)
    {
        demoMode = true;
        demoBpm = Mathf.Max(1f, bpm);
        demoTarget = Mathf.Max(1, targetCompressions);
        demoStreak = 0;
        demoStartTime = Time.time;
        lastDemoPhase = 0f;
        SetVisible(true);
        ResetHud();
    }

    public void StopDemo()
    {
        demoMode = false;
        SetVisible(false);
    }

    [ContextMenu("Start Demo")]
    private void StartDemoContext()
    {
        StartDemo();
    }

    [ContextMenu("Stop Demo")]
    private void StopDemoContext()
    {
        StopDemo();
    }

    private void BindStateMachine()
    {
        if (rhythmValidator == null)
            return;

        rhythmValidator.SequenceCompleted += OnSequenceCompleted;
        rhythmValidator.RhythmFeedback += OnRhythmFeedback;
    }

    private void UnbindStateMachine()
    {
        if (rhythmValidator == null)
            return;

        rhythmValidator.SequenceCompleted -= OnSequenceCompleted;
        rhythmValidator.RhythmFeedback -= OnRhythmFeedback;
    }

    private void RefreshContent()
    {
        EnsureInitialized();

        if (!isBuilt)
            return;

        RefreshStaticText();

        if (demoMode)
            UpdateDemo();
        else if (rhythmValidator != null)
            RefreshFromValidator();
        else
            RefreshIdleState();
    }

    private void UpdateDemo()
    {
        float beatInterval = 60f / Mathf.Max(1f, demoBpm);
        float phase = Mathf.Repeat((Time.time - demoStartTime) / beatInterval, 1f);

        if (compressionCountText != null)
            compressionCountText.text = $"{demoStreak}/{demoTarget}";

        if (bpmText != null)
            bpmText.text = $"{Mathf.RoundToInt(demoBpm)} BPM";

        if (statusText != null)
            statusText.text = demoStreak >= demoTarget ? "SEQUENCIA CONCLUIDA" : "DEMO: MANTENHA O RITMO";

        if (streakText != null)
            streakText.text = $"STREAK {demoStreak}";

        if (progressFill != null)
            progressFill.fillAmount = (float)demoStreak / Mathf.Max(1, demoTarget);

        UpdateBeatVisuals(phase);

        if (IsPhaseInsideWindow(phase, DemoHitCenter01, DemoHitWindow01) &&
            !IsPhaseInsideWindow(lastDemoPhase, DemoHitCenter01, DemoHitWindow01))
        {
            demoStreak = Mathf.Min(demoTarget, demoStreak + 1);
            ShowFeedback(Random.Range(0.5f, 1f), Random.value > 0.3f);
        }

        lastDemoPhase = phase;
    }

    private void RefreshStaticText()
    {
        if (titleText != null)
            titleText.text = "COMPRESSOES";
    }

    private void RefreshFromValidator()
    {
        if (rhythmValidator == null)
            return;

        if (compressionCountText != null)
            compressionCountText.text = $"{rhythmValidator.TotalCompressions}/{Mathf.Max(1, rhythmValidator.TargetCompressions)}";

        if (bpmText != null)
            bpmText.text = rhythmValidator.IsRunning ? $"{Mathf.RoundToInt(rhythmValidator.TargetBpm)} BPM" : "AGUARDANDO";

        if (statusText != null)
            statusText.text = rhythmValidator.IsComplete
                ? "SEQUENCIA CONCLUIDA"
                : rhythmValidator.IsRunning ? "MANTENHA O RITMO" : "POSICIONE AS MAOS";

        if (streakText != null)
            streakText.text = $"STREAK {rhythmValidator.CurrentStreak}";

        if (progressFill != null)
            progressFill.fillAmount = rhythmValidator.Progress;

        UpdateBeatVisuals(rhythmValidator.BeatPhase01);
    }

    private void RefreshIdleState()
    {
        if (compressionCountText != null)
            compressionCountText.text = $"0/{Mathf.Max(1, demoTarget)}";

        if (bpmText != null)
            bpmText.text = "AGUARDANDO";

        if (statusText != null)
            statusText.text = "POSICIONE AS MAOS";

        if (streakText != null)
            streakText.text = "STREAK 0";

        if (progressFill != null)
            progressFill.fillAmount = 0f;

        UpdateBeatVisuals(0f);
    }

    private void UpdateBeatVisuals(float phase01)
    {
        if (beatTrack != null && beatMarker != null)
        {
            float trackWidth = beatTrack.rect.width;
            float leftEdge = -trackWidth * 0.5f + beatMarkerPadding;
            float rightEdge = trackWidth * 0.5f - beatMarkerPadding;
            float markerX = Mathf.Lerp(leftEdge, rightEdge, phase01);
            beatMarker.anchoredPosition = new Vector2(markerX, beatMarker.anchoredPosition.y);

            UpdateHitZone(trackWidth);
        }
    }

    private void UpdateHitZone(float trackWidth)
    {
        if (beatHitZone == null || trackWidth <= 0f)
            return;

        float zoneWidth = Mathf.Clamp(beatHitZoneWidth, 4f, trackWidth);
        beatHitZone.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, zoneWidth);
        beatHitZone.anchoredPosition = new Vector2(0f, beatHitZone.anchoredPosition.y);
    }

    private bool IsPhaseInsideWindow(float phase, float hitCenter, float halfWindow)
    {
        float delta = Mathf.Abs(Mathf.Repeat(phase - hitCenter + 0.5f, 1f) - 0.5f);
        return delta <= Mathf.Max(0.001f, halfWindow);
    }

    private void OnSequenceCompleted(CPRCompressionRhythmValidator validator)
    {
        if (statusText != null)
            statusText.text = "SEQUENCIA CONCLUIDA";
    }

    private void OnRhythmFeedback(CPRCompressionRhythmValidator validator, float strength, bool wasPerfect)
    {
        ShowFeedback(strength, wasPerfect);
    }

    private void BuildHUD()
    {
        if (isBuilt)
            return;

        var canvas = GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = xrCamera != null ? xrCamera : Camera.main;

        GetOrAddComponent<CanvasScaler>();
        GetOrAddComponent<GraphicRaycaster>();
        GetOrAddComponent<TrackedDeviceGraphicRaycaster>();

        if (canvasGroup == null)
            canvasGroup = GetOrAddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(PanelW, PanelH);
        transform.localScale = Vector3.one * 0.001f;

        MakeImage(gameObject, BgColor, Vector2.zero, new Vector2(PanelW, PanelH));
        BuildGrid(gameObject);

        float hw = PanelW / 2f + 4f;
        float hh = PanelH / 2f + 4f;
        AddCorner(gameObject, new Vector2(-hw, hh), new Vector2(14f, 2f), new Vector2(7f, -1f));
        AddCorner(gameObject, new Vector2(-hw, hh), new Vector2(2f, 14f), new Vector2(1f, -7f));
        AddCorner(gameObject, new Vector2(hw, hh), new Vector2(14f, 2f), new Vector2(-7f, -1f));
        AddCorner(gameObject, new Vector2(hw, hh), new Vector2(2f, 14f), new Vector2(-1f, -7f));
        AddCorner(gameObject, new Vector2(-hw, -hh), new Vector2(14f, 2f), new Vector2(7f, 1f));
        AddCorner(gameObject, new Vector2(-hw, -hh), new Vector2(2f, 14f), new Vector2(1f, 7f));
        AddCorner(gameObject, new Vector2(hw, -hh), new Vector2(14f, 2f), new Vector2(-7f, 1f));
        AddCorner(gameObject, new Vector2(hw, -hh), new Vector2(2f, 14f), new Vector2(-1f, 7f));

        MakeImage(gameObject, HeaderBg, new Vector2(0f, 83f), new Vector2(PanelW, 54f));
        MakeImage(gameObject, BorderMain, new Vector2(0f, 56f), new Vector2(PanelW, 1f));
        MakeImage(gameObject, BorderDash, new Vector2(-36f, -8f), new Vector2(1f, 150f));

        titleText = MakeTMP(gameObject, "// RITMO CPR //",
            new Vector2(-138f, 84f), new Vector2(180f, 16f), 9f, TextDim,
            TextAlignmentOptions.Left, 5f);

        compressionCountText = MakeTMP(gameObject, "— / —",
            new Vector2(190f, 84f), new Vector2(90f, 16f), 10f, TextMed,
            TextAlignmentOptions.Right, 2f);

        MakeImage(gameObject, new Color(1f, 1f, 1f, 0.06f), new Vector2(0f, 66f), new Vector2(BarW, 3f));

        var fillGO = new GameObject("ProgressFill");
        fillGO.transform.SetParent(transform, false);
        var fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchorMin = fillRect.anchorMax = new Vector2(0.5f, 0.5f);
        fillRect.anchoredPosition = new Vector2(-BarW / 2f, 66f);
        fillRect.sizeDelta = new Vector2(0f, 3f);
        progressFill = fillGO.AddComponent<Image>();
        progressFill.color = FillColor;

        BuildInfoPanel(gameObject);
        BuildBeatPanel(gameObject);

        isBuilt = true;
    }

    private void BuildInfoPanel(GameObject root)
    {
        var panel = new GameObject("InfoPanel");
        panel.transform.SetParent(root.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(-132f, -24f);
        rt.sizeDelta = new Vector2(216f, 120f);

        MakeImage(panel, PanelBg, Vector2.zero, new Vector2(216f, 120f));
        MakeImage(panel, BorderDash, new Vector2(0f, 42f), new Vector2(186f, 1f));

        MakeTMP(panel, "RITMO ATUAL",
            new Vector2(-20f, 42f), new Vector2(164f, 14f), 8f, TextDim,
            TextAlignmentOptions.Left, 3f);

        bpmText = MakeTMP(panel, "AGUARDANDO",
            new Vector2(-10f, 14f), new Vector2(178f, 22f), 16f, TextBright,
            TextAlignmentOptions.Left, 1f);

        statusText = MakeTMP(panel, "POSICIONE AS MAOS",
            new Vector2(-8f, -16f), new Vector2(186f, 28f), 10f, TextMed,
            TextAlignmentOptions.Left, 1.5f);

        streakText = MakeTMP(panel, "STREAK 0",
            new Vector2(-20f, -42f), new Vector2(164f, 14f), 9f, TextDim,
            TextAlignmentOptions.Left, 2f);

        feedbackText = MakeTMP(panel, string.Empty,
            new Vector2(0f, -56f), new Vector2(182f, 18f), 10f, TextMed,
            TextAlignmentOptions.Center, 1f);
    }

    private void BuildBeatPanel(GameObject root)
    {
        var panel = new GameObject("BeatPanel");
        panel.transform.SetParent(root.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(125f, -24f);
        rt.sizeDelta = new Vector2(216f, 120f);

        MakeImage(panel, PanelBg, Vector2.zero, new Vector2(216f, 120f));
        MakeImage(panel, BorderDash, new Vector2(0f, 42f), new Vector2(186f, 1f));

        MakeTMP(panel, "SINCRONIA",
            new Vector2(-20f, 42f), new Vector2(140f, 14f), 8f, TextDim,
            TextAlignmentOptions.Left, 3f);

        var trackGO = new GameObject("BeatTrack");
        trackGO.transform.SetParent(panel.transform, false);
        beatTrack = trackGO.AddComponent<RectTransform>();
        beatTrack.anchoredPosition = new Vector2(0f, 4f);
        beatTrack.sizeDelta = new Vector2(164f, 56f);

        MakeImage(trackGO, new Color(1f, 1f, 1f, 0.04f), Vector2.zero, new Vector2(164f, 56f));
        MakeImage(trackGO, BorderMain, new Vector2(0f, -16f), new Vector2(138f, 1f));

        beatHitZone = new GameObject("HitZone").AddComponent<RectTransform>();
        beatHitZone.SetParent(trackGO.transform, false);
        beatHitZone.anchorMin = beatHitZone.anchorMax = new Vector2(0.5f, 0.5f);
        beatHitZone.pivot = new Vector2(0.5f, 0.5f);
        beatHitZone.sizeDelta = new Vector2(beatHitZoneWidth, 22f);
        beatHitZone.anchoredPosition = Vector2.zero;
        var hitZoneImage = beatHitZone.gameObject.AddComponent<Image>();
        hitZoneImage.color = HitZoneColor;

        beatHitCenter = new GameObject("HitCenter").AddComponent<RectTransform>();
        beatHitCenter.SetParent(trackGO.transform, false);
        beatHitCenter.anchorMin = beatHitCenter.anchorMax = new Vector2(0.5f, 0.5f);
        beatHitCenter.pivot = new Vector2(0.5f, 0.5f);
        beatHitCenter.sizeDelta = new Vector2(3f, 40f);
        beatHitCenter.anchoredPosition = Vector2.zero;
        var hitCenterImage = beatHitCenter.gameObject.AddComponent<Image>();
        hitCenterImage.color = HitCenterColor;

        beatMarker = new GameObject("Marker").AddComponent<RectTransform>();
        beatMarker.SetParent(trackGO.transform, false);
        beatMarker.anchorMin = beatMarker.anchorMax = new Vector2(0.5f, 0.5f);
        beatMarker.pivot = new Vector2(0.5f, 0.5f);
        beatMarker.sizeDelta = new Vector2(8f, 28f);
        beatMarker.anchoredPosition = new Vector2(-164f * 0.5f + beatMarkerPadding, 0f);
        var markerImage = beatMarker.gameObject.AddComponent<Image>();
        markerImage.color = Accent;

        MakeTMP(panel, "ALVO",
            new Vector2(0f, 22f), new Vector2(50f, 12f), 8f, TextFaint,
            TextAlignmentOptions.Center, 2f);

        MakeTMP(panel, "APERTE NO RITMO",
            new Vector2(0f, -33f), new Vector2(164f, 18f), 9f, TextFaint,
            TextAlignmentOptions.Center, 2f);
    }

    private void BuildGrid(GameObject root)
    {
        int cols = 8;
        int rows = 4;

        for (int c = 1; c < cols; c++)
        {
            MakeImage(root, new Color(1f, 1f, 1f, 0.02f),
                new Vector2(-PanelW / 2f + c * (PanelW / cols), 0f), new Vector2(1f, PanelH));
        }

        for (int r = 1; r < rows; r++)
        {
            MakeImage(root, new Color(1f, 1f, 1f, 0.02f),
                new Vector2(0f, -PanelH / 2f + r * (PanelH / rows)), new Vector2(PanelW, 1f));
        }
    }

    private void AddCorner(GameObject parent, Vector2 pos, Vector2 size, Vector2 offset)
    {
        MakeImage(parent, Accent, pos + offset, size);
    }

    private Image MakeImage(GameObject parent, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Img");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private TextMeshProUGUI MakeTMP(GameObject parent, string text, Vector2 pos, Vector2 size,
        float fontSize, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, float spacing = 0f)
    {
        var go = new GameObject("TMP");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.characterSpacing = spacing;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;

        if (customFont != null)
            tmp.font = customFont;

        return tmp;
    }

    private void ApplyPlacementTransform()
    {
        if (placementAnchor == null)
            return;

        transform.SetPositionAndRotation(placementAnchor.position, placementAnchor.rotation);

        var canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.worldCamera = xrCamera;
    }

    private void FadeTo(float target, float duration)
    {
        if (canvasGroup == null)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(DoFade(target, duration));
    }

    private IEnumerator DoFade(float target, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(time / duration));
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();

        return component;
    }

    private void EnsureInitialized()
    {
        if (isBuilt)
            return;

        BuildHUD();
        ApplyPlacementTransform();
        RefreshStaticText();
    }
}