using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TrainingManager : MonoBehaviour
{
    // ── Modo ─────────────────────────────────────────────────────────────
    [Header("Modo")]
    public bool isTestMode = false;  // true = /test, false = /training

    // ── Referências UI ────────────────────────────────────────────────────
    [Header("HUD — Sessão (top-right)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI compressionsText;
    public TextMeshProUGUI scoreText;
    public GameObject scorePanel;      // activo só em isTestMode
    public GameObject depthOkPanel;    // activo só em !isTestMode
    public TextMeshProUGUI depthOkText;
    public TextMeshProUGUI modeBadgeText;

    [Header("HUD — Ritmo (top-left)")]
    public TextMeshProUGUI bpmText;
    public TextMeshProUGUI bpmStatusText;
    public ECGDisplay ecgDisplay;      // componente separado

    [Header("HUD — Compressão (bottom-right)")]
    public CompressionGauge compressionGauge;

    [Header("HUD — Retorno (bottom-left)")]
    public TextMeshProUGUI feedbackText;
    public Image depthProgressBar;     // largura animada 0–1 via fillAmount
    public Image depthProgressBarBg;

    public GameObject pauseButton;

    // ── Estado interno ────────────────────────────────────────────────────
    private bool   paused       = false;
    private int    elapsed      = 0;       // segundos
    private int    bpm          = 102;
    private float  depth        = 0f;
    private int    compressions = 0;
    private string feedback     = "INICIAR COMPRESSÕES";
    private int    score        = 0;
    private float  compressPhase = 0f;

    // Timers internos (equivalente aos setInterval do React)
    private float timerAccum      = 0f;
    private float compressAccum   = 0f;
    private float compressionAccum = 0f;

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

        if (scorePanel  != null) scorePanel.SetActive(isTestMode);
        if (depthOkPanel != null) depthOkPanel.SetActive(!isTestMode);

        RefreshUI();
    }

    void Update()
    {
        if (paused) return;

        float dt = Time.deltaTime;

        // ── Timer de sessão (1 segundo) ──────────────────────────────────
        timerAccum += dt;
        if (timerAccum >= 1f)
        {
            timerAccum -= 1f;
            elapsed++;
            timerText.text = FormatTime(elapsed);
        }

        // ── Animação de compressão (~60ms = 0.06s) ───────────────────────
        compressAccum += dt;
        if (compressAccum >= 0.06f)
        {
            compressAccum -= 0.06f;
            compressPhase = (compressPhase + 0.08f) % (Mathf.PI * 2f);
            depth = Mathf.Max(0f, Mathf.Sin(compressPhase) * 5.2f);

            // BPM flutua aleatoriamente ~5% das vezes
            if (Random.value < 0.05f)
                bpm = Mathf.FloorToInt(95 + Random.value * 15f);

            // Atualiza gauge e silhueta
            compressionGauge.SetDepth(depth);
            UpdateDepthProgressBar();
            UpdateBpmUI();

            float normalizedDepth = Mathf.Max(0f, Mathf.Sin(compressPhase)) * 0.85f;
        }

        // ── Compressões + feedback (580ms) ───────────────────────────────
        compressionAccum += dt;
        if (compressionAccum >= 0.58f)
        {
            compressionAccum -= 0.58f;
            compressions++;
            score += Mathf.FloorToInt(Random.value * 15f + 5f);
            feedback = feedbackMessages[Mathf.FloorToInt(Random.value * feedbackMessages.Length)];

            RefreshUI();
        }
    }


    // ── Helpers ────────────────────────────────────────────────────────────
    private string FormatTime(int seconds)
    {
        int m   = seconds / 60;
        int sec = seconds % 60;
        return $"{m:D2}:{sec:D2}";
    }

    private void UpdateBpmUI()
    {
        bpmText.text = bpm.ToString();

        string status;
        bool ideal;
        if (bpm < 90)       { status = "MUITO LENTO"; ideal = false; }
        else if (bpm > 115) { status = "MUITO RÁPIDO"; ideal = false; }
        else                { status = "IDEAL";        ideal = true;  }

        bpmStatusText.text  = status;
        Color c = ideal
            ? new Color(1f, 1f, 1f, 0.75f)
            : new Color(1f, 0.588f, 0.392f, 0.8f);
        bpmStatusText.color = c;
        bpmText.color       = c;
    }

    private void UpdateDepthProgressBar()
    {
        if (depthProgressBar == null) return;
        depthProgressBar.fillAmount = depth / 6f;
        depthProgressBar.color = depth >= 4f
            ? new Color(1f, 1f, 1f, 0.6f)
            : new Color(1f, 1f, 1f, 0.25f);
    }

    private void RefreshUI()
    {
        timerText.text       = FormatTime(elapsed);
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
