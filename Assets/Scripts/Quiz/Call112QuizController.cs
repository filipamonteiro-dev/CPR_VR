using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Call112QuizController : MonoBehaviour
{
    [Serializable]
    public class QuizCompletedEvent : UnityEvent<QuizSession>
    {
    }

    [SerializeField] private CallQuestionBank questionBank;
    [SerializeField] private WorldSpaceQuizPresenter presenter;

    [Header("Flow")]
    [SerializeField] private bool shuffleQuestionOrder;
    [SerializeField] private bool retryQuestionUntilCorrect;
    [SerializeField] private bool showAnswerFeedback = true;
    [SerializeField] private bool hideCanvasOnComplete;

    [Header("Debug")]
    [SerializeField] private bool debugKeyboardStart;
    [SerializeField] private KeyCode debugStartKey = KeyCode.F8;
    [SerializeField] private bool debugRestartIfAlreadyRunning = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onQuizStarted;
    [SerializeField] private QuizCompletedEvent onQuizCompleted;

    public event Action<QuizSession> QuizCompleted;

    public bool IsRunning => session != null && !session.IsCompleted;
    public QuizSession CurrentSession => session;

    private QuizSession session;

    private void Awake()
    {
        if (presenter != null)
            presenter.SetVisible(false);
    }

    private void OnEnable()
    {
        if (presenter != null)
            presenter.AnswerSelected += OnAnswerSelected;
    }

    private void OnDisable()
    {
        if (presenter != null)
            presenter.AnswerSelected -= OnAnswerSelected;
    }

    private void Update()
    {
        if (!debugKeyboardStart)
            return;

        if (!Application.isEditor)
            return;

        if (!Input.GetKeyDown(debugStartKey))
            return;

        StartQuizFromDebug();
    }

    public void SetShowAnswerFeedback(bool value)
    {
        showAnswerFeedback = value;
        if (presenter != null)
            presenter.SetFeedbackVisibility(showAnswerFeedback);
    }

    public void BeginQuiz()
    {
        if (questionBank == null || !questionBank.HasQuestions || presenter == null)
            return;

        if (IsRunning)
            return;

        session = new QuizSession(questionBank, shuffleQuestionOrder);
        if (session.TotalQuestions == 0)
            return;

        presenter.SetVisible(true);
        presenter.SetFeedbackVisibility(showAnswerFeedback);
        ShowCurrentQuestion();

        onQuizStarted?.Invoke();
    }

    public void EndQuizAndHide()
    {
        session = null;
        if (presenter != null)
            presenter.SetVisible(false);
    }

    [ContextMenu("Debug/Start Quiz")]
    public void StartQuizFromDebug()
    {
        if (debugRestartIfAlreadyRunning && IsRunning)
            EndQuizAndHide();

        BeginQuiz();
    }

    private void OnAnswerSelected(int selectedIndex)
    {
        if (session == null || session.IsCompleted)
            return;

        QuizSubmitResult result = session.SubmitAnswer(selectedIndex, retryQuestionUntilCorrect);
        presenter.ShowAnswerFeedback(result.IsCorrect);

        if (!result.QuestionAdvanced && !result.QuizCompleted)
            return;

        if (result.QuizCompleted)
        {
            presenter.ShowCompletion(session);

            QuizCompleted?.Invoke(session);
            onQuizCompleted?.Invoke(session);

            if (hideCanvasOnComplete)
                presenter.SetVisible(false);

            return;
        }

        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        if (session == null || presenter == null)
            return;

        presenter.ShowQuestion(session.CurrentQuestion, session.CurrentQuestionNumber, session.TotalQuestions);
    }
}
