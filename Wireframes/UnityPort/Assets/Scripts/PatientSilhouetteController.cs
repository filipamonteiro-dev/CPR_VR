using UnityEngine;

namespace VrCpr
{
    public class PatientSilhouetteController : MonoBehaviour
    {
        [Header("Scene Parts")]
        [SerializeField] private GameObject fullBodyHighlight;
        [SerializeField] private GameObject chestHighlight;
        [SerializeField] private GameObject handPlacementOverlay;
        [SerializeField] private GameObject compressionArrow;

        [Header("Motion")]
        [SerializeField] private RectTransform bodyRoot;
        [SerializeField] private float compressionPunchStrength = 0.02f;

        private TutorialHighlight currentHighlight = TutorialHighlight.None;
        private bool showHands;
        private bool showArrow;

        public void ApplyStep(TutorialHighlight highlight, bool showHandPlacement, bool showCompressionArrow)
        {
            currentHighlight = highlight;
            showHands = showHandPlacement;
            showArrow = showCompressionArrow;
            RefreshVisibility();
        }

        public void SetCompressionDepth(float normalizedDepth)
        {
            if (bodyRoot != null)
            {
                bodyRoot.anchoredPosition = new Vector2(bodyRoot.anchoredPosition.x, -normalizedDepth * 100f * compressionPunchStrength);
            }
        }

        private void RefreshVisibility()
        {
            if (fullBodyHighlight != null)
            {
                fullBodyHighlight.SetActive(currentHighlight == TutorialHighlight.Full);
            }

            if (chestHighlight != null)
            {
                chestHighlight.SetActive(currentHighlight == TutorialHighlight.Chest);
            }

            if (handPlacementOverlay != null)
            {
                handPlacementOverlay.SetActive(showHands);
            }

            if (compressionArrow != null)
            {
                compressionArrow.SetActive(showArrow);
            }
        }
    }
}