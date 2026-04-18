using System;

namespace VrCpr
{
    public enum AppScreen
    {
        MainMenu,
        Tutorial,
        Training,
        Test,
        Pause
    }

    public enum TutorialHighlight
    {
        None,
        Full,
        Chest,
        Hands,
        Head
    }

    public enum TrainingMode
    {
        Guided,
        Test
    }

    [Serializable]
    public class TutorialStepData
    {
        public int id;
        public string label;
        public string title;
        public string instruction;
        public TutorialHighlight highlight;
        public bool showArrow;
        public bool showHandPlacement;
    }

    [Serializable]
    public struct SessionSnapshot
    {
        public int elapsedSeconds;
        public int compressions;
        public int score;
        public float bpm;
        public float depth;
        public string feedback;
    }
}