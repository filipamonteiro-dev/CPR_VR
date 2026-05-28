using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Painel HUD flutuante que mostra ao jogador a instrução do estado atual.
/// Liga-se ao StateMachine e usa StateLabel + GetSubtitles() de cada State.
///
/// Setup no Editor:
///   1. Cria um GameObject vazio na cena.
///   2. Adiciona este script.
///   3. Liga stateMachine, xrCamera e customFont no Inspector.
/// </summary>
[DisallowMultipleComponent]
public class StateInstructionHUD : MonoBehaviour
{
    [Header("Ligação")]
    [SerializeField] private StateMachine stateMachine;
    [SerializeField] private Camera xrCamera;

    [Header("Aparência")]
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] private float distanceFromPlayer = 1.5f;
    [SerializeField] private float verticalOffset     = -0.15f;

    // ── Referências dinâmicas ─────────────────────────────────────────────
    private TextMeshProUGUI labelText;
    private TextMeshProUGUI subtitleText;
    private TextMeshProUGUI stepCounterText;
    private RectTransform   progressFillRect;
    private CanvasGroup     group;
    private Coroutine       fadeRoutine;

    private const float PanelW  = 500f;
    private const float PanelH  = 220f;
    private const float BarW    = 440f;

    // ── Paleta ────────────────────────────────────────────────────────────
    private static readonly Color BgColor    = new Color(0.02f, 0.03f, 0.06f, 0.94f);
    private static readonly Color HeaderBg   = new Color(1f, 1f, 1f, 0.04f);
    private static readonly Color BorderMain = new Color(1f, 1f, 1f, 0.10f);
    private static readonly Color Accent     = new Color(1f, 1f, 1f, 0.45f);
    private static readonly Color FillColor  = new Color(1f, 1f, 1f, 0.38f);
    private static readonly Color TextDim    = new Color(1f, 1f, 1f, 0.22f);
    private static readonly Color TextMed    = new Color(1f, 1f, 1f, 0.50f);
    private static readonly Color TextBright = new Color(1f, 1f, 1f, 0.90f);

    // ── Ciclo de vida ─────────────────────────────────────────────────────
    void Awake()
    {
        BuildHUD();
        PositionHUD();
    }

    void LateUpdate()
    {
        if (xrCamera == null) return;

        // Calcula posição alvo e suaviza o seguimento da câmara
        Transform cam = xrCamera.transform;
        Vector3 fwd   = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;

        Vector3 targetPos = cam.position + fwd * distanceFromPlayer + Vector3.up * verticalOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 3f);

        Quaternion targetRot = Quaternion.LookRotation(transform.position - cam.position);
        transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 3f);
    }

    void Start()
    {
        if (stateMachine == null) return;

        foreach (var state in stateMachine.StatesToExecute)
            state.OnEnter += OnStateEntered;
    }

    void OnDestroy()
    {
        if (stateMachine == null) return;

        foreach (var state in stateMachine.StatesToExecute)
            state.OnEnter -= OnStateEntered;
    }

    // ── Resposta aos estados ──────────────────────────────────────────────
    private void OnStateEntered(State state)
    {
        string lbl = state.StateLabel;
        string sub = state.GetSubtitles();

        if (string.IsNullOrWhiteSpace(lbl) && string.IsNullOrWhiteSpace(sub))
        {
            FadeOut();
            return;
        }

        int idx   = stateMachine.CurrentStateIndex;
        int total = stateMachine.StatesToExecute.Count;

        if (labelText        != null) labelText.text       = (lbl ?? "").ToUpper();
        if (subtitleText     != null) subtitleText.text    = sub ?? "";
        if (stepCounterText  != null) stepCounterText.text = $"{idx + 1} / {total}";

        if (progressFillRect != null)
            progressFillRect.sizeDelta = new Vector2(BarW * ((float)(idx + 1) / Mathf.Max(1, total)), 3f);

        FadeIn();
    }

    // ── Fade ──────────────────────────────────────────────────────────────
    private void FadeIn()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(DoFade(1f, 0.35f));
    }

    private void FadeOut()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(DoFade(0f, 0.25f));
    }

    private IEnumerator DoFade(float target, float dur)
    {
        if (group == null) yield break;
        float start = group.alpha, t = 0f;
        while (t < dur)
        {
            t           += Time.deltaTime;
            group.alpha  = Mathf.Lerp(start, target, Mathf.Clamp01(t / dur));
            yield return null;
        }
        group.alpha = target;
    }

    // ── Posicionamento ────────────────────────────────────────────────────
    private void PositionHUD()
    {
        if (xrCamera == null) xrCamera = Camera.main;
        if (xrCamera == null) return;

        Transform cam = xrCamera.transform;
        Vector3   fwd = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;

        transform.position = cam.position + fwd * distanceFromPlayer + Vector3.up * verticalOffset;
        transform.LookAt(cam.position);
        transform.Rotate(0f, 180f, 0f);
    }

    // ── Construção do HUD ─────────────────────────────────────────────────
    private void BuildHUD()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.worldCamera = xrCamera;
        gameObject.AddComponent<CanvasScaler>();

        GetComponent<RectTransform>().sizeDelta = new Vector2(PanelW, PanelH);
        transform.localScale = Vector3.one * 0.001f;

        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha          = 0f;
        group.blocksRaycasts = false;

        // Fundo
        MakeImg(gameObject, BgColor, Vector2.zero, new Vector2(PanelW, PanelH));
        BuildGrid(gameObject);

        // Cantos decorativos
        float hw = PanelW / 2f + 4f, hh = PanelH / 2f + 4f;
        (Vector2 p, Vector2 sz, Vector2 off)[] corners =
        {
            (new Vector2(-hw, hh),  new Vector2(14f,  2f), new Vector2( 7f,-1f)),
            (new Vector2(-hw, hh),  new Vector2( 2f, 14f), new Vector2( 1f,-7f)),
            (new Vector2( hw, hh),  new Vector2(14f,  2f), new Vector2(-7f,-1f)),
            (new Vector2( hw, hh),  new Vector2( 2f, 14f), new Vector2(-1f,-7f)),
            (new Vector2(-hw,-hh),  new Vector2(14f,  2f), new Vector2( 7f, 1f)),
            (new Vector2(-hw,-hh),  new Vector2( 2f, 14f), new Vector2( 1f, 7f)),
            (new Vector2( hw,-hh),  new Vector2(14f,  2f), new Vector2(-7f, 1f)),
            (new Vector2( hw,-hh),  new Vector2( 2f, 14f), new Vector2(-1f, 7f)),
        };
        foreach (var c in corners) MakeImg(gameObject, Accent, c.p + c.off, c.sz);

        // Zona de cabeçalho
        MakeImg(gameObject, HeaderBg,   new Vector2(0f, 83f), new Vector2(PanelW, 54f));
        MakeImg(gameObject, BorderMain, new Vector2(0f, 56f), new Vector2(PanelW,  1f));

        MakeTMP(gameObject, "// INSTRUÇÃO ATUAL //",
            new Vector2(-160f, 84f), new Vector2(220f, 16f),
            9f, TextDim, TextAlignmentOptions.Left, 5f);

        stepCounterText = MakeTMP(gameObject, "— / —",
            new Vector2(200f, 84f), new Vector2(80f, 16f),
            10f, TextMed, TextAlignmentOptions.Right, 2f);

        // Barra de progresso
        MakeImg(gameObject, new Color(1f, 1f, 1f, 0.06f), new Vector2(0f, 66f), new Vector2(BarW, 3f));

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(transform, false);
        progressFillRect = fillGO.AddComponent<RectTransform>();
        progressFillRect.pivot          = new Vector2(0f, 0.5f);
        progressFillRect.anchorMin      = progressFillRect.anchorMax = new Vector2(0.5f, 0.5f);
        progressFillRect.anchoredPosition = new Vector2(-BarW / 2f, 66f);
        progressFillRect.sizeDelta      = new Vector2(0f, 3f);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = FillColor;

        // Label do estado (destaque)
        labelText = MakeTMP(gameObject, "AGUARDANDO...",
            new Vector2(0f, 18f), new Vector2(460f, 36f),
            20f, TextBright, TextAlignmentOptions.Left, 1.5f);

        // Texto de instrução
        subtitleText = MakeTMP(gameObject, "",
            new Vector2(0f, -30f), new Vector2(460f, 68f),
            11f, TextMed, TextAlignmentOptions.TopLeft);
        if (subtitleText != null)
        {
            subtitleText.lineSpacing    = 16f;
            subtitleText.enableWordWrapping = true;
        }
    }

    private void BuildGrid(GameObject root)
    {
        int cols = 8, rows = 4;
        for (int c = 1; c < cols; c++)
            MakeImg(root, new Color(1f, 1f, 1f, 0.02f),
                new Vector2(-PanelW / 2f + c * (PanelW / cols), 0f), new Vector2(1f, PanelH));
        for (int r = 1; r < rows; r++)
            MakeImg(root, new Color(1f, 1f, 1f, 0.02f),
                new Vector2(0f, -PanelH / 2f + r * (PanelH / rows)), new Vector2(PanelW, 1f));
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private Image MakeImg(GameObject parent, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("I");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private TextMeshProUGUI MakeTMP(GameObject parent, string text, Vector2 pos, Vector2 size,
        float fs, Color color,
        TextAlignmentOptions align   = TextAlignmentOptions.Left,
        float spacing = 0f)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text                = text;
        tmp.fontSize            = fs;
        tmp.color               = color;
        tmp.alignment           = align;
        tmp.characterSpacing    = spacing;
        tmp.enableWordWrapping  = true;
        if (customFont != null) tmp.font = customFont;
        return tmp;
    }
}
