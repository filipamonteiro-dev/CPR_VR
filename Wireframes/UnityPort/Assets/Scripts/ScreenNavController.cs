using UnityEngine;
using UnityEngine.UI;

namespace VrCpr
{
    public class ScreenNavController : MonoBehaviour
    {
        [SerializeField] private VrCprAppController appController;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button trainingButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button testButton;

        [SerializeField] private Image mainMenuState;
        [SerializeField] private Image trainingState;
        [SerializeField] private Image tutorialState;
        [SerializeField] private Image testState;

        private void Awake()
        {
            BindState(mainMenuButton, mainMenuState, AppScreen.MainMenu);
            BindState(trainingButton, trainingState, AppScreen.Training);
            BindState(tutorialButton, tutorialState, AppScreen.Tutorial);
            BindState(testButton, testState, AppScreen.Test);
        }

        public void SetActiveScreen(AppScreen screen)
        {
            SetState(mainMenuState, screen == AppScreen.MainMenu);
            SetState(trainingState, screen == AppScreen.Training);
            SetState(tutorialState, screen == AppScreen.Tutorial);
            SetState(testState, screen == AppScreen.Test);
        }

        private void BindState(Button button, Image state, AppScreen screen)
        {
            if (button != null)
            {
                button.interactable = true;

                button.onClick.AddListener(() =>
                {
                    if (appController == null)
                    {
                        return;
                    }

                    switch (screen)
                    {
                        case AppScreen.MainMenu:
                            appController.ShowMainMenu();
                            break;
                        case AppScreen.Training:
                            appController.StartGuidedTraining();
                            break;
                        case AppScreen.Tutorial:
                            appController.OpenTutorial();
                            break;
                        case AppScreen.Test:
                            appController.StartTestMode();
                            break;
                    }
                });
            }

            SetState(state, false);
        }

        private static void SetState(Image image, bool active)
        {
            if (image != null)
            {
                image.enabled = active;
            }
        }
    }
}