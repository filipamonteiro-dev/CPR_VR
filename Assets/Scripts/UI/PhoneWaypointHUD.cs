using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD flutuante que mostra uma seta a apontar para o telefone enquanto
/// ele não está no campo de visão do jogador.
///
/// Setup no Editor:
///   1. Cria um GameObject vazio na cena.
///   2. Adiciona este script.
///   3. Liga phoneTransform, xrCamera e customFont no Inspector.
///   4. Chama SetActive(true) quando o Call112State entrar e SetActive(false) ao sair.
/// </summary>
[DisallowMultipleComponent]
public class PhoneWaypointHUD : MonoBehaviour
{
    [Header("Ligação")]
    [SerializeField] private Transform phoneTransform;
    [SerializeField] private Camera    xrCamera;
    [SerializeField] private TMP_FontAsset customFont;

    [Header("Comportamento")]
    [SerializeField] private float distanceFromCamera = 1.4f;
    [SerializeField] private float verticalOffset     = -0.05f;
    [SerializeField] private float hideWhenCloserThan = 1.2f;   // metros
    [SerializeField] private float hideWhenAngleLess  = 25f;    // graus (dentro do FOV)

    // ── Refs internas ─────────────────────────────────────────────────────
    private RectTransform arrowRect;
    private TextMeshProUGUI distanceText;
    private CanvasGroup     group;
    private float           targetAlpha;

    private static readonly Color PanelBg  = new Color(0.02f, 0.03f, 0.06f, 0.90f);
    private static readonly Color Accent   = new Color(1f, 1f, 1f, 0.50f);
    private static readonly Color TextDim  = new Color(1f, 1f, 1f, 0.28f);
    private static readonly Color TextMed  = new Color(1f, 1f, 1f, 0.70f);

    void Awake()
    {
        BuildHUD();
        group.alpha = 0f;
    }

    void LateUpdate()
    {
        if (xrCamera == null) xrCamera = Camera.main;
        if (xrCamera == null || phoneTransform == null) return;

        UpdateArrow();
        UpdatePosition();
        group.alpha = Mathf.Lerp(group.alpha, targetAlpha, Time.deltaTime * 8f);
    }

    // ── Lógica principal ──────────────────────────────────────────────────

    private void UpdateArrow()
    {
        Transform cam   = xrCamera.transform;
        Vector3   toPhone = phoneTransform.position - cam.position;
        float     dist    = toPhone.magnitude;

        // Dentro do raio mínimo → esconde
        if (dist < hideWhenCloserThan)
        {
            targetAlpha = 0f;
            return;
        }

        // Dentro do campo de visão → esconde
        float angle = Vector3.Angle(cam.forward, toPhone.normalized);
        if (angle < hideWhenAngleLess)
        {
            targetAlpha = 0f;
            return;
        }

        targetAlpha = 1f;

        // Projetar direção no plano da câmara para obter ângulo 2D da seta
        float x   = Vector3.Dot(toPhone, cam.right);
        float y   = Vector3.Dot(toPhone, cam.up);
        float deg = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        // arrow aponta para cima por defeito → -90°
        if (arrowRect != null)
            arrowRect.localRotation = Quaternion.Euler(0f, 0f, deg - 90f);

        if (distanceText != null)
            distanceText.text = $"{dist:0.0} m";
    }

    private void UpdatePosition()
    {
        Transform cam = xrCamera.transform;
        Vector3   fwd = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;

        transform.position = cam.position + fwd * distanceFromCamera + Vector3.up * verticalOffset;
        transform.LookAt(cam.position);
        transform.Rotate(0f, 180f, 0f);
    }

    // ── Construção ────────────────────────────────────────────────────────

    private void BuildHUD()
    {
        const float W = 110f, H = 110f;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.worldCamera = xrCamera;
        gameObject.AddComponent<CanvasScaler>();

        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(W, H);
        transform.localScale = Vector3.one * 0.001f;

        group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        // Fundo circular (simulado com painel quadrado)
        var bg = MakeImg(gameObject, PanelBg, Vector2.zero, new Vector2(W, H));
        var outline = bg.gameObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor    = new Color(1f, 1f, 1f, 0.18f);
        outline.effectDistance = new Vector2(1f, -1f);

        // Cantos decorativos
        float hw = W / 2f + 3f, hh = H / 2f + 3f;
        MakeImg(gameObject, Accent, new Vector2(-hw + 6f, hh),   new Vector2(12f, 1.5f));
        MakeImg(gameObject, Accent, new Vector2(-hw,  hh - 6f),  new Vector2(1.5f, 12f));
        MakeImg(gameObject, Accent, new Vector2( hw - 6f, hh),   new Vector2(12f, 1.5f));
        MakeImg(gameObject, Accent, new Vector2( hw,  hh - 6f),  new Vector2(1.5f, 12f));
        MakeImg(gameObject, Accent, new Vector2(-hw + 6f, -hh),  new Vector2(12f, 1.5f));
        MakeImg(gameObject, Accent, new Vector2(-hw,  -hh + 6f), new Vector2(1.5f, 12f));
        MakeImg(gameObject, Accent, new Vector2( hw - 6f, -hh),  new Vector2(12f, 1.5f));
        MakeImg(gameObject, Accent, new Vector2( hw,  -hh + 6f), new Vector2(1.5f, 12f));

        // Label topo
        MakeTMP(gameObject, "TELEFONE", new Vector2(0f, 40f), new Vector2(100f, 14f),
            8f, TextDim, TextAlignmentOptions.Center, 4f);

        // Seta (▲ em TMP, rodada em LateUpdate)
        var arrowGO = new GameObject("Arrow");
        arrowGO.transform.SetParent(transform, false);
        arrowRect = arrowGO.AddComponent<RectTransform>();
        arrowRect.anchoredPosition = new Vector2(0f, 5f);
        arrowRect.sizeDelta        = new Vector2(36f, 36f);
        var arrowTmp = arrowGO.AddComponent<TextMeshProUGUI>();
        arrowTmp.text      = "▲";
        arrowTmp.fontSize  = 26f;
        arrowTmp.color     = new Color(1f, 1f, 1f, 0.85f);
        arrowTmp.alignment = TextAlignmentOptions.Center;
        if (customFont != null) arrowTmp.font = customFont;

        // Distância
        distanceText = MakeTMP(gameObject, "— m", new Vector2(0f, -36f), new Vector2(100f, 14f),
            9f, TextMed, TextAlignmentOptions.Center, 1f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private UnityEngine.UI.Image MakeImg(GameObject parent, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("I");
        go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        r.anchoredPosition = pos;
        r.sizeDelta        = size;
        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        return img;
    }

    private TextMeshProUGUI MakeTMP(GameObject parent, string text, Vector2 pos, Vector2 size,
        float fs, Color color, TextAlignmentOptions align, float spacing = 0f)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        r.anchoredPosition = pos;
        r.sizeDelta        = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text             = text;
        tmp.fontSize         = fs;
        tmp.color            = color;
        tmp.alignment        = align;
        tmp.characterSpacing = spacing;
        if (customFont != null) tmp.font = customFont;
        return tmp;
    }
}
