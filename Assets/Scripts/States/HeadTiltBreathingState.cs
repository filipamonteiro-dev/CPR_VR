using UnityEngine;

public class HeadTiltBreathingState : State
{
    [Header("Detection")]
    [SerializeField] private HeadTiltDetector headTiltDetector;
    [SerializeField] private Transform mannequinHead;
    [SerializeField] private Transform playerHead;
    [SerializeField] private float startDistance = 0.25f;

    [Header("Breathing Audio")]
    [SerializeField] private AudioSource breathingAudioSource;
    [SerializeField] private bool finishAfterClipEnds = true;
    [SerializeField] private float minimumPlaybackTime = 0.75f;
    [SerializeField] private bool stopAudioOnExit;

    private bool isFinished;
    private bool hasStartedBreathingAudio;
    private float playbackTimer;

    public override void Enter()
    {
        base.Enter();

        isFinished = false;
        hasStartedBreathingAudio = false;
        playbackTimer = 0f;

        if (breathingAudioSource != null)
            breathingAudioSource.Stop();

        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        if (headTiltDetector != null && !UsesProximity())
        {
            headTiltDetector.ResetDetector();
            headTiltDetector.TiltValidated += OnTiltValidated;
        }
    }

    public override void Execute()
    {
        if (!hasStartedBreathingAudio && UsesProximity())
        {
            if (IsPlayerNearMannequinHead())
                StartBreathingAudio();
        }

        if (isFinished || !hasStartedBreathingAudio)
            return;

        playbackTimer += Time.deltaTime;

        if (!finishAfterClipEnds)
        {
            if (playbackTimer >= minimumPlaybackTime)
                isFinished = true;

            return;
        }

        if (breathingAudioSource == null)
        {
            if (playbackTimer >= minimumPlaybackTime)
                isFinished = true;

            return;
        }

        bool waitForNaturalEnd = breathingAudioSource.clip != null && !breathingAudioSource.loop;
        if (!waitForNaturalEnd)
        {
            if (playbackTimer >= minimumPlaybackTime)
                isFinished = true;

            return;
        }

        if (!breathingAudioSource.isPlaying && playbackTimer >= minimumPlaybackTime)
            isFinished = true;
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
        if (headTiltDetector != null)
            headTiltDetector.TiltValidated -= OnTiltValidated;

        if (stopAudioOnExit && breathingAudioSource != null && breathingAudioSource.isPlaying)
            breathingAudioSource.Stop();

        base.Exit();
    }

    private void OnTiltValidated(HeadTiltDetector detector)
    {
        if (UsesProximity())
            return;

        StartBreathingAudio();
    }

    private bool UsesProximity()
    {
        return mannequinHead != null && playerHead != null;
    }

    private bool IsPlayerNearMannequinHead()
    {
        if (mannequinHead == null || playerHead == null)
            return false;

        float sqrDistance = (playerHead.position - mannequinHead.position).sqrMagnitude;
        return sqrDistance <= startDistance * startDistance;
    }

    private void StartBreathingAudio()
    {
        if (hasStartedBreathingAudio)
            return;

        hasStartedBreathingAudio = true;

        if (breathingAudioSource != null)
            breathingAudioSource.Play();

        if (breathingAudioSource == null && minimumPlaybackTime <= 0f)
            isFinished = true;
    }
}
