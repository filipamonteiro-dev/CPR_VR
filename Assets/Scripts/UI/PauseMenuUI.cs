using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Overlay de pausa seguindo a wireframe React (PauseMenu.tsx).
/// Constrói o painel em código — coloque este componente num GameObject filho
/// do Canvas do TrainingManager e chame Show() / Hide() conforme necessário.
/// </summary>
[DisallowMultipleComponent]
public class PauseMenuUI : MonoBehaviour
{
    [Header("Tipografia")]
    [SerializeField] private TMP_FontAsset font;

    [Header("Eventos")]
    public UnityEvent onResume;
    public UnityEvent onRestart;
    public UnityEvent onQuitToMenu;

    // Referências internas ao painel
    private GameObject overlay;
    private TextMeshProUGUI compressionsStatText;
    private TextMeshProUGUI elapsedStatText;
    private TextMeshProUGUI accuracyStatText;

    // Paleta (replicada do React)
    private static readonly Color bg          = new Color(0.02f, 0.03f, 0.06f, 0.82f);
    private static readonly Color panelBg     = new Color(0.02f, 0.03f, 0.06f, 0.60f);
    private static readonly Color borderSolid = new Color(1f, 1f, 1f, 0.10f);
    private static readonly Color borderDash  = new Color(1f, 1f, 1f, 0.20f);
    private static readonly Color borderDanger= new Color(1f, 0.35f, 0.35f, 0.40f);
    private static readonly Color borderAccent= new Color(1f, 1f, 1f, 0.50f);

    private static readonly Color textDim    = new Color(1f, 1f, 1f, 0.25f);
    private static readonly Color textMedium = new Color(1f, 1f, 1f, 0.60f);
    private static readonly Color textBright = new Color(1f, 1f, 1f, 0.88f);
    private static readonly Color textDanger = new Color(1f, 0.39f, 0.39f, 0.75f);
    private static readonly Color textAccent = new Color(1f, 1f, 1f, 0.90f);

    void Awake()
    {
        BuildOverlay();
        Hide();
    }

    // ── API pública ──────────────────────────────────────────────────────

    public void Show(int compressions, int elapsedSeconds, float accuracy)
    {
        if (compressionsStatText != null) compressionsStatText.text = compressions.ToString();
        if (elapsedStatText      != null) elapsedStatText.text      = FormatTime(elapsedSeconds);
        if (accuracyStatText     != null) accuracyStatText.text     = $"{Mathf.RoundToInt(accuracy * 100f)}%";
        overlay.SetActive(true);
    }

    public void Hide()
    {
        if (overlay != null) overlay.SetActive(false);
    }

    // ── Construção ───────────────────────────────────────────────────────

    private void BuildOverlay()
    {
        // Fundo semi-transparente full-screen
        overlay = new GameObject("PauseOverlay");
        overlay.transform.SetParent(transform, false);
        var overlayRt = overlay.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = bg;

        // Scanlines
        BuildScanlines(overlay);

        // Painel central (320x auto)
        var panel = BuildPanel(overlay);
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = new Vector2(320f, 480f);
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot     = new Vector2(0.5f, 0.5f);
    }

    private void BuildScanlines(GameObject parent)
    {
        // Thin horizontal line pattern simulated with a low-alpha image
        var go = new GameObject("Scanlines");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.012f);
    }

    private GameObject BuildPanel(GameObject parent)
    {
        var panel = new GameObject("PausePanel");
        panel.transform.SetParent(parent.transform, false);
        panel.AddComponent<RectTransform>();

        // Borda exterior
        var outerBorder = panel.AddComponent<Image>();
        outerBorder.color = borderSolid;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor    = borderSolid;
        outline.effectDistance = new Vector2(1f, -1f);

        // Fundo interior
        var inner = new GameObject("Inner");
        inner.transform.SetParent(panel.transform, false);
        var innerRt = inner.AddComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(2f, 2f);
        innerRt.offsetMax = new Vector2(-2f, -2f);
        var innerImg = inner.AddComponent<Image>();
        innerImg.color = panelBg;

        // Cantos decorativos
        AddCornerAccent(panel, new Vector2(-150f, 230f), new Vector2(12f, 2f), new Vector2(6f, -1f), new Color(1f,1f,1f,0.30f));
        AddCornerAccent(panel, new Vector2(-150f, 230f), new Vector2(2f, 12f), new Vector2(1f, -6f), new Color(1f,1f,1f,0.30f));
        AddCornerAccent(panel, new Vector2(150f, 230f),  new Vector2(12f, 2f), new Vector2(-6f, -1f),new Color(1f,1f,1f,0.30f));
        AddCornerAccent(panel, new Vector2(150f, 230f),  new Vector2(2f, 12f), new Vector2(-1f, -6f),new Color(1f,1f,1f,0.30f));
        AddCornerAccent(panel, new Vector2(-150f,-230f), new Vector2(12f, 2f), new Vector2(6f, 1f),  new Color(1f,1f,1f,0.30f));
        AddCornerAccent(panel, new Vector2(-150f,-230f), new Vector2(2f, 12f), new Vector2(1f, 6f),  new Color(1f,1f,1f,0.30f));
        AddCornerAccent(panel, new Vector2(150f, -230f), new Vector2(12f, 2f), new Vector2(-6f, 1f), new Color(1f,1f,1f,0.30f));
        AddCornerAccent(panel, new Vector2(150f, -230f), new Vector2(2f, 12f), new Vector2(-1f, 6f), new Color(1f,1f,1f,0.30f));

        BuildPanelContent(panel);
        return panel;
    }

    private void BuildPanelContent(GameObject panel)
    {
        // ── Cabeçalho ─────────────────────────────────────────────────────
        MakeTMP(panel, "// SISTEMA //",       new Vector2(0, 195), new Vector2(260, 20), 10f, textDim, TextAlignmentOptions.Center, 5f);
        MakeTMP(panel, "PAUSADO",             new Vector2(0, 165), new Vector2(260, 40), 24f, textBright, TextAlignmentOptions.Center, 3f);

        // Barras de pausa animadas (estáticas — animação seria Animator no prefab)
        AddImage(panel, new Color(1f,1f,1f,0.30f), new Vector2(-8f, 130f), new Vector2(6f, 20f));
        AddImage(panel, new Color(1f,1f,1f,0.30f), new Vector2( 8f, 130f), new Vector2(6f, 20f));

        // Separador
        AddImage(panel, new Color(1f,1f,1f,0.08f), new Vector2(0, 105f), new Vector2(280f, 1f));

        // ── Resumo da sessão ──────────────────────────────────────────────
        var statsBox = BuildStatsBox(panel, new Vector2(0, 55f));

        // ── Botões ────────────────────────────────────────────────────────
        BuildPauseButton(panel, "RETOMAR",        "continuar sessão atual",             new Vector2(0, -80f),  "accent",  () => onResume?.Invoke());
        BuildPauseButton(panel, "REINICIAR",      "reiniciar sessão · manter config.",  new Vector2(0, -135f), "default", () => onRestart?.Invoke());
        BuildPauseButton(panel, "SAIR PARA MENU", "voltar ao menu principal",           new Vector2(0, -190f), "danger",  () => onQuitToMenu?.Invoke());
    }

    private GameObject BuildStatsBox(GameObject parent, Vector2 pos)
    {
        var box = new GameObject("StatsBox");
        box.transform.SetParent(parent.transform, false);
        var rt = box.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(280f, 80f);

        var bg = box.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.025f);
        var outline = box.AddComponent<Outline>();
        outline.effectColor    = new Color(1f, 1f, 1f, 0.10f);
        outline.effectDistance = new Vector2(1f, -1f);

        MakeTMP(box, "RESUMO DA SESSÃO", new Vector2(0, 30f), new Vector2(260f, 16f), 9f, textDim, TextAlignmentOptions.Center, 4f);

        // 3 estatísticas lado a lado
        compressionsStatText = MakeTMP(box, "—", new Vector2(-90f, 0f), new Vector2(80f, 22f), 14f, textMedium, TextAlignmentOptions.Center);
        MakeTMP(box, "COMPRESSÕES", new Vector2(-90f, -18f), new Vector2(80f, 14f), 8f, textDim, TextAlignmentOptions.Center);

        elapsedStatText = MakeTMP(box, "—", new Vector2(0f, 0f), new Vector2(80f, 22f), 14f, textMedium, TextAlignmentOptions.Center);
        MakeTMP(box, "DECORRIDO", new Vector2(0f, -18f), new Vector2(80f, 14f), 8f, textDim, TextAlignmentOptions.Center);

        accuracyStatText = MakeTMP(box, "—", new Vector2(90f, 0f), new Vector2(80f, 22f), 14f, textMedium, TextAlignmentOptions.Center);
        MakeTMP(box, "PRECISÃO", new Vector2(90f, -18f), new Vector2(80f, 14f), 8f, textDim, TextAlignmentOptions.Center);

        return box;
    }

    private void BuildPauseButton(GameObject parent, string label, string desc, Vector2 pos, string variant, Action onClick)
    {
        Color borderCol = variant == "accent"  ? borderAccent
                        : variant == "danger"  ? borderDanger
                                               : borderDash;
        Color textCol   = variant == "accent"  ? textAccent
                        : variant == "danger"  ? textDanger
                                               : textMedium;
        Color hoverCol  = variant == "accent"  ? new Color(1f,1f,1f,0.08f)
                        : variant == "danger"  ? new Color(1f,0.35f,0.35f,0.07f)
                                               : new Color(1f,1f,1f,0.04f);

        var btnGO = new GameObject($"Btn_{label}");
        btnGO.transform.SetParent(parent.transform, false);
        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(280f, 46f);

        var hitbox = AddImage(btnGO, Color.clear, Vector2.zero, new Vector2(280f, 46f));
        var outline = hitbox.gameObject.AddComponent<Outline>();
        outline.effectColor    = borderCol;
        outline.effectDistance = new Vector2(1f, -1f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = hitbox;
        var colors = btn.colors;
        colors.normalColor      = Color.clear;
        colors.highlightedColor = hoverCol;
        colors.pressedColor     = new Color(hoverCol.r, hoverCol.g, hoverCol.b, hoverCol.a * 2f);
        colors.selectedColor    = hoverCol;
        btn.colors = colors;

        if (onClick != null) btn.onClick.AddListener(() => onClick());

        MakeTMP(btnGO, label, new Vector2(-10f, 8f),  new Vector2(240f, 22f), 13f, textCol, TextAlignmentOptions.Left, 2f);
        MakeTMP(btnGO, desc,  new Vector2(-10f, -10f), new Vector2(240f, 16f), 9f,  textDim, TextAlignmentOptions.Left);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private Image AddImage(GameObject parent, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Image");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private TextMeshProUGUI MakeTMP(GameObject parent, string text, Vector2 pos, Vector2 size, float fs, Color color,
        TextAlignmentOptions align = TextAlignmentOptions.Center, float charSpacing = 0f)
    {
        var go = new GameObject($"Text_{text[..Mathf.Min(6, text.Length)]}");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fs;
        tmp.color = color;
        tmp.alignment = align;
        tmp.characterSpacing = charSpacing;
        if (font != null) tmp.font = font;
        return tmp;
    }

    private void AddCornerAccent(GameObject parent, Vector2 pos, Vector2 size, Vector2 offset, Color color)
    {
        AddImage(parent, color, pos + offset, size);
    }

    private string FormatTime(int seconds)
    {
        int m   = seconds / 60;
        int sec = seconds % 60;
        return $"{m:D2}:{sec:D2}";
    }
}
