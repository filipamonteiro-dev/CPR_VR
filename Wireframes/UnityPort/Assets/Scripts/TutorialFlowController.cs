using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VrCpr
{
    public class TutorialFlowController : MonoBehaviour
    {
        [Header("Steps")]
        [SerializeField] private List<TutorialStepData> steps = new List<TutorialStepData>();
        [SerializeField] private int currentStepIndex;

        [Header("UI")]
        [SerializeField] private TMP_Text stepTitleText;
        [SerializeField] private TMP_Text stepLabelText;
        [SerializeField] private TMP_Text stepInstructionText;
        [SerializeField] private TMP_Text stepCounterText;

        [Header("Bindings")]
        [SerializeField] private PatientSilhouetteController silhouetteController;

        public int CurrentStepIndex => currentStepIndex;

        private void Start()
        {
            RefreshStep();
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void ResetFlow()
        {
            currentStepIndex = 0;
            RefreshStep();
        }

        public void NextStep()
        {
            if (currentStepIndex < steps.Count - 1)
            {
                currentStepIndex += 1;
                RefreshStep();
            }
        }

        public void PreviousStep()
        {
            if (currentStepIndex > 0)
            {
                currentStepIndex -= 1;
                RefreshStep();
            }
        }

        private void RefreshStep()
        {
            if (steps.Count == 0)
            {
                return;
            }

            var step = steps[Mathf.Clamp(currentStepIndex, 0, steps.Count - 1)];

            if (stepTitleText != null)
            {
                stepTitleText.text = step.title;
            }

            if (stepLabelText != null)
            {
                stepLabelText.text = step.label;
            }

            if (stepInstructionText != null)
            {
                stepInstructionText.text = step.instruction;
            }

            if (stepCounterText != null)
            {
                stepCounterText.text = $"{currentStepIndex + 1} / {steps.Count}";
            }

            if (silhouetteController != null)
            {
                silhouetteController.ApplyStep(step.highlight, step.showHandPlacement, step.showArrow);
            }
        }
    }
}