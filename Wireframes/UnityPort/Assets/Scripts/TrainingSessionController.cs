using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VrCpr
{
    public class TrainingSessionController : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private TrainingMode currentMode = TrainingMode.Guided;
        [SerializeField] private bool simulateSession = true;

        [Header("HUD")]
        [SerializeField] private TMP_Text modeBadgeText;
        [SerializeField] private TMP_Text elapsedText;
        [SerializeField] private TMP_Text compressionCountText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text bpmText;
        [SerializeField] private TMP_Text bpmStatusText;
        [SerializeField] private TMP_Text depthText;

        [Header("Telemetry")]
        [SerializeField] private float bpm = 102f;
        [SerializeField] private float depth;
        [SerializeField] private int compressions;
        [SerializeField] private int score;

        private readonly List<string> feedbackMessages = new List<string>
        {
            "BOA PROFUNDIDADE - MANTENHA O RITMO",
            "TAXA: IDEAL",
            "COMPRESSAO EFICAZ",
            "MANTENHA A POSICAO DAS MAOS",
            "RITMO CONSISTENTE"
        };

        private float elapsedSeconds;
        private float compressionTimer;
        private float phase;
        private bool paused;

        public TrainingMode CurrentMode => currentMode;

        private void Start()
        {
            RefreshHud();
        }

        private void Update()
        {
            if (paused)
            {
                return;
            }

            elapsedSeconds += Time.deltaTime;
            compressionTimer += Time.deltaTime;

            if (simulateSession)
            {
                phase = (phase + Time.deltaTime * 7f) % (Mathf.PI * 2f);
                depth = Mathf.Max(0f, Mathf.Sin(phase) * 5.2f);

                if (Random.value < Time.deltaTime * 0.85f)
                {
                    bpm = Mathf.Round(Random.Range(95f, 116f));
                }

                if (compressionTimer >= 0.58f)
                {
                    compressionTimer = 0f;
                    compressions += 1;
                    score += Random.Range(5, 20);
                    SetRandomFeedback();
                }
            }

            RefreshHud();
        }

        public void SetMode(TrainingMode mode)
        {
            currentMode = mode;
            RefreshHud();
        }

        public void SetPaused(bool value)
        {
            paused = value;
        }

        public void ResetSession()
        {
            elapsedSeconds = 0f;
            compressionTimer = 0f;
            phase = 0f;
            bpm = 102f;
            depth = 0f;
            compressions = 0;
            score = 0;
            paused = false;

            if (feedbackText != null)
            {
                feedbackText.text = "INICIAR COMPRESSOES";
            }

            RefreshHud();
        }

        public SessionSnapshot GetSnapshot()
        {
            return new SessionSnapshot
            {
                elapsedSeconds = Mathf.FloorToInt(elapsedSeconds),
                compressions = compressions,
                score = score,
                bpm = bpm,
                depth = depth,
                feedback = feedbackText != null ? feedbackText.text : string.Empty
            };
        }

        private void SetRandomFeedback()
        {
            if (feedbackText == null || feedbackMessages.Count == 0)
            {
                return;
            }

            feedbackText.text = feedbackMessages[Random.Range(0, feedbackMessages.Count)];
        }

        private void RefreshHud()
        {
            if (modeBadgeText != null)
            {
                modeBadgeText.text = currentMode == TrainingMode.Test ? "// MODO TESTE //" : "// MODO TREINO //";
            }

            if (elapsedText != null)
            {
                elapsedText.text = FormatTime(Mathf.FloorToInt(elapsedSeconds));
            }

            if (compressionCountText != null)
            {
                compressionCountText.text = compressions.ToString();
            }

            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }

            if (bpmText != null)
            {
                bpmText.text = Mathf.RoundToInt(bpm).ToString();
            }

            if (bpmStatusText != null)
            {
                bpmStatusText.text = bpm < 90f ? "MUITO LENTO" : bpm > 115f ? "MUITO RAPIDO" : "IDEAL";
            }

            if (depthText != null)
            {
                depthText.text = depth.ToString("0.0");
            }
        }

        private static string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            return $"{minutes:00}:{remainingSeconds:00}";
        }
    }
}