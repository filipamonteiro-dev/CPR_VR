using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Equivalente ao CompressionGauge do React.
/// Gauge vertical que mostra profundidade de compressão (0–6 cm).
///
/// Hierarquia esperada no Canvas:
///   CompressionGauge (este script)
///   ├── FillBar        (Image, tipo Filled, Fill Method: Vertical, Fill Origin: Bottom)
///   ├── TargetZone     (Image, semitransparente, posição gerida por este script)
///   ├── LabelTop       (TextMeshProUGUI — "6cm")
///   ├── LabelBottom    (TextMeshProUGUI — "4cm")
///   ├── ValueText      (TextMeshProUGUI — valor actual ex: "4.2")
///   └── UnitText       (TextMeshProUGUI — "cm")
/// </summary>
public class CompressionGauge : MonoBehaviour
{
    [Header("Referências")]
    public Image            fillBar;
    public RectTransform    targetZone;     // overlay da zona alvo
    public TextMeshProUGUI  valueText;

    [Header("Parâmetros")]
    public float maxDepth   = 6f;
    public float targetMin  = 4f;   // cm
    public float targetMax  = 6f;   // cm

    // Cores
    private static readonly Color colorInZone   = new Color(1f, 1f, 1f, 0.35f);
    private static readonly Color colorTooShallow = new Color(1f, 1f, 1f, 0.18f);
    private static readonly Color colorTooDeep  = new Color(1f, 0.47f, 0.47f, 0.5f);
    private static readonly Color colorValueOn  = new Color(1f, 1f, 1f, 0.8f);
    private static readonly Color colorValueOff = new Color(1f, 1f, 1f, 0.4f);

    void Start()
    {
        // Posiciona a target zone (zona alvo entre 4cm e 6cm)
        if (targetZone != null)
        {
            float minPct = targetMin / maxDepth;
            float maxPct = targetMax / maxDepth;
            // ancoragem relativa à altura total do gauge
            targetZone.anchorMin = new Vector2(0f, minPct);
            targetZone.anchorMax = new Vector2(1f, maxPct);
            targetZone.offsetMin = Vector2.zero;
            targetZone.offsetMax = Vector2.zero;
        }
    }

    /// <summary>Chamado a cada frame pelo TrainingManager.</summary>
    public void SetDepth(float depth)
    {
        float pct   = Mathf.Clamp01(depth / maxDepth);
        bool inZone = depth >= targetMin && depth <= targetMax;

        // Fill bar
        if (fillBar != null)
        {
            fillBar.fillAmount = pct;
            fillBar.color = inZone     ? colorInZone
                          : depth < targetMin ? colorTooShallow
                                              : colorTooDeep;
        }

        // Valor
        if (valueText != null)
        {
            valueText.text  = depth.ToString("F1");
            valueText.color = inZone ? colorValueOn : colorValueOff;
        }
    }
}
