using System;
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

    [Tooltip("Background color for answer buttons.")]
    [SerializeField] private Color buttonBgColor = new Color(0.06f, 0.07f, 0.09f, 1f);

    [Tooltip("Text color for answer labels.")]
    [SerializeField] private Color buttonTextColor = new Color(1f, 1f, 1f, 0.9f);

    [Header("Feedback")]
    [SerializeField] private Color correctColor = new Color(0.24f, 0.68f, 0.28f);
    [SerializeField] private Color wrongColor = new Color(0.82f, 0.2f, 0.2f);

    private bool showFeedback = true;
    private int lastSelectedIndex = -1;

    public event Action<int> AnswerSelected;

    private void Awake()
    {
        BindButtons();
        ApplyStyleToAllButtons();
    }

    public void SetVisible(bool visible)
    {
        if (canvasRoot != null)
            canvasRoot.SetActive(visible);
        else
            gameObject.SetActive(visible);
    }

    public void SetFeedbackVisibility(bool visible)
    {
        showFeedback = visible;
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(visible);
    }

    public void ShowQuestion(CallQuestionData question, int questionNumber, int totalQuestions)
    {
        if (questionText != null)
            questionText.text = question != null ? question.Prompt : string.Empty;

        if (progressText != null)
            progressText.text = $"Pergunta {questionNumber}/{Mathf.Max(1, totalQuestions)}";

        if (summaryText != null)
            summaryText.text = string.Empty;

        if (feedbackText != null)
            feedbackText.text = string.Empty;

        ConfigureAnswerButtons(question);
        ApplyStyleToAllButtons();
        lastSelectedIndex = -1;
    }

    public void ShowAnswerFeedback(bool isCorrect)
    {
        if (!showFeedback || feedbackText == null)
            return;

        feedbackText.text = isCorrect ? "Correto" : "Errado";
        feedbackText.color = isCorrect ? correctColor : wrongColor;

        if (lastSelectedIndex >= 0 && answerButtons != null && lastSelectedIndex < answerButtons.Length)
        {
            var btn = answerButtons[lastSelectedIndex];
            if (btn != null)
            {
                Image bg = btn.GetComponent<Image>() ?? btn.targetGraphic as Image;
                if (bg != null)
                    bg.color = isCorrect ? correctColor : wrongColor;

                TMP_Text txt = (answerTexts != null && lastSelectedIndex < answerTexts.Length) ? answerTexts[lastSelectedIndex] : null;
                if (txt != null)
                    txt.color = Color.white;
            }
        }
    }

    public void ShowCompletion(QuizSession session)
    {
        SetButtonsInteractable(false);

        if (questionText != null)
            questionText.text = "Chamada para 112 concluída";

        if (progressText != null)
            progressText.text = "Fim";

        if (summaryText != null && session != null)
        {
            float accuracyPercent = session.Accuracy * 100f;
            summaryText.text = $"Certas: {session.CorrectCount} | Erradas: {session.WrongCount} | Precisão: {accuracyPercent:0}%";
        }
    }

    private void ConfigureAnswerButtons(CallQuestionData question)
    {
        if (answerButtons == null)
            return;

        int optionCount = question != null && question.Options != null ? question.Options.Count : 0;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool active = i < optionCount;
            if (answerButtons[i] != null)
            {
                answerButtons[i].gameObject.SetActive(active);
                answerButtons[i].interactable = active;
                ApplyStyleToButton(i);
            }

            if (answerTexts != null && i < answerTexts.Length && answerTexts[i] != null)
                answerTexts[i].text = active ? question.Options[i] : string.Empty;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (answerButtons == null)
            return;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null && answerButtons[i].gameObject.activeInHierarchy)
                answerButtons[i].interactable = interactable;
        }
    }

    private void BindButtons()
    {
        if (answerButtons == null)
            return;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null)
                continue;

            int capturedIndex = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() =>
            {
                lastSelectedIndex = capturedIndex;
                AnswerSelected?.Invoke(capturedIndex);
            });
        }
    }

    // ── Styling helpers ─────────────────────────────────────────────────────

    private void ApplyStyleToAllButtons()
    {
        if (answerButtons == null)
            return;

        for (int i = 0; i < answerButtons.Length; i++)
            ApplyStyleToButton(i);
    }

    private void ApplyStyleToButton(int index)
    {
        if (answerButtons == null || index < 0 || index >= answerButtons.Length)
            return;

        var btn = answerButtons[index];
        TMP_Text txt = (answerTexts != null && index < answerTexts.Length) ? answerTexts[index] : null;

        Image bg = btn.GetComponent<Image>() ?? btn.targetGraphic as Image;
        if (bg != null)
            bg.color = buttonBgColor;

        if (txt != null)
        {
            if (uiFont != null)
                txt.font = uiFont;
            txt.color = buttonTextColor;
        }

        var outline = btn.GetComponent<Outline>();
        if (outline == null)
            outline = btn.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.06f);
        outline.effectDistance = new Vector2(1f, -1f);
    }

    private void ResetButtonVisual(int index)
    {
        if (answerButtons == null || index < 0 || index >= answerButtons.Length)
            return;

        var btn = answerButtons[index];
        Image bg = btn.GetComponent<Image>() ?? btn.targetGraphic as Image;
        if (bg != null)
            bg.color = buttonBgColor;

        TMP_Text txt = (answerTexts != null && index < answerTexts.Length) ? answerTexts[index] : null;
        if (txt != null)
            txt.color = buttonTextColor;
    }
}
