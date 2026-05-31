using UnityEngine;


public class CheckConscience : State
{
    [SerializeField] private ShakeGestureDetector shakeDetector;
    [SerializeField] private MannequinResetter mannequinResetter;

    private bool isConsciousnessCheckDone;

    public override void Enter()
    {
        base.Enter();

        isConsciousnessCheckDone = false;

        if (shakeDetector != null)
        {
            shakeDetector.ResetDetector();
            shakeDetector.ShakeValidated += OnShakeValidated;
        }

        if (mannequinResetter != null)
            mannequinResetter.SetRagdollEnabled(true);

    }
    public override void Execute()
    {
    }

    public override void ForceFinished()
    {
        isConsciousnessCheckDone = true;
    }

    public override bool IsFinished()
    {
        return isConsciousnessCheckDone;
    }

    public override void Exit()
    {
        if (shakeDetector != null)
            shakeDetector.ShakeValidated -= OnShakeValidated;

        if (mannequinResetter != null)
            mannequinResetter.SetRagdollEnabled(false);

        base.Exit();


    }

    private void OnShakeValidated(ShakeGestureDetector detector)
    {
        isConsciousnessCheckDone = true;
    }
}
