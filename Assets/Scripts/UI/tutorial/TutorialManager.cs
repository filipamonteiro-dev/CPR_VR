using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;


public class TutorialManagerUI : MonoBehaviour
{
    // ── Evento de saída ────────────────────────────────────────────────
    [Header("Navegação")]
    public UnityEvent OnTrainingStart;   // equivalente a navigate("/training")

    // ── Header ─────────────────────────────────────────────────────────
    [Header("Header — Dots de progresso")]
    public Transform   dotsContainer;   // HorizontalLayoutGroup com os StepDot filhos
    public TextMeshProUGUI stepCounterText; // "1 / 6"

    // ── Painel esquerdo ────────────────────────────────────────────────
    [Header("Painel Esquerdo")]
    public TextMeshProUGUI stepNumberText;  // "01"
    public TextMeshProUGUI stepLabelText;   // "VERIFICAÇÃO DO LOCAL"

    // ── Painel direito ─────────────────────────────────────────────────
    [Header("Painel Direito")]
    public GameObject      compressionSpecPanel; // activo só quando showArrow
    public Image           progressBar;           // fillAmount animado

    // ── Painel de instrução (bottom) ───────────────────────────────────
    [Header("Painel de Instrução")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;
    public Button          prevButton;
    public Button          nextButton;
    public TextMeshProUGUI nextButtonLabel;  // "PRÓXIMO →" ou "INICIAR TREINO →"

    // ── Animação ────────────────────────────────────────────────────────
    [Header("Animação")]
    public float panelFadeDuration = 0.3f;

    // ── Estado interno ─────────────────────────────────────────────────
    private TutorialStep[] steps;
    private int            currentStep = 0;
    private StepDotUI[]    dots;

    // ── Ciclo de vida ──────────────────────────────────────────────────
    void Start()
    {
        steps = TutorialStepsCatalog.All;

        // Cria os dots dinamicamente se o container estiver vazio
        BuildDots();

        SetNavigationVisible(false);

        ApplyStep(currentStep, animate: false);
    }

    // ── Aplicar estado do passo ────────────────────────────────────────
    private void ApplyStep(int index, bool animate)
    {
        TutorialStep step = steps[index];

        // ── Dots ──────────────────────────────────────────────────────
        for (int i = 0; i < dots.Length; i++)
            dots[i].SetState(active: i == index, complete: i < index);

        // ── Header counter ────────────────────────────────────────────
        stepCounterText.text = $"{index + 1} / {steps.Length}";

        // ── Painel esquerdo ────────────────────────────────────────────
        stepNumberText.text = $"0{step.id}";
        stepLabelText.text  = step.label;

        // ── Painel direito ─────────────────────────────────────────────
        compressionSpecPanel.SetActive(step.showArrow);

        float targetFill = (float)(index + 1) / steps.Length;
        if (animate) StartCoroutine(AnimateFill(targetFill));
        else         progressBar.fillAmount = targetFill;

        // ── Instrução (com fade) ───────────────────────────────────────
        if (animate) StartCoroutine(FadeInstruction(step));
        else         SetInstructionImmediate(step);

    }

    // ── Coroutines de animação ─────────────────────────────────────────
    private IEnumerator FadeInstruction(TutorialStep step)
    {
        // Fade out
        yield return StartCoroutine(FadeGroup(titleText, instructionText, 0f));
        SetInstructionImmediate(step);
        // Fade in
        yield return StartCoroutine(FadeGroup(titleText, instructionText, 1f));
    }

    private IEnumerator FadeGroup(TextMeshProUGUI a, TextMeshProUGUI b, float target)
    {
        float start   = a.alpha;
        float elapsed = 0f;
        while (elapsed < panelFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / panelFadeDuration);
            a.alpha = Mathf.Lerp(start, target, t);
            b.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        a.alpha = target;
        b.alpha = target;
    }

    private IEnumerator AnimateFill(float target)
    {
        float start   = progressBar.fillAmount;
        float elapsed = 0f;
        float dur     = 0.4f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            progressBar.fillAmount = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / dur));
            yield return null;
        }
        progressBar.fillAmount = target;
    }

    private void SetInstructionImmediate(TutorialStep step)
    {
        titleText.text       = step.title;
        instructionText.text = step.instruction;
    }

    // ── Helpers ────────────────────────────────────────────────────────
    private void BuildDots()
    {
        // Procura dots já existentes no container
        dots = dotsContainer.GetComponentsInChildren<StepDotUI>();

        // Se não existirem, avisa — devem ser criados no Editor
        if (dots.Length != steps.Length)
            Debug.LogWarning($"[TutorialManager] Esperados {steps.Length} StepDotUI no container, encontrados {dots.Length}. Cria-os no Editor.");
    }

    private void SetButtonAlpha(Button btn, float alpha)
    {
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.alpha = alpha;
    }

    private void SetNavigationVisible(bool isVisible)
    {
        if (prevButton != null)
        {
            prevButton.gameObject.SetActive(isVisible);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(isVisible);
        }

        if (nextButtonLabel != null)
        {
            nextButtonLabel.gameObject.SetActive(isVisible);
        }
    }
}
