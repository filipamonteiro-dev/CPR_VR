using System.Collections;
using UnityEngine;

/// <summary>
/// Small runtime tester to exercise the tutorial flow in Play Mode.
/// Attach to any GameObject in the scene (or leave unassigned) and use
/// the component context menu "Run Tutorial Test" while in Play Mode.
/// It will advance steps with delays and log visible UI text via the
/// getters exposed on `TutorialFlowController`.
/// </summary>
public class TestTutorialRunner : MonoBehaviour
{
    [Tooltip("Reference to the TutorialFlowController in the scene. If null, will find one automatically.")]
    public TutorialFlowController tutorialFlow;

    [Tooltip("Delay between steps while testing (seconds).")]
    public float stepDelay = 0.8f;

    [ContextMenu("Run Tutorial Test")]
    public void RunTest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[TestTutorialRunner] Enter Play Mode to run the test.");
            return;
        }

        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        if (tutorialFlow == null)
            tutorialFlow = FindAnyObjectByType<TutorialFlowController>();

        if (tutorialFlow == null)
        {
            Debug.LogError("[TestTutorialRunner] No TutorialFlowController found in scene.");
            yield break;
        }

        tutorialFlow.ResetFlow();
        yield return null; // allow one frame for UI to update

        int total = tutorialFlow.StepsCount;
        Debug.Log($"[TestTutorialRunner] StepsCount={total}");

        for (int i = 0; i < total; i++)
        {
            Debug.Log($"[TestTutorialRunner] Step {i + 1}/{total} — Index={tutorialFlow.CurrentStepIndex}");
            Debug.Log($"  Title: {tutorialFlow.GetCurrentTitle()}");
            Debug.Log($"  Label: {tutorialFlow.GetCurrentLabel()}");
            Debug.Log($"  Instr: {tutorialFlow.GetCurrentInstruction()}");
            Debug.Log($"  Counter: {tutorialFlow.GetCurrentCounter()}");

            yield return new WaitForSeconds(stepDelay);

            // Advance unless last
            if (i < total - 1)
                tutorialFlow.NextStep();
        }

        Debug.Log("[TestTutorialRunner] Completed sequence.");
    }
}
