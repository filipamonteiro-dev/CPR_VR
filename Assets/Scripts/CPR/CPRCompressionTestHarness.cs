using UnityEngine;

[DisallowMultipleComponent]
public class CPRCompressionTestHarness : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private CPRHandPlacementDetector handPlacementDetector;
    [SerializeField] private CPRSoftLockFollower softLockFollower;
    [SerializeField] private CPRCompressionPulseDetector pulseDetector;
    [SerializeField] private CPRCompressionRhythmValidator rhythmValidator;
    [SerializeField] private CPRRhythmHudPresenter rhythmHudPresenter;
    [SerializeField] private MannequinResetter mannequinResetter;

    [Header("Test")]
    [SerializeField] private bool startOnEnable;

    public bool IsRunning { get; private set; }

    private bool sessionStarted;

    private void OnEnable()
    {
        if (startOnEnable)
            StartTest();
    }

    private void OnDisable()
    {
        StopTest();
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        TryStartSession();
    }

    [ContextMenu("Start Test")]
    public void StartTest()
    {
        if (IsRunning)
            return;

        IsRunning = true;
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

    [ContextMenu("Stop Test")]
    public void StopTest()
    {
        if (!IsRunning)
            return;

        IsRunning = false;
        sessionStarted = false;

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
        StopTest();
    }
}
