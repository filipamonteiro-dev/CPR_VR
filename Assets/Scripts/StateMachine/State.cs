using System;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    [SerializeField] protected bool m_UseHighlight;
    [SerializeField] string m_StateLabel;

    [TextArea(3, 10)]
    [SerializeField] string m_SubtitlesText;

    [Header("Success Feedback")]
    [SerializeField] private bool playSuccessOnExit = true;
    [SerializeField] private AudioClip successClip;
    [SerializeField, Range(0f, 1f)] private float successVolume = 0.2f;
    [SerializeField] private AudioSource successAudioSource;
   
    public string StateLabel => m_StateLabel;

    private bool m_ExitWasSuccessful = true;

    public event Action<State> OnEnter;
    public event Action<State> OnExit;
    public event Action<State> OnAskToBeActive;
    protected void RaiseOnEnter() => OnEnter?.Invoke(this);

    /// <summary>
    /// Called when the state is entered
    /// </summary>
    public virtual void Enter()
    {
        m_ExitWasSuccessful = true;
        RaiseOnEnter();
    }

    /// <summary>
    /// Called every frame while the state is active
    /// </summary>
    public abstract void Execute();

    /// <summary>
    /// Called when the state is exited
    /// </summary>
    public virtual void Exit()
    {
        if (playSuccessOnExit && m_ExitWasSuccessful)
            PlaySuccessCue();

        OnExit?.Invoke(this);
    }

    /// <summary>
    /// Check if the state is done and ready to move to the next state
    /// </summary>
    /// <returns>True if state finished </returns>
    public abstract bool IsFinished();

    public abstract void ForceFinished();

    protected virtual void AskToBeActive()
    {
        OnAskToBeActive?.Invoke(this);
    }

    public void SetUseHighlight(bool useHighlight)
    {
        m_UseHighlight = useHighlight;
    }

    public virtual State GetState() => this;

    public string GetSubtitles() => m_SubtitlesText;

    public virtual float GetCompletionProgress()
    {
        return IsFinished() ? 1f : 0f;
    }

    public virtual bool IsSuccessfulCompletion()
    {
        return IsFinished();
    }

    public void SetExitOutcome(bool wasSuccessful)
    {
        m_ExitWasSuccessful = wasSuccessful;
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/Complete State")]
    private void DebugCompleteState()
    {
        ForceFinished();
    }
#endif

    protected void PlaySuccessCue()
    {
        if (successClip == null)
            return;

        var source = GetOrCreateSuccessAudioSource();
        if (source == null)
            return;

        source.PlayOneShot(successClip, Mathf.Clamp01(successVolume));
    }

    private AudioSource GetOrCreateSuccessAudioSource()
    {
        if (successAudioSource != null)
            return successAudioSource;

        successAudioSource = GetComponentInParent<AudioSource>(true);
        if (successAudioSource != null)
            return successAudioSource;

        successAudioSource = gameObject.AddComponent<AudioSource>();
        successAudioSource.playOnAwake = false;
        return successAudioSource;
    }

   

}
