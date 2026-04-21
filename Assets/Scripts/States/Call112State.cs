using UnityEngine;

public class Call112State : State
{
    [SerializeField] private ControllerButtonGate callButtonGate;
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

        if (quizController != null)
            quizController.QuizCompleted -= OnQuizCompleted;

        base.Exit();
    }

    private void OnCallRequested()
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
