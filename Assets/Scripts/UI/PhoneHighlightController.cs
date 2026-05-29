using UnityEngine;

/// <summary>
/// Pulsa a emissão do material do telefone enquanto está activo,
/// indicando ao jogador que deve interagir com ele.
/// Para quando o jogador marca 1-1-2.
///
/// Setup no Editor:
///   1. Adiciona este script ao GameObject do telefone (ou a um pai).
///   2. Liga phoneRenderer (o Renderer do modelo 3D) e phoneDialer no Inspector.
///   3. Activa/desactiva o GameObject para ligar/desligar o efeito.
///
/// Nota: o material do telefone precisa de ter a keyword "_EMISSION" activa
///       (no Standard Shader: Emission checkbox marcada).
/// </summary>
[DisallowMultipleComponent]
public class PhoneHighlightController : MonoBehaviour
{
    [Header("Ligação")]
    [SerializeField] private Renderer   phoneRenderer;
    [SerializeField] private PhoneDialer phoneDialer;

    [Header("Glow")]
    [SerializeField] private Color  glowColor     = new Color(0.4f, 0.8f, 1f);
    [SerializeField] private float  glowIntensity = 1.2f;   // multiplicador HDR
    [SerializeField] private float  pulseSpeed    = 1.8f;   // Hz
    [SerializeField] private float  pulseMin      = 0.1f;   // alpha mínimo do glow
    [SerializeField] private float  pulseMax      = 1.0f;   // alpha máximo do glow

    private Material  runtimeMat;
    private bool      active = false;
    private float     phase  = 0f;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void OnEnable()
    {
        if (phoneRenderer != null)
        {
            // Cria instância do material para não alterar o asset original
            runtimeMat = phoneRenderer.material;
            runtimeMat.EnableKeyword("_EMISSION");
        }

        if (phoneDialer != null)
            phoneDialer.EmergencyNumberDialed += OnDialComplete;

        active = true;
        phase  = 0f;
    }

    void OnDisable()
    {
        if (phoneDialer != null)
            phoneDialer.EmergencyNumberDialed -= OnDialComplete;

        StopGlow();
    }

    void Update()
    {
        if (!active || runtimeMat == null) return;

        phase += Time.deltaTime * pulseSpeed * Mathf.PI * 2f;
        float t         = (Mathf.Sin(phase) + 1f) * 0.5f;             // 0..1
        float intensity = Mathf.Lerp(pulseMin, pulseMax, t) * glowIntensity;
        runtimeMat.SetColor(EmissionColorId, glowColor * intensity);
    }

    private void OnDialComplete(PhoneDialer dialer)
    {
        active = false;
        StopGlow();
    }

    private void StopGlow()
    {
        if (runtimeMat != null)
            runtimeMat.SetColor(EmissionColorId, Color.black);
    }
}
