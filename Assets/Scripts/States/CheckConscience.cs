using UnityEngine;


public class CheckConscience : State
{
    [SerializeField] private ShakeGestureDetector shakeDetector;

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

        base.Exit();


    }

    private void OnShakeValidated(ShakeGestureDetector detector)
    {
        isConsciousnessCheckDone = true;
    }
}
