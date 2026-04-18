using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VrCpr
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text compressionCountText;
        [SerializeField] private TMP_Text averageDepthText;
        [SerializeField] private TMP_Text precisionText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [SerializeField] private UnityEvent onResume;
        [SerializeField] private UnityEvent onRestart;
        [SerializeField] private UnityEvent onSettings;
        [SerializeField] private UnityEvent onExit;

        private void Awake()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(() => onResume?.Invoke());
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(() => onRestart?.Invoke());
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(() => onSettings?.Invoke());
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(() => onExit?.Invoke());
            }
        }

        public void SetVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        public void SetSnapshot(SessionSnapshot snapshot)
        {
            if (compressionCountText != null)
            {
                compressionCountText.text = snapshot.compressions.ToString();
            }

            if (averageDepthText != null)
            {
                averageDepthText.text = $"{snapshot.depth:0.0}cm";
            }

            if (precisionText != null)
            {
                precisionText.text = $"{snapshot.score}%";
            }
        }
    }
}