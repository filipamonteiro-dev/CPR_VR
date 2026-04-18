using UnityEngine;
using UnityEngine.UI;

namespace VrCpr
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private VrCprAppController appController;
        [SerializeField] private Button startTrainingButton;
        [SerializeField] private Button testModeButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            if (startTrainingButton != null)
            {
                startTrainingButton.onClick.AddListener(() => appController?.StartGuidedTraining());
            }

            if (testModeButton != null)
            {
                testModeButton.onClick.AddListener(() => appController?.StartTestMode());
            }

            if (tutorialButton != null)
            {
                tutorialButton.onClick.AddListener(() => appController?.OpenTutorial());
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(() => appController?.ExitToMainMenu());
            }
        }
    }
}