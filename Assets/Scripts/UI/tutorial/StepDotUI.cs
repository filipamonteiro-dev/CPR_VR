using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Equivalente ao componente StepDot do Tutorial.tsx.
/// Representa um círculo no header que indica o estado de cada passo.
///
/// ── Hierarquia esperada ────────────────────────────────────────────────
///   StepDot (este script + RectTransform)
///   ├── Border        (Image — outline, troca entre solid/dashed via sprite)
///   ├── Fill          (Image — fundo semitransparente)
///   ├── NumberText    (TextMeshProUGUI — número do passo)
///   └── CheckText     (TextMeshProUGUI — "✓", desactivado por defeito)
///
/// Cria 6 prefabs iguais e coloca no HorizontalLayoutGroup do header.
/// </summary>
public class StepDotUI : MonoBehaviour
{
    [Header("Referências")]
    public RectTransform  dotRect;
    public Image          borderImage;
    public Image          fillImage;
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI checkText;

    [Header("Sprites de borda")]
    [Tooltip("Sprite com borda sólida (passo activo)")]
    public Sprite solidBorderSprite;
    [Tooltip("Sprite com borda tracejada (passo inactivo)")]
    public Sprite dashedBorderSprite;

    [Header("Tamanhos")]
    public float sizeActive   = 28f;
    public float sizeInactive = 20f;

    // Cores exactas do React
    private static readonly Color borderComplete = new Color(1f, 1f, 1f, 0.60f);
    private static readonly Color borderActive   = new Color(1f, 1f, 1f, 0.80f);
    private static readonly Color borderInactive = new Color(1f, 1f, 1f, 0.20f);

    private static readonly Color fillComplete  = new Color(1f, 1f, 1f, 0.15f);
    private static readonly Color fillActive    = new Color(1f, 1f, 1f, 0.08f);
    private static readonly Color fillInactive  = new Color(0f, 0f, 0f, 0f);

    private static readonly Color numberActive   = new Color(1f, 1f, 1f, 0.90f);
    private static readonly Color numberInactive = new Color(1f, 1f, 1f, 0.25f);
    private static readonly Color checkColor     = new Color(1f, 1f, 1f, 0.70f);

    /// <summary>Chamado pelo TutorialManager a cada mudança de passo.</summary>
    public void SetState(bool active, bool complete)
    {
        // ── Tamanho ────────────────────────────────────────────────────
        float size = active ? sizeActive : sizeInactive;
        if (dotRect != null)
            dotRect.sizeDelta = new Vector2(size, size);

        // ── Borda ──────────────────────────────────────────────────────
        if (borderImage != null)
        {
            borderImage.color = complete ? borderComplete
                              : active   ? borderActive
                                         : borderInactive;

            // Sólido quando activo, tracejado caso contrário
            if (solidBorderSprite  != null && dashedBorderSprite != null)
                borderImage.sprite = active ? solidBorderSprite : dashedBorderSprite;
        }

        // ── Fundo ──────────────────────────────────────────────────────
        if (fillImage != null)
            fillImage.color = complete ? fillComplete
                            : active   ? fillActive
                                       : fillInactive;

        // ── Conteúdo: número ou check ──────────────────────────────────
        bool showCheck = complete;

        if (numberText != null)
        {
            numberText.gameObject.SetActive(!showCheck);
            numberText.color = active ? numberActive : numberInactive;
            // O número é definido uma vez na inicialização via SetIndex()
        }

        if (checkText != null)
        {
            checkText.gameObject.SetActive(showCheck);
            checkText.color = checkColor;
        }
    }

    /// <summary>Define o índice exibido no dot (chamado uma vez no arranque).</summary>
    public void SetIndex(int index)
    {
        if (numberText != null)
            numberText.text = (index + 1).ToString();
    }
}
