using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CallQuestionBank", menuName = "CPR VR/Quiz/Call Question Bank")]
public class CallQuestionBank : ScriptableObject
{
    [SerializeField] private List<CallQuestionData> questions = new List<CallQuestionData>();

    public IReadOnlyList<CallQuestionData> Questions => questions;

    public bool HasQuestions => questions != null && questions.Count > 0;
}

[Serializable]
public class CallQuestionData
{
    [SerializeField] private string questionId;
    [TextArea(2, 6)]
    [SerializeField] private string prompt;
    [SerializeField] private List<string> options = new List<string>();
    [SerializeField] private int correctOptionIndex;
    [SerializeField] private bool criticalQuestion;

    public string QuestionId => string.IsNullOrWhiteSpace(questionId) ? Guid.NewGuid().ToString("N") : questionId;
    public string Prompt => prompt;
    public IReadOnlyList<string> Options => options;
    public int CorrectOptionIndex => correctOptionIndex;
    public bool CriticalQuestion => criticalQuestion;

    public bool IsValid()
    {
        if (options == null || options.Count == 0)
            return false;

        return correctOptionIndex >= 0 && correctOptionIndex < options.Count;
    }
}
