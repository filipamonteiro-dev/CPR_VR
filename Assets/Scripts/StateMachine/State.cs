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

    public event Action<State> OnEnter;
    public event Action<State> OnExit;
    public event Action<State> OnAskToBeActive;
    protected void RaiseOnEnter() => OnEnter?.Invoke(this);

    /// <summary>
    /// Called when the state is entered
    /// </summary>
    public virtual void Enter()
    {
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
        if (playSuccessOnExit)
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
