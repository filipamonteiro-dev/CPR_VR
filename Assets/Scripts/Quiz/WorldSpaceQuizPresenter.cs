using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class WorldSpaceQuizPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject canvasRoot;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TMP_Text[] answerTexts;

    [Header("Style")]
    [Tooltip("Optional Font Asset to apply to texts.")]
    [SerializeField] private TMP_FontAsset uiFont;

    [Header("Feedback")]
    [SerializeField] private Color correctColor = new Color(0.55f, 1f, 0.60f, 0.85f);
    [SerializeField] private Color wrongColor   = new Color(1f, 0.39f, 0.39f, 0.85f);
    [SerializeField] private float autoHideDelaySeconds = 1.5f;

    // ── Paleta ──────────────────────────────────────────────────────────────
    private static readonly Color BgColor      = new Color(0.02f, 0.03f, 0.06f, 0.96f);
    private static readonly Color BorderNormal = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color BorderHover  = new Color(1f, 1f, 1f, 0.35f);
    private static readonly Color CornerAccent = new Color(1f, 1f, 1f, 0.35f);
    private static readonly Color TextDim      = new Color(1f, 1f, 1f, 0.25f);
    private static readonly Color TextMed      = new Color(1f, 1f, 1f, 0.60f);
    private static readonly Color TextBright   = new Color(1f, 1f, 1f, 0.90f);
    private static readonly Color BtnBg        = new Color(1f, 1f, 1f, 0.03f);

    private bool showFeedback    = true;
    private int  lastSelectedIndex = -1;
    private Coroutine autoHideRoutine;

    public event Action<int> AnswerSelected;

    private void Awake()
    {
        ApplyPanelStyling();
        ApplyTextStyling();
        BindButtons();
        ApplyStyleToAllButtons();
        NormalizeLocalZ();
    }

    // ── API pública ──────────────────────────────────────────────────────────

    public void SetVisible(bool visible)
    {
        if (canvasRoot != null) canvasRoot.SetActive(visible);
        else                    gameObject.SetActive(visible);
    }

    public void SetFeedbackVisibility(bool visible)
    {
        showFeedback = visible;
        if (feedbackText != null) feedbackText.gameObject.SetActive(visible);
    }

    public void ShowQuestion(CallQuestionData question, int questionNumber, int totalQuestions)
    {
        if (questionText != null)
            questionText.text = question != null ? question.Prompt : string.Empty;

        if (progressText != null)
            progressText.text = $"// {questionNumber} / {Mathf.Max(1, totalQuestions)} //";

        if (summaryText  != null) summaryText.text  = string.Empty;
        if (feedbackText != null) feedbackText.text = string.Empty;

        ConfigureAnswerButtons(question);
        ApplyStyleToAllButtons();
        lastSelectedIndex = -1;
    }

    public void ShowAnswerFeedback(bool isCorrect)
    {
        if (!showFeedback || feedbackText == null) return;

        feedbackText.text  = isCorrect ? "[ CORRETO ]" : "[ ERRADO ]";
        feedbackText.color = isCorrect ? correctColor  : wrongColor;

        if (lastSelectedIndex >= 0 && answerButtons != null && lastSelectedIndex < answerButtons.Length)
        {
            var btn = answerButtons[lastSelectedIndex];
            if (btn == null) return;

            Image bg = btn.GetComponent<Image>() ?? btn.targetGraphic as Image;
            if (bg != null)
                bg.color = isCorrect
                    ? new Color(correctColor.r, correctColor.g, correctColor.b, 0.12f)
                    : new Color(wrongColor.r,  wrongColor.g,  wrongColor.b,  0.12f);

            Color borderCol = isCorrect ? correctColor : wrongColor;
            SetButtonBorderColor(btn, borderCol);

            if (answerTexts != null && lastSelectedIndex < answerTexts.Length && answerTexts[lastSelectedIndex] != null)
                answerTexts[lastSelectedIndex].color = isCorrect ? correctColor : wrongColor;
        }

        BeginAutoHide();
    }

    public void ShowCompletion(QuizSession session)
    {
        SetButtonsInteractable(false);

        if (questionText != null)
        {
            questionText.text = "CHAMADA 112 CONCLUÍDA";
            questionText.characterSpacing = 3f;
        }

        if (progressText != null) progressText.text = "// FIM //";

        if (summaryText != null && session != null)
        {
            float pct = session.Accuracy * 100f;
            summaryText.text = $"CERTAS: {session.CorrectCount}   ERRADAS: {session.WrongCount}   PRECISÃO: {pct:0}%";
        }

        BeginAutoHide();
    }

    // ── Estilo do painel principal ────────────────────────────────────────────

    private void ApplyPanelStyling()
    {
        GameObject root = canvasRoot != null ? canvasRoot : gameObject;

        var img = root.GetComponent<Image>();
        if (img == null) img = root.AddComponent<Image>();
        img.color = BgColor;

        var outline = root.GetComponent<Outline>();
        if (outline == null) outline = root.AddComponent<Outline>();
        outline.effectColor    = BorderNormal;
        outline.effectDistance = new Vector2(1f, -1f);
    }

    private void ApplyTextStyling()
    {
        StyleTMP(questionText, TextBright, 26f, 0.8f, 18f, 30f);
        StyleTMP(progressText, TextDim,    14f, 3f,   10f, 16f);
        StyleTMP(feedbackText, TextMed,    18f, 1.5f, 14f, 20f);
        StyleTMP(summaryText,  TextMed,    14f, 1f,   10f, 16f);
    }

    private void StyleTMP(TMP_Text t, Color color, float fontSize, float charSpacing, float minSize, float maxSize)
    {
        if (t == null) return;
        t.color            = color;
        t.fontSize         = fontSize;
        t.characterSpacing = charSpacing;
        t.enableAutoSizing = true;
        t.fontSizeMin      = minSize;
        t.fontSizeMax      = maxSize;
        t.overflowMode     = TextOverflowModes.Ellipsis;
        if (uiFont != null) t.font = uiFont;
    }

    // ── Estilo dos botões ────────────────────────────────────────────────────

    private void ApplyStyleToAllButtons()
    {
        if (answerButtons == null) return;
        for (int i = 0; i < answerButtons.Length; i++)
            ApplyStyleToButton(i);
    }

    private void ApplyStyleToButton(int index)
    {
        if (answerButtons == null || index < 0 || index >= answerButtons.Length) return;

        var btn = answerButtons[index];
        if (btn == null) return;

        // Fundo
        Image bg = btn.GetComponent<Image>() ?? btn.targetGraphic as Image;
        if (bg != null) bg.color = BtnBg;

        // Hover colours
        var colors = btn.colors;
        colors.normalColor      = Color.clear;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.06f);
        colors.pressedColor     = new Color(1f, 1f, 1f, 0.12f);
        colors.selectedColor    = new Color(1f, 1f, 1f, 0.06f);
        colors.disabledColor    = new Color(1f, 1f, 1f, 0.02f);
        btn.colors = colors;
        if (bg != null) btn.targetGraphic = bg;

        // Borda exterior
        SetButtonBorderColor(btn, BorderNormal);

        // Cantos decorativos
        AddCornerAccents(btn.gameObject);

        // Texto
        TMP_Text txt = (answerTexts != null && index < answerTexts.Length) ? answerTexts[index] : null;
        if (txt != null)
        {
            txt.color            = TextMed;
            txt.characterSpacing = 0.6f;
            txt.enableAutoSizing = true;
            txt.fontSizeMin      = 14f;
            txt.fontSizeMax      = 20f;
            if (uiFont != null) txt.font = uiFont;
        }
    }

    private void ResetButtonVisual(int index)
    {
        if (answerButtons == null || index < 0 || index >= answerButtons.Length) return;

        var btn = answerButtons[index];
        Image bg = btn.GetComponent<Image>() ?? btn.targetGraphic as Image;
        if (bg != null) bg.color = BtnBg;

        SetButtonBorderColor(btn, BorderNormal);

        TMP_Text txt = (answerTexts != null && index < answerTexts.Length) ? answerTexts[index] : null;
        if (txt != null) txt.color = TextMed;
    }

    // ── Internos ─────────────────────────────────────────────────────────────

    private void ConfigureAnswerButtons(CallQuestionData question)
    {
        if (answerButtons == null) return;

        int optionCount = question != null && question.Options != null ? question.Options.Count : 0;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool active = i < optionCount;
            if (answerButtons[i] != null)
            {
                answerButtons[i].gameObject.SetActive(active);
                answerButtons[i].interactable = active;
                ResetButtonVisual(i);
            }

            if (answerTexts != null && i < answerTexts.Length && answerTexts[i] != null)
                answerTexts[i].text = active ? question.Options[i].ToUpper() : string.Empty;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (answerButtons == null) return;
        for (int i = 0; i < answerButtons.Length; i++)
            if (answerButtons[i] != null && answerButtons[i].gameObject.activeInHierarchy)
                answerButtons[i].interactable = interactable;
    }

    private void BindButtons()
    {
        if (answerButtons == null) return;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;
            int idx = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() =>
            {
                lastSelectedIndex = idx;
                AnswerSelected?.Invoke(idx);
            });
        }
    }

    // ── Helpers visuais ──────────────────────────────────────────────────────

    private void SetButtonBorderColor(Button btn, Color color)
    {
        var outline = btn.GetComponent<Outline>();
        if (outline == null) outline = btn.gameObject.AddComponent<Outline>();
        outline.effectColor    = color;
        outline.effectDistance = new Vector2(1f, -1f);
    }

    private void AddCornerAccents(GameObject btn)
    {
        const string tag = "CornerAccents";
        if (btn.transform.Find(tag) != null) return;   // já existem

        var root = new GameObject(tag);
        root.transform.SetParent(btn.transform, false);
        var rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        // 4 cantos × 2 barras = 8 imagens
        // anchorMin == anchorMax == corner anchor; pivot flips the bar direction
        (Vector2 anchor, Vector2 pivotH, Vector2 pivotV)[] corners =
        {
            (new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f)),   // top-left
            (new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f)),   // top-right
            (new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f)),   // bottom-left
            (new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f)),   // bottom-right
        };

        foreach (var c in corners)
        {
            MakeBar(root, c.anchor, c.pivotH, new Vector2(10f, 1.5f));   // horizontal
            MakeBar(root, c.anchor, c.pivotV, new Vector2(1.5f, 10f));   // vertical
        }
    }

    private void MakeBar(GameObject parent, Vector2 anchor, Vector2 pivot, Vector2 size)
    {
        var go  = new GameObject("B");
        go.transform.SetParent(parent.transform, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin      = anchor;
        rt.anchorMax      = anchor;
        rt.pivot          = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta      = size;
        var img = go.AddComponent<Image>();
        img.color = CornerAccent;
    }

    private void BeginAutoHide()
    {
        if (autoHideDelaySeconds <= 0f)
            return;

        if (autoHideRoutine != null)
            StopCoroutine(autoHideRoutine);

        autoHideRoutine = StartCoroutine(AutoHideAfterDelay());
    }

    private IEnumerator AutoHideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelaySeconds);
        SetVisible(false);
        autoHideRoutine = null;
    }

    private void NormalizeLocalZ()
    {
        NormalizeZ(questionText);
        NormalizeZ(progressText);
        NormalizeZ(feedbackText);
        NormalizeZ(summaryText);

        if (answerButtons != null)
        {
            for (int i = 0; i < answerButtons.Length; i++)
                NormalizeZ(answerButtons[i]);
        }

        if (answerTexts != null)
        {
            for (int i = 0; i < answerTexts.Length; i++)
                NormalizeZ(answerTexts[i]);
        }
    }

    private static void NormalizeZ(Component component)
    {
        if (component == null) return;
        var rt = component.GetComponent<RectTransform>();
        if (rt == null) return;
        var pos = rt.anchoredPosition3D;
        pos.z = 0f;
        rt.anchoredPosition3D = pos;
        rt.localScale = Vector3.one;
    }
}
