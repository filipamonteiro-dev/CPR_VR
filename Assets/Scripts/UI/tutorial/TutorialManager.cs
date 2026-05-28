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

    // ── Paciente ────────────────────────────────────────────────────────
    [Header("Paciente")]

    public GameObject        fullBodyGlow;    // AnimatePresence "full" highlight

    // ── Anotações ───────────────────────────────────────────────────────
    [Header("Anotações")]
    public TutorialAnnotationManager annotationManager;

    // ── Whiteboard ──────────────────────────────────────────────────────
    [Header("Whiteboard")]
    [Tooltip("Opcional — whiteboard físico na cena que mostra os passos e a instrução atual")]
    public TutorialWhiteboardUI whiteboard;

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
        steps = TutorialStepData.All;

        // Cria os dots dinamicamente se o container estiver vazio
        BuildDots();

        prevButton.onClick.AddListener(GoPrev);
        nextButton.onClick.AddListener(GoNext);

        ApplyStep(currentStep, animate: false);
    }

    // ── Navegação ──────────────────────────────────────────────────────
    public void GoNext()
    {
        if (currentStep < steps.Length - 1)
        {
            currentStep++;
            ApplyStep(currentStep, animate: true);
        }
        else
        {
            OnTrainingStart?.Invoke();
        }
    }

    public void GoPrev()
    {
        if (currentStep == 0) return;
        currentStep--;
        ApplyStep(currentStep, animate: true);
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

        // ── Botão anterior ─────────────────────────────────────────────
        prevButton.interactable = index > 0;
        SetButtonAlpha(prevButton, index > 0 ? 0.55f : 0.15f);

        // ── Label do botão seguinte ────────────────────────────────────
        nextButtonLabel.text = index == steps.Length - 1
            ? "INICIAR TREINO →"
            : "PRÓXIMO →";



        // Glow "full body"
        if (fullBodyGlow != null)
            fullBodyGlow.SetActive(step.highlight == HighlightMode.Full);

        // ── Anotações ──────────────────────────────────────────────────
        annotationManager?.ShowAnnotations(step.annotations);

        // ── Whiteboard ─────────────────────────────────────────────────
        whiteboard?.SetStep(index);
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
        dots = dotsContainer != null
            ? dotsContainer.GetComponentsInChildren<StepDotUI>()
            : new StepDotUI[0];

        if (dots.Length == steps.Length) return;

        // Criação dinâmica quando os dots não estão no Editor
        if (dotsContainer == null)
        {
            var go = new GameObject("DotsContainer");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2((float)steps.Length * 32f, 28f);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth  = false;
            hlg.childControlHeight = false;
            dotsContainer = go.transform;
        }
        else
        {
            foreach (Transform child in dotsContainer)
                Destroy(child.gameObject);
        }

        dots = new StepDotUI[steps.Length];
        for (int i = 0; i < steps.Length; i++)
            dots[i] = CreateDot(dotsContainer.gameObject, i);
    }

    private StepDotUI CreateDot(GameObject parent, int index)
    {
        var dot = new GameObject($"StepDot_{index + 1}");
        dot.transform.SetParent(parent.transform, false);
        var rt = dot.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(24f, 24f);

        var le = dot.AddComponent<LayoutElement>();
        le.preferredWidth  = 24f;
        le.preferredHeight = 24f;
        le.minWidth  = 24f;
        le.minHeight = 24f;

        var dotUI = dot.AddComponent<StepDotUI>();
        dotUI.dotRect      = rt;
        dotUI.sizeActive   = 24f;
        dotUI.sizeInactive = 24f;

        var borderGO  = CreateDotChild(dot, "Border");
        var borderImg = borderGO.AddComponent<Image>();
        borderImg.color = Color.clear;
        var outline = borderGO.AddComponent<Outline>();
        outline.effectColor    = new Color(1f, 1f, 1f, 0.20f);
        outline.effectDistance = new Vector2(1f, -1f);
        dotUI.borderImage = borderImg;

        var fillGO  = CreateDotChild(dot, "Fill");
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = Color.clear;
        dotUI.fillImage = fillImg;

        var numGO  = CreateDotChild(dot, "Number");
        var numTmp = numGO.AddComponent<TextMeshProUGUI>();
        numTmp.text      = (index + 1).ToString();
        numTmp.fontSize  = 10f;
        numTmp.alignment = TextAlignmentOptions.Center;
        numTmp.color     = new Color(1f, 1f, 1f, 0.25f);
        dotUI.numberText = numTmp;

        var chkGO  = CreateDotChild(dot, "Check");
        var chkTmp = chkGO.AddComponent<TextMeshProUGUI>();
        chkTmp.text      = "✓";
        chkTmp.fontSize  = 10f;
        chkTmp.alignment = TextAlignmentOptions.Center;
        chkTmp.color     = new Color(1f, 1f, 1f, 0.70f);
        chkGO.SetActive(false);
        dotUI.checkText = chkTmp;

        dotUI.SetIndex(index);
        dotUI.SetState(active: index == 0, complete: false);
        return dotUI;
    }

    private GameObject CreateDotChild(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    private void SetButtonAlpha(Button btn, float alpha)
    {
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.alpha = alpha;
    }
}
