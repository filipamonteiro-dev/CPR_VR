using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CPRRhythmHudPresenter : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject canvasRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("References")]
    [SerializeField] private CPRCompressionRhythmValidator rhythmValidator;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text compressionCountText;
    [SerializeField] private TMP_Text bpmText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text streakText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Indicators")]
    [SerializeField] private Image progressFill;
    [SerializeField] private RectTransform beatTrack;
    [SerializeField] private RectTransform beatMarker;
    [SerializeField] private RectTransform beatHitZone;

    [Header("Colors")]
    [SerializeField] private Color perfectColor = new Color(0.24f, 0.68f, 0.28f);
    [SerializeField] private Color goodColor = new Color(0.95f, 0.74f, 0.2f);
    [SerializeField] private Color missColor = new Color(0.82f, 0.2f, 0.2f);

    [Header("Beat Marker")]
    [SerializeField] private float beatMarkerPadding = 20f;
    [SerializeField] private float beatHitZoneWidth = 100f;

    private bool isVisible;
    private float feedbackTimer;
    private bool feedbackWasPerfect;
    private float feedbackStrength;
    // Demo mode fields
    private bool demoMode;
    private float demoBpm = 110f;
    private int demoTarget = 30;
    private int demoStreak;
    private float demoStartTime;
    private float lastDemoPhase;

    private void Awake()
    {
        SetVisible(false);
        RefreshStaticText();
    }

    private void OnEnable()
    {
        if (rhythmValidator != null)
        {
            rhythmValidator.SequenceCompleted += OnSequenceCompleted;
            rhythmValidator.RhythmFeedback += OnRhythmFeedback;
        }
    }

    private void OnDisable()
    {
        if (rhythmValidator != null)
        {
            rhythmValidator.SequenceCompleted -= OnSequenceCompleted;
            rhythmValidator.RhythmFeedback -= OnRhythmFeedback;
        }
    }

    private void Update()
    {
        if (!isVisible)
            return;

        if (demoMode)
        {
            UpdateDemo();
        }
        else
        {
            if (rhythmValidator == null)
                return;

            RefreshFromValidator();
        }

        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;

            if (feedbackTimer <= 0f && feedbackText != null)
                feedbackText.text = string.Empty;
        }
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;

        if (canvasRoot != null)
            canvasRoot.SetActive(visible);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        if (visible)
            RefreshFromValidator();
    }

    public void ResetHud()
    {
        feedbackTimer = 0f;
        feedbackStrength = 0f;
        feedbackWasPerfect = false;

        if (feedbackText != null)
            feedbackText.text = string.Empty;

        RefreshFromValidator();
    }

    public void ShowFeedback(float strength, bool wasPerfect)
    {
        feedbackTimer = 0.6f;
        feedbackStrength = Mathf.Clamp01(strength);
        feedbackWasPerfect = wasPerfect;

        if (feedbackText != null)
            feedbackText.text = wasPerfect ? "PERFEITO" : feedbackStrength >= 0.5f ? "BOM RITMO" : "FORA DO TEMPO";

        if (feedbackText != null)
            feedbackText.color = wasPerfect ? perfectColor : feedbackStrength >= 0.5f ? goodColor : missColor;
    }

    // Demo API -------------------------------------------------
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

    private void UpdateDemo()
    {
        float beatInterval = 60f / Mathf.Max(1f, demoBpm);
        float phase = Mathf.Repeat((Time.time - demoStartTime) / beatInterval, 1f);

        // Update basic UI
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

        if (beatTrack != null && beatMarker != null)
        {
            float trackWidth = beatTrack.rect.width;
            float leftEdge = -trackWidth * 0.5f + beatMarkerPadding;
            float rightEdge = trackWidth * 0.5f - beatMarkerPadding;
            float markerX = Mathf.Lerp(leftEdge, rightEdge, phase);
            beatMarker.anchoredPosition = new Vector2(markerX, beatMarker.anchoredPosition.y);

            UpdateHitZone(trackWidth);
        }

        // On beat crossing (simple edge detect)
        if (IsInHitZone(phase) && !IsInHitZone(lastDemoPhase))
        {
            demoStreak = Mathf.Min(demoTarget, demoStreak + 1);
            // random feedback strength
            float strength = Random.Range(0.5f, 1f);
            bool perfect = Random.value > 0.3f;
            ShowFeedback(strength, perfect);
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
            compressionCountText.text = $"{rhythmValidator.CurrentStreak}/{Mathf.Max(1, rhythmValidator.TargetCompressions)}";

        if (bpmText != null)
            bpmText.text = rhythmValidator.IsRunning ? $"{Mathf.RoundToInt(rhythmValidator.TargetBpm)} BPM" : "AGUARDANDO";

        if (statusText != null)
            statusText.text = rhythmValidator.IsComplete ? "SEQUENCIA CONCLUIDA" : rhythmValidator.IsRunning ? "MANTENHA O RITMO" : "POSICIONE AS MAOS";

        if (streakText != null)
            streakText.text = $"STREAK {rhythmValidator.CurrentStreak}";

        if (progressFill != null)
            progressFill.fillAmount = rhythmValidator.Progress;

        if (beatTrack != null && beatMarker != null)
        {
            float trackWidth = beatTrack.rect.width;
            float leftEdge = -trackWidth * 0.5f + beatMarkerPadding;
            float rightEdge = trackWidth * 0.5f - beatMarkerPadding;
            float markerX = Mathf.Lerp(leftEdge, rightEdge, rhythmValidator.BeatPhase01);
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

    private bool IsInHitZone(float phase)
    {
        if (rhythmValidator == null)
            return false;

        float hitCenter = rhythmValidator.HitPhase01;
        float halfWindow = Mathf.Max(0.001f, rhythmValidator.HitWindow01 * 0.5f);
        float delta = Mathf.Abs(Mathf.Repeat(phase - hitCenter + 0.5f, 1f) - 0.5f);
        return delta <= halfWindow;
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
}