using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TrainingManager : MonoBehaviour
{
    // ── Modo ─────────────────────────────────────────────────────────────
    [Header("Modo")]
    public bool isTestMode = false;

    // ── Detectors CPR reais (opcional — se null usa simulação) ────────────
    [Header("Detectors CPR")]
    public CPRCompressionPulseDetector   compressionDetector;
    public CPRCompressionRhythmValidator rhythmValidator;
    public CPRHandPlacementDetector      handPlacementDetector;

    // ── Referências UI ────────────────────────────────────────────────────
    [Header("HUD — Sessão (top-right)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI compressionsText;
    public TextMeshProUGUI scoreText;
    public GameObject scorePanel;
    public GameObject depthOkPanel;
    public TextMeshProUGUI depthOkText;
    public TextMeshProUGUI modeBadgeText;

    [Header("HUD — Ritmo (top-left)")]
    public TextMeshProUGUI bpmText;
    public TextMeshProUGUI bpmStatusText;
    public ECGDisplay ecgDisplay;
    [Tooltip("Ponto circular que pulsa ao ritmo do BPM")]
    public Image bpmPulseDot;

    [Header("HUD — Compressão (bottom-right)")]
    public CompressionGauge compressionGauge;

    [Header("HUD — Retorno (bottom-left)")]
    public TextMeshProUGUI feedbackText;
    public Image depthProgressBar;
    public Image depthProgressBarBg;

    [Header("Pausa / Fim")]
    public GameObject pauseButton;
    public PauseMenuUI pauseMenu;
    public EndSessionScreen endScreen;

    // ── Estado interno ────────────────────────────────────────────────────
    private bool   paused        = false;
    private bool   sessionEnded  = false;
    private int    elapsed       = 0;
    private int    bpm           = 102;
    private float  depth         = 0f;
    private int    compressions  = 0;
    private string feedback      = "INICIAR COMPRESSÕES";
    private int    score         = 0;

    // Medição de BPM real
    private float lastCompressionTime = -1f;
    private float measuredBpm         = 0f;
    private float bpmAccum            = 0f;     // acumulador para decaimento
    private float totalBpmSum         = 0f;
    private int   bpmSamples          = 0;

    // Modos de execução
    private bool usingRealData = false;

    // Timers de simulação (usados apenas quando usingRealData == false)
    private float timerAccum        = 0f;
    private float compressAccum     = 0f;
    private float compressionAccum  = 0f;
    private float compressPhase     = 0f;
    private float bpmPulseTimer     = 0f;

    private static readonly string[] feedbackMessages = {
        "BOA PROFUNDIDADE — MANTENHA O RITMO",
        "TAXA: IDEAL",
        "COMPRESSÃO EFICAZ",
        "MANTENHA A POSIÇÃO DAS MÃOS",
        "RITMO CONSISTENTE",
    };

    // ── Ciclo de vida ─────────────────────────────────────────────────────
    void Start()
    {
        modeBadgeText.text = isTestMode ? "// MODO TESTE //" : "// MODO TREINO //";

        if (scorePanel   != null) scorePanel.SetActive(isTestMode);
        if (depthOkPanel != null) depthOkPanel.SetActive(!isTestMode);

        // Pausa
        if (pauseMenu != null)
        {
            pauseMenu.onResume.AddListener(ResumeSession);
            pauseMenu.onRestart.AddListener(RestartSession);
            pauseMenu.onQuitToMenu.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));
        }

        // End screen
        if (endScreen != null)
            endScreen.onRestart.AddListener(RestartSession);

        // Detectors reais
        usingRealData = compressionDetector != null;

        if (compressionDetector != null)
            compressionDetector.CompressionPressed += OnCompressionPressed;

        if (rhythmValidator != null)
        {
            rhythmValidator.RhythmFeedback    += OnRhythmFeedback;
            rhythmValidator.SequenceCompleted += OnSequenceCompleted;
        }

        if (handPlacementDetector != null)
        {
            handPlacementDetector.AlignmentLocked += OnHandsAligned;
            handPlacementDetector.AlignmentLost   += OnHandsLost;
        }

        RefreshUI();
    }

    void OnDestroy()
    {
        if (compressionDetector != null)
            compressionDetector.CompressionPressed -= OnCompressionPressed;

        if (rhythmValidator != null)
        {
            rhythmValidator.RhythmFeedback    -= OnRhythmFeedback;
            rhythmValidator.SequenceCompleted -= OnSequenceCompleted;
        }

        if (handPlacementDetector != null)
        {
            handPlacementDetector.AlignmentLocked -= OnHandsAligned;
            handPlacementDetector.AlignmentLost   -= OnHandsLost;
        }
    }

    // ── Pausa ─────────────────────────────────────────────────────────────
    public void PauseSession()
    {
        if (paused || sessionEnded) return;
        paused = true;
        float acc = bpmSamples > 0 ? Mathf.Round(totalBpmSum / bpmSamples) : 0f;
        pauseMenu?.Show(compressions, elapsed, score > 0 ? Mathf.Clamp01(score / Mathf.Max(1f, elapsed * 15f)) : 0f);
    }

    public void ResumeSession()
    {
        paused = false;
        pauseMenu?.Hide();
    }

    public void RestartSession()
    {
        paused           = false;
        sessionEnded     = false;
        elapsed          = 0;
        compressions     = 0;
        score            = 0;
        feedback         = "INICIAR COMPRESSÕES";
        measuredBpm      = 0f;
        lastCompressionTime = -1f;
        bpmAccum         = 0f;
        totalBpmSum      = 0f;
        bpmSamples       = 0;
        timerAccum       = 0f;
        compressAccum    = 0f;
        compressionAccum = 0f;
        bpmPulseTimer    = 0f;
        pauseMenu?.Hide();
        endScreen?.Hide();
        RefreshUI();
    }

    // ── Update ────────────────────────────────────────────────────────────
    void Update()
    {
        if (paused || sessionEnded) return;

        float dt = Time.deltaTime;

        // Timer de sessão (sempre real)
        timerAccum += dt;
        if (timerAccum >= 1f)
        {
            timerAccum -= 1f;
            elapsed++;
            timerText.text = FormatTime(elapsed);
        }

        if (usingRealData)
            UpdateRealData(dt);
        else
            UpdateSimulation(dt);

        // Pulsação do ponto de BPM (comum a ambos os modos)
        if (bpmPulseDot != null)
        {
            bpmPulseTimer += dt;
            float beatInterval = 60f / Mathf.Max(1f, bpm);
            float phase  = (bpmPulseTimer % beatInterval) / beatInterval;
            float pulse  = Mathf.Sin(phase * Mathf.PI);
            bpmPulseDot.transform.localScale = Vector3.one * (1f + pulse * 0.6f);
            bpmPulseDot.color = new Color(1f, 1f, 1f, 0.4f + pulse * 0.55f);
        }
    }

    // ── Dados reais (detectors) ───────────────────────────────────────────

    private void UpdateRealData(float dt)
    {
        // Profundidade em tempo real (metros → cm)
        if (compressionDetector != null)
        {
            depth = compressionDetector.GetCompressionDepth() * 100f;
            compressionGauge?.SetDepth(depth);
            UpdateDepthProgressBar();
        }

        // Decaimento do BPM medido se não houver compressões há mais de 2 s
        if (measuredBpm > 0f)
        {
            bpmAccum += dt;
            if (bpmAccum > 2f)
            {
                measuredBpm = 0f;
                bpm = 0;
                UpdateBpmUI();
            }
        }
    }

    private void OnCompressionPressed(CPRCompressionPulseDetector det)
    {
        compressions++;
        bpmAccum = 0f;

        float now = Time.time;
        if (lastCompressionTime > 0f)
        {
            float interval = now - lastCompressionTime;
            if (interval > 0.2f && interval < 2.5f)
            {
                measuredBpm  = 60f / interval;
                bpm          = Mathf.RoundToInt(measuredBpm);
                totalBpmSum += measuredBpm;
                bpmSamples++;
            }
        }
        lastCompressionTime = now;

        if (isTestMode)
            score += 10;

        RefreshUI();
    }

    private void OnRhythmFeedback(CPRCompressionRhythmValidator val, float strength, bool wasPerfect)
    {
        if (wasPerfect)
            feedback = "RITMO PERFEITO";
        else if (strength > 0.75f)
            feedback = "BOA PROFUNDIDADE — MANTENHA O RITMO";
        else if (strength > 0.45f)
            feedback = "COMPRESSÃO EFICAZ";
        else if (strength > 0f)
            feedback = "COMPRIMA MAIS FUNDO";
        else
            feedback = "FORA DO RITMO — AJUSTE O TEMPO";

        if (isTestMode && wasPerfect)
            score += 5;

        RefreshUI();
    }

    private void OnSequenceCompleted(CPRCompressionRhythmValidator val)
    {
        EndSession();
    }

    private void OnHandsAligned(CPRHandPlacementDetector det)
    {
        feedback = "POSIÇÃO CORRETA — INICIE COMPRESSÕES";
        RefreshUI();
    }

    private void OnHandsLost(CPRHandPlacementDetector det)
    {
        feedback = "REPOSICIONE AS MÃOS NO CENTRO DO PEITO";
        RefreshUI();
    }

    // ── Simulação (fallback quando detectors == null) ─────────────────────

    private void UpdateSimulation(float dt)
    {
        compressAccum += dt;
        if (compressAccum >= 0.06f)
        {
            compressAccum -= 0.06f;
            compressPhase  = (compressPhase + 0.08f) % (Mathf.PI * 2f);
            depth          = Mathf.Max(0f, Mathf.Sin(compressPhase) * 5.2f);

            if (Random.value < 0.05f)
                bpm = Mathf.FloorToInt(95 + Random.value * 15f);

            compressionGauge?.SetDepth(depth);
            UpdateDepthProgressBar();
            UpdateBpmUI();
        }

        compressionAccum += dt;
        if (compressionAccum >= 0.58f)
        {
            compressionAccum -= 0.58f;
            compressions++;
            score   += Mathf.FloorToInt(Random.value * 15f + 5f);
            feedback = feedbackMessages[Mathf.FloorToInt(Random.value * feedbackMessages.Length)];
            RefreshUI();
        }
    }

    // ── Fim de sessão ─────────────────────────────────────────────────────

    public void EndSession()
    {
        if (sessionEnded) return;
        sessionEnded = true;

        float avgBpm   = bpmSamples > 0 ? totalBpmSum / bpmSamples : bpm;
        float accuracy = isTestMode
            ? Mathf.Clamp01(score / Mathf.Max(1f, elapsed * 15f))
            : (compressions > 0 ? Mathf.Clamp01((float)compressions / Mathf.Max(1, elapsed * 2)) : 0f);

        endScreen?.Show(compressions, elapsed, accuracy, avgBpm, isTestMode);
    }

    // ── Helpers de UI ─────────────────────────────────────────────────────

    private string FormatTime(int seconds) => $"{seconds/60:D2}:{seconds%60:D2}";

    private void UpdateBpmUI()
    {
        if (bpmText == null) return;
        bpmText.text = bpm > 0 ? bpm.ToString() : "—";

        string status;
        bool   ideal;
        if      (bpm <= 0)   { status = "A AGUARDAR"; ideal = false; }
        else if (bpm < 90)   { status = "MUITO LENTO"; ideal = false; }
        else if (bpm > 115)  { status = "MUITO RÁPIDO"; ideal = false; }
        else                 { status = "IDEAL"; ideal = true; }

        if (bpmStatusText != null)
        {
            bpmStatusText.text  = status;
            Color c = ideal ? new Color(1f,1f,1f,0.75f) : new Color(1f,0.588f,0.392f,0.8f);
            bpmStatusText.color = c;
            bpmText.color       = c;
        }
    }

    private void UpdateDepthProgressBar()
    {
        if (depthProgressBar == null) return;
        depthProgressBar.fillAmount = depth / 6f;
        depthProgressBar.color      = depth >= 4f
            ? new Color(1f,1f,1f,0.6f)
            : new Color(1f,1f,1f,0.25f);
    }

    private void RefreshUI()
    {
        timerText.text        = FormatTime(elapsed);
        compressionsText.text = compressions.ToString();
        feedbackText.text     = feedback;

        if (isTestMode && scoreText != null)
            scoreText.text = score.ToString();

        if (!isTestMode && depthOkText != null)
            depthOkText.text = depth >= 4f ? "✓" : "–";

        UpdateBpmUI();
        UpdateDepthProgressBar();
        ecgDisplay?.SetBpm(bpm);
    }
}
