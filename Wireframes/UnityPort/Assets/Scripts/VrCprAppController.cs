using UnityEngine;

namespace VrCpr
{
    public class VrCprAppController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private GameObject trainingPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject screenNavPanel;

        [Header("Controllers")]
        [SerializeField] private TrainingSessionController trainingSession;
        [SerializeField] private TutorialFlowController tutorialFlow;
        [SerializeField] private ScreenNavController screenNav;

        public AppScreen CurrentScreen { get; private set; } = AppScreen.MainMenu;

        private void Awake()
        {
            ApplyScreen(AppScreen.MainMenu);
        }

        public void ShowMainMenu()
        {
            ApplyScreen(AppScreen.MainMenu);
        }

        public void StartGuidedTraining()
        {
            if (trainingSession != null)
            {
                trainingSession.SetMode(TrainingMode.Guided);
                trainingSession.ResetSession();
            }

            ApplyScreen(AppScreen.Training);
        }

        public void StartTestMode()
        {
            if (trainingSession != null)
            {
                trainingSession.SetMode(TrainingMode.Test);
                trainingSession.ResetSession();
            }

            ApplyScreen(AppScreen.Test);
        }

        public void OpenTutorial()
        {
            if (tutorialFlow != null)
            {
                tutorialFlow.ResetFlow();
            }

            ApplyScreen(AppScreen.Tutorial);
        }

        public void TogglePause()
        {
            if (CurrentScreen == AppScreen.Pause)
            {
                ResumeSession();
                return;
            }

            if (CurrentScreen == AppScreen.Training || CurrentScreen == AppScreen.Test)
            {
                ApplyScreen(AppScreen.Pause);
                if (trainingSession != null)
                {
                    trainingSession.SetPaused(true);
                }
            }
        }

        public void ResumeSession()
        {
            if (CurrentScreen != AppScreen.Pause)
            {
                return;
            }

            var nextScreen = trainingSession != null && trainingSession.CurrentMode == TrainingMode.Test
                ? AppScreen.Test
                : AppScreen.Training;

            ApplyScreen(nextScreen);

            if (trainingSession != null)
            {
                trainingSession.SetPaused(false);
            }
        }

        public void RestartSession()
        {
            if (trainingSession != null)
            {
                trainingSession.ResetSession();
                trainingSession.SetPaused(false);
            }

            var nextScreen = trainingSession != null && trainingSession.CurrentMode == TrainingMode.Test
                ? AppScreen.Test
                : AppScreen.Training;

            ApplyScreen(nextScreen);
        }

        public void ExitToMainMenu()
        {
            if (trainingSession != null)
            {
                trainingSession.SetPaused(false);
            }

            ApplyScreen(AppScreen.MainMenu);
        }

        private void ApplyScreen(AppScreen screen)
        {
            CurrentScreen = screen;

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(screen == AppScreen.MainMenu);
            }

            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(screen == AppScreen.Tutorial);
            }

            if (trainingPanel != null)
            {
                trainingPanel.SetActive(screen == AppScreen.Training || screen == AppScreen.Test);
            }

            if (pausePanel != null)
            {
                pausePanel.SetActive(screen == AppScreen.Pause);
            }

            if (screenNavPanel != null)
            {
                screenNavPanel.SetActive(true);
            }

            if (screenNav != null)
            {
                screenNav.SetActiveScreen(screen);
            }

            if (tutorialFlow != null)
            {
                tutorialFlow.SetVisible(screen == AppScreen.Tutorial);
            }
        }
    }
}