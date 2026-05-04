using UnityEngine;

public class CPRCompressionState : State
{
    [Header("Detection")]
    [SerializeField] private CPRHandPlacementDetector handPlacementDetector;
    [SerializeField] private CPRSoftLockFollower softLockFollower;
    [SerializeField] private CPRCompressionPulseDetector pulseDetector;
    [SerializeField] private CPRCompressionRhythmValidator rhythmValidator;
    [SerializeField] private CPRRhythmHudPresenter rhythmHudPresenter;
    [SerializeField] private MannequinResetter mannequinResetter;

    private bool isFinished;
    private bool sessionStarted;

    public override void Enter()
    {
        base.Enter();

        isFinished = false;
        sessionStarted = false;

        if (handPlacementDetector != null)
        {
            handPlacementDetector.ResetDetector();
            handPlacementDetector.AlignmentLocked += OnAlignmentLocked;
        }

        if (pulseDetector != null)
            pulseDetector.ResetDetector();

        if (rhythmValidator != null)
        {
            rhythmValidator.ResetSession();
            rhythmValidator.SequenceCompleted += OnSequenceCompleted;
        }

        if (softLockFollower != null)
            softLockFollower.enabled = true;

        if (rhythmHudPresenter != null)
        {
            rhythmHudPresenter.ResetHud();
            rhythmHudPresenter.SetVisible(true);
        }

        if (mannequinResetter != null)
            mannequinResetter.SetRagdollEnabled(false);

        TryStartSession();
    }

    public override void Execute()
    {
        TryStartSession();
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
        if (handPlacementDetector != null)
            handPlacementDetector.AlignmentLocked -= OnAlignmentLocked;

        if (rhythmValidator != null)
            rhythmValidator.SequenceCompleted -= OnSequenceCompleted;

        if (softLockFollower != null)
            softLockFollower.enabled = false;

        if (rhythmHudPresenter != null)
            rhythmHudPresenter.SetVisible(false);

        if (mannequinResetter != null)
            mannequinResetter.SetRagdollEnabled(true);

        base.Exit();
    }

    private void OnAlignmentLocked(CPRHandPlacementDetector detector)
    {
        TryStartSession();
    }

    private void TryStartSession()
    {
        if (sessionStarted || handPlacementDetector == null || rhythmValidator == null)
            return;

        if (!handPlacementDetector.IsAligned)
            return;

        sessionStarted = true;
        rhythmValidator.BeginSession();
    }

    private void OnSequenceCompleted(CPRCompressionRhythmValidator validator)
    {
        isFinished = true;
    }
}