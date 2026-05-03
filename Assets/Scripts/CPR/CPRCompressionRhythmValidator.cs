using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class CPRCompressionRhythmValidator : MonoBehaviour
{
    [Serializable]
    public class RhythmFeedbackEvent : UnityEvent<CPRCompressionRhythmValidator, float, bool>
    {
    }

    [Header("References")]
    [SerializeField] private CPRCompressionPulseDetector pulseDetector;

    [Header("Rhythm")]
    [SerializeField] private int targetCompressions = 30;
    [SerializeField] private float targetBpm = 110f;
    [SerializeField] private float earlyWindowSeconds = 0.18f;
    [SerializeField] private float lateWindowSeconds = 0.20f;
    [SerializeField] private float perfectWindowSeconds = 0.08f;

    [Header("Events")]
    [SerializeField] private UnityEvent onSequenceCompleted;
    [SerializeField] private RhythmFeedbackEvent onRhythmFeedback;

    public event Action<CPRCompressionRhythmValidator> SequenceCompleted;
    public event Action<CPRCompressionRhythmValidator, float, bool> RhythmFeedback;

    public bool IsRunning { get; private set; }
    public bool IsComplete { get; private set; }
    public int CurrentStreak { get; private set; }
    public int BestStreak { get; private set; }
    public int MissCount { get; private set; }
    public float TargetBpm => targetBpm;
    public int TargetCompressions => targetCompressions;
    public float BeatInterval => 60f / Mathf.Max(1f, targetBpm);
    public float HitPhase01 => 0.5f;
    public float HitWindow01 => Mathf.Clamp01(Mathf.Max(earlyWindowSeconds, lateWindowSeconds) / BeatInterval);
    public float PerfectWindow01 => Mathf.Clamp01(perfectWindowSeconds / BeatInterval);
    public float BeatPhase01 => IsRunning ? Mathf.Repeat((Time.time - sessionStartTime) / BeatInterval, 1f) : 0f;
    public float Progress => targetCompressions <= 0 ? 1f : Mathf.Clamp01((float)CurrentStreak / targetCompressions);

    private float sessionStartTime;
    private bool hasSubscribedToPulse;

    private void OnEnable()
    {
        if (pulseDetector != null)
            SubscribeToPulseDetector();
    }

    private void OnDisable()
    {
        UnsubscribeFromPulseDetector();
    }

    public void BeginSession()
    {
        ResetSession();
        IsRunning = true;
        sessionStartTime = Time.time;
    }

    public void ResetSession()
    {
        IsRunning = false;
        IsComplete = false;
        CurrentStreak = 0;
        BestStreak = 0;
        MissCount = 0;
        sessionStartTime = Time.time;
    }

    public void SetTargetBpm(float bpm)
    {
        targetBpm = Mathf.Max(1f, bpm);
    }

    private void SubscribeToPulseDetector()
    {
        if (hasSubscribedToPulse || pulseDetector == null)
            return;

        pulseDetector.CompressionPressed += OnCompressionPressed;
        hasSubscribedToPulse = true;
    }

    private void UnsubscribeFromPulseDetector()
    {
        if (!hasSubscribedToPulse || pulseDetector == null)
            return;

        pulseDetector.CompressionPressed -= OnCompressionPressed;
        hasSubscribedToPulse = false;
    }

    private void OnCompressionPressed(CPRCompressionPulseDetector detector)
    {
        if (!IsRunning || IsComplete)
            return;

        float elapsed = Time.time - sessionStartTime;
        float beatInterval = 60f / Mathf.Max(1f, targetBpm);
        float expectedBeatTime = (CurrentStreak + HitPhase01) * beatInterval;
        float timingError = elapsed - expectedBeatTime;
        float absoluteError = Mathf.Abs(timingError);

        if (timingError < -earlyWindowSeconds || timingError > lateWindowSeconds)
        {
            RegisterMiss();
            return;
        }

        CurrentStreak++;
        BestStreak = Mathf.Max(BestStreak, CurrentStreak);

        bool wasPerfect = absoluteError <= perfectWindowSeconds;
        onRhythmFeedback?.Invoke(this, Mathf.Clamp01(1f - absoluteError / Mathf.Max(0.0001f, lateWindowSeconds)), wasPerfect);
        RhythmFeedback?.Invoke(this, Mathf.Clamp01(1f - absoluteError / Mathf.Max(0.0001f, lateWindowSeconds)), wasPerfect);

        if (CurrentStreak < targetCompressions)
            return;

        IsComplete = true;
        IsRunning = false;
        SequenceCompleted?.Invoke(this);
        onSequenceCompleted?.Invoke();
    }

    private void RegisterMiss()
    {
        MissCount++;
        BestStreak = Mathf.Max(BestStreak, CurrentStreak);
        CurrentStreak = 0;
        sessionStartTime = Time.time;
        onRhythmFeedback?.Invoke(this, 0f, false);
        RhythmFeedback?.Invoke(this, 0f, false);
    }
}