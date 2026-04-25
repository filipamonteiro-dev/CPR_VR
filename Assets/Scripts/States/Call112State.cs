using UnityEngine;

public class Call112State : State
{
    [Header("Dialing")]
    [SerializeField] private PhoneDialer phoneDialer;
    [SerializeField] private ControllerButtonGate callButtonGate;

    [Header("Quiz")]
    [SerializeField] private Call112QuizController quizController;
    [SerializeField] private bool startQuizImmediately;

    private bool isFinished;

    public override void Enter()
    {
        base.Enter();

        isFinished = false;

        if (callButtonGate != null)
        {
            callButtonGate.ResetGate();
            callButtonGate.CallRequested += OnCallRequested;
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

    public override bool IsFinished()
    {
        return isFinished;
    }

    public override void Exit()
    {
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
        TryStartQuiz();
    }

    private void OnQuizCompleted(QuizSession session)
    {
        isFinished = true;
    }

    private void TryStartQuiz()
    {
        if (quizController == null || quizController.IsRunning)
            return;

        quizController.BeginQuiz();
    }
}
