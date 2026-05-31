using UnityEngine;

public class Call112State : State
{
    [Header("Dialing")]
    [SerializeField] private PhoneDialer phoneDialer;
    [SerializeField] private ControllerButtonGate callButtonGate;
    [SerializeField] private FloatingToolPresenter phonePresenter;
    [SerializeField] private bool showPhoneInHolster = true;
    [SerializeField] private bool activatePhoneOnEnter = true;

    [Header("Quiz")]
    [SerializeField] private Call112QuizController quizController;
    [SerializeField] private bool startQuizImmediately;

    [Header("Guias UX")]
    [SerializeField] private PhoneWaypointHUD  waypointHUD;
    [SerializeField] private DialerFeedbackHUD dialerHUD;
    [SerializeField] private PhoneHighlightController phoneHighlight;

    private bool isFinished;
    private bool hasDialedEmergencyNumber;
    private QuizSession completedSession;

    public override void Enter()
    {
        base.Enter();

        isFinished = false;
        hasDialedEmergencyNumber = false;
        completedSession = null;

        if (waypointHUD  != null) waypointHUD.gameObject.SetActive(true);
        if (dialerHUD    != null) dialerHUD.gameObject.SetActive(true);
        if (phoneHighlight != null) phoneHighlight.enabled = true;

        if (callButtonGate != null)
        {
            callButtonGate.ResetGate();
            callButtonGate.CallRequested += OnCallRequested;
        }

        if (phonePresenter != null)
        {
            if (activatePhoneOnEnter && !phonePresenter.gameObject.activeSelf)
                phonePresenter.gameObject.SetActive(true);

            phonePresenter.SnapToHolster();

            if (showPhoneInHolster)
                phonePresenter.SetHolsterVisible(true);
        }

        if (phoneDialer != null)
        {
            phoneDialer.ResetDialer();
            phoneDialer.EmergencyNumberDialed += OnEmergencyNumberDialed;
        }

        if (quizController != null)
            quizController.QuizCompleted += OnQuizCompleted;

        if (startQuizImmediately)
            TryStartQuiz();
    }

    public override void Execute()
    {
    }

    public override void ForceFinished()
    {
        isFinished = true;
    }

    public override float GetCompletionProgress()
    {
        float dialProgress = hasDialedEmergencyNumber ? 0.5f : 0f;
        float quizProgress = 0f;

        QuizSession session = completedSession;
        if (session == null && quizController != null)
        {
            session = quizController.CurrentSession;
        }

        if (session != null && session.TotalAnswersSubmitted >= 0)
        {
            quizProgress = session.TotalQuestions > 0 ? session.Accuracy : 0f;
        }

        return Mathf.Clamp01(dialProgress + (quizProgress * 0.5f));
    }

    public override bool IsSuccessfulCompletion()
    {
        if (!hasDialedEmergencyNumber)
        {
            return false;
        }

        if (completedSession == null)
        {
            return false;
        }

        return completedSession.IsCompleted && completedSession.TotalAnswersSubmitted > 0 && completedSession.Accuracy >= 1f;
    }

    public override bool IsFinished()
    {
        return isFinished;
    }

    public override void Exit()
    {
        if (waypointHUD    != null) waypointHUD.gameObject.SetActive(false);
        if (dialerHUD      != null) dialerHUD.gameObject.SetActive(false);
        if (phoneHighlight != null) phoneHighlight.enabled = false;

        if (callButtonGate != null)
            callButtonGate.CallRequested -= OnCallRequested;

        if (phoneDialer != null)
            phoneDialer.EmergencyNumberDialed -= OnEmergencyNumberDialed;

        if (quizController != null)
            quizController.QuizCompleted -= OnQuizCompleted;

        base.Exit();
    }

    private void OnCallRequested()
    {
        TryStartQuiz();
    }

    private void OnEmergencyNumberDialed(PhoneDialer dialer)
    {
        hasDialedEmergencyNumber = true;
        TryStartQuiz();
    }

    private void OnQuizCompleted(QuizSession session)
    {
        completedSession = session;
        isFinished = true;
    }

    private void TryStartQuiz()
    {
        if (quizController == null || quizController.IsRunning)
            return;

        quizController.BeginQuiz();
    }
}
