using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class WorldSpaceQuizPresenter : MonoBehaviour
{
    [SerializeField] private GameObject canvasRoot;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TMP_Text[] answerTexts;

    [Header("Feedback")]
    [SerializeField] private Color correctColor = new Color(0.24f, 0.68f, 0.28f);
    [SerializeField] private Color wrongColor = new Color(0.82f, 0.2f, 0.2f);

    private bool showFeedback = true;

    public event Action<int> AnswerSelected;

    private void Awake()
    {
        BindButtons();
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
    }

    public void ShowAnswerFeedback(bool isCorrect)
    {
        if (!showFeedback || feedbackText == null)
            return;

        feedbackText.text = isCorrect ? "Correto" : "Errado";
        feedbackText.color = isCorrect ? correctColor : wrongColor;
    }

    public void ShowCompletion(QuizSession session)
    {
        SetButtonsInteractable(false);

        if (questionText != null)
            questionText.text = "Chamada para 112 concluida";

        if (progressText != null)
            progressText.text = "Fim";

        if (summaryText != null && session != null)
        {
            float accuracyPercent = session.Accuracy * 100f;
            summaryText.text = $"Certas: {session.CorrectCount} | Erradas: {session.WrongCount} | Precisao: {accuracyPercent:0}%";
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
            answerButtons[i].onClick.AddListener(() => AnswerSelected?.Invoke(capturedIndex));
        }
    }
}
