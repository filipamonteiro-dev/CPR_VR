using System;
using System.Collections.Generic;
using UnityEngine;

public class QuizSession
{
    private readonly CallQuestionBank bank;
    private readonly List<int> questionOrder = new List<int>();

    private int currentQuestionPointer;
    private readonly float startTime;

    public int CorrectCount { get; private set; }
    public int WrongCount { get; private set; }
    public int TotalAnswersSubmitted => CorrectCount + WrongCount;
    public bool IsCompleted => currentQuestionPointer >= questionOrder.Count;
    public float DurationSeconds => IsCompleted ? completedTime - startTime : Time.time - startTime;

    private float completedTime;

    public int CurrentQuestionNumber => Mathf.Min(currentQuestionPointer + 1, TotalQuestions);
    public int TotalQuestions => questionOrder.Count;

    public CallQuestionData CurrentQuestion
    {
        get
        {
            if (IsCompleted || TotalQuestions == 0)
                return null;

            int index = questionOrder[currentQuestionPointer];
            return bank.Questions[index];
        }
    }

    public float Accuracy
    {
        get
        {
            int total = TotalAnswersSubmitted;
            if (total == 0)
                return 0f;

            return (float)CorrectCount / total;
        }
    }

    public QuizSession(CallQuestionBank questionBank, bool shuffleQuestionOrder)
    {
        bank = questionBank;
        BuildQuestionOrder(shuffleQuestionOrder);
        startTime = Time.time;
    }

    public QuizSubmitResult SubmitAnswer(int selectedOptionIndex, bool retryQuestionUntilCorrect)
    {
        if (IsCompleted)
            return QuizSubmitResult.Completed;

        CallQuestionData question = CurrentQuestion;
        if (question == null)
            return QuizSubmitResult.Completed;

        bool isCorrect = selectedOptionIndex == question.CorrectOptionIndex;
        bool questionAdvanced = false;

        if (isCorrect)
        {
            CorrectCount++;
            currentQuestionPointer++;
            questionAdvanced = true;
        }
        else
        {
            WrongCount++;
            if (!retryQuestionUntilCorrect)
            {
                currentQuestionPointer++;
                questionAdvanced = true;
            }
        }

        if (IsCompleted)
            completedTime = Time.time;

        return new QuizSubmitResult(isCorrect, questionAdvanced, IsCompleted, question.CorrectOptionIndex);
    }

    private void BuildQuestionOrder(bool shuffleQuestionOrder)
    {
        questionOrder.Clear();

        if (bank == null || bank.Questions == null)
            return;

        for (int i = 0; i < bank.Questions.Count; i++)
        {
            if (bank.Questions[i] != null && bank.Questions[i].IsValid())
                questionOrder.Add(i);
        }

        if (!shuffleQuestionOrder)
            return;

        for (int i = questionOrder.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (questionOrder[i], questionOrder[j]) = (questionOrder[j], questionOrder[i]);
        }
    }
}

public readonly struct QuizSubmitResult
{
    public static readonly QuizSubmitResult Completed = new QuizSubmitResult(false, false, true, -1);

    public bool IsCorrect { get; }
    public bool QuestionAdvanced { get; }
    public bool QuizCompleted { get; }
    public int CorrectOptionIndex { get; }

    public QuizSubmitResult(bool isCorrect, bool questionAdvanced, bool quizCompleted, int correctOptionIndex)
    {
        IsCorrect = isCorrect;
        QuestionAdvanced = questionAdvanced;
        QuizCompleted = quizCompleted;
        CorrectOptionIndex = correctOptionIndex;
    }
}
