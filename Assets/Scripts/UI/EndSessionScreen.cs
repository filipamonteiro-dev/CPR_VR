using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Ecrã de resultados exibido no final do treino ou teste.
///
/// Setup no Editor:
///   1. Cria um GameObject filho no Canvas do TrainingManager.
///   2. Adiciona este script.
///   3. Liga customFont no Inspector.
///   4. Arrasta para o campo endScreen do TrainingManager.
/// </summary>
[DisallowMultipleComponent]
public class EndSessionScreen : MonoBehaviour
{
    [Header("Tipografia")]
    [SerializeField] private TMP_FontAsset customFont;

    [Header("Navegação")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string trainingScene  = "TrainingScene";

    public UnityEvent onRestart;

    // ── Refs internas ─────────────────────────────────────────────────────
    private GameObject     overlay;
    private CanvasGroup    group;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI badgeText;
    private TextMeshProUGUI compressionsVal;
    private TextMeshProUGUI elapsedVal;
    private TextMeshProUGUI accuracyVal;
    private TextMeshProUGUI avgBpmVal;

    // ── Paleta ────────────────────────────────────────────────────────────
    private static readonly Color Bg          = new Color(0.02f, 0.03f, 0.06f, 0.96f);
    private static readonly Color PanelBg     = new Color(0.02f, 0.03f, 0.06f, 0.80f);
    private static readonly Color BorderMain  = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color Accent      = new Color(1f, 1f, 1f, 0.40f);
    private static readonly Color TextDim     = new Color(1f, 1f, 1f, 0.25f);
    private static readonly Color TextMed     = new Color(1f, 1f, 1f, 0.60f);
    private static readonly Color TextBright  = new Color(1f, 1f, 1f, 0.90f);
    private static readonly Color GreenAccent = new Color(0.55f, 1f, 0.60f, 0.85f);

    void Awake()
    {
        BuildScreen();
        overlay.SetActive(false);
    }

    // ── API pública ───────────────────────────────────────────────────────

    public void Show(int compressions, int elapsedSeconds, float accuracy, float avgBpm, bool isTestMode)
    {
        UpdateStats(compressions, elapsedSeconds, accuracy, avgBpm, isTestMode);
        overlay.SetActive(true);
        if (group != null) StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        overlay.SetActive(false);
    }

    // ── Construção ────────────────────────────────────────────────────────

    private void BuildScreen()
    {
        overlay = new GameObject("EndOverlay");
        overlay.transform.SetParent(transform, false);

        var rt = overlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        overlay.AddComponent<Image>().color = Bg;

        group = overlay.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        // Scanlines subtis
        MakeImg(overlay, new Color(1f, 1f, 1f, 0.012f), Vector2.zero, Vector2.zero, true);

        // Painel central
        BuildPanel(overlay);
    }

    private void BuildPanel(GameObject parent)
    {
        const float PW = 380f, PH = 520f;

        var panel = new GameObject("Panel");
        panel.transform.SetParent(parent.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(PW, PH);
        rt.anchoredPosition = Vector2.zero;

        var bg = panel.AddComponent<Image>();
        bg.color = PanelBg;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor    = BorderMain;
        outline.effectDistance = new Vector2(1f, -1f);

        // Cantos decorativos
        float hw = PW / 2f, hh = PH / 2f;
        AddCorner(panel, new Vector2(-hw, hh),   new Vector2(16f, 2f), new Vector2( 8f,-1f));
        AddCorner(panel, new Vector2(-hw, hh),   new Vector2(2f, 16f), new Vector2( 1f,-8f));
        AddCorner(panel, new Vector2( hw, hh),   new Vector2(16f, 2f), new Vector2(-8f,-1f));
        AddCorner(panel, new Vector2( hw, hh),   new Vector2(2f, 16f), new Vector2(-1f,-8f));
        AddCorner(panel, new Vector2(-hw,-hh),   new Vector2(16f, 2f), new Vector2( 8f, 1f));
        AddCorner(panel, new Vector2(-hw,-hh),   new Vector2(2f, 16f), new Vector2( 1f, 8f));
        AddCorner(panel, new Vector2( hw,-hh),   new Vector2(16f, 2f), new Vector2(-8f, 1f));
        AddCorner(panel, new Vector2( hw,-hh),   new Vector2(2f, 16f), new Vector2(-1f, 8f));

        // Cabeçalho
        MakeTMP(panel, "// SESSÃO CONCLUÍDA //", new Vector2(0f, 218f), new Vector2(320f, 18f),
            9f, TextDim, TextAlignmentOptions.Center, 5f);

        titleText = MakeTMP(panel, "RESULTADO", new Vector2(0f, 185f), new Vector2(340f, 44f),
            28f, TextBright, TextAlignmentOptions.Center, 3f);

        badgeText = MakeTMP(panel, "MODO TREINO", new Vector2(0f, 155f), new Vector2(200f, 20f),
            9f, new Color(1f,1f,1f,0.18f), TextAlignmentOptions.Center, 4f);

        // Separador
        MakeImg(panel, new Color(1f,1f,1f,0.08f), new Vector2(0f, 138f), new Vector2(320f, 1f));

        // Caixa de stats
        BuildStatsBox(panel, new Vector2(0f, 60f));

        // Separador 2
        MakeImg(panel, new Color(1f,1f,1f,0.06f), new Vector2(0f, -38f), new Vector2(320f, 1f));

        // Botões
        BuildBtn(panel, "REINICIAR SESSÃO",   "repetir com a mesma configuração",
            new Vector2(0f, -100f), false, () => onRestart?.Invoke());
        BuildBtn(panel, "MENU PRINCIPAL",     "voltar ao ecrã inicial",
            new Vector2(0f, -165f), false, () => SceneManager.LoadScene(mainMenuScene));
    }

    private void BuildStatsBox(GameObject parent, Vector2 center)
    {
        var box = new GameObject("Stats");
        box.transform.SetParent(parent.transform, false);
        var rt = box.AddComponent<RectTransform>();
        rt.anchoredPosition = center;
        rt.sizeDelta = new Vector2(330f, 160f);
        box.AddComponent<Image>().color = new Color(1f,1f,1f,0.025f);
        var ol = box.AddComponent<Outline>();
        ol.effectColor    = new Color(1f,1f,1f,0.07f);
        ol.effectDistance = new Vector2(1f,-1f);

        MakeTMP(box, "RESUMO DA SESSÃO", new Vector2(0f, 60f), new Vector2(300f, 16f),
            8f, TextDim, TextAlignmentOptions.Center, 4f);

        // Linha 1: compressões | tempo
        compressionsVal = StatCell(box, "—", "COMPRESSÕES", new Vector2(-80f,  18f));
        elapsedVal      = StatCell(box, "—", "TEMPO",       new Vector2( 80f,  18f));

        // Linha 2: precisão | bpm médio
        accuracyVal = StatCell(box, "—", "PRECISÃO",  new Vector2(-80f, -45f));
        avgBpmVal   = StatCell(box, "—", "BPM MÉDIO", new Vector2( 80f, -45f));
    }

    private TextMeshProUGUI StatCell(GameObject parent, string val, string label, Vector2 pos)
    {
        var valTmp = MakeTMP(parent, val,   pos,                        new Vector2(130f, 28f),
            18f, TextMed, TextAlignmentOptions.Center);
        MakeTMP(parent, label, pos + new Vector2(0f, -22f), new Vector2(130f, 14f),
            8f, TextDim, TextAlignmentOptions.Center, 2f);
        return valTmp;
    }

    private void BuildBtn(GameObject parent, string label, string desc, Vector2 pos, bool danger, System.Action onClick)
    {
        Color border = danger ? new Color(1f,0.35f,0.35f,0.40f) : new Color(1f,1f,1f,0.20f);
        Color text   = danger ? new Color(1f,0.39f,0.39f,0.75f) : TextMed;
        Color hover  = danger ? new Color(1f,0.35f,0.35f,0.07f) : new Color(1f,1f,1f,0.05f);

        var go = new GameObject($"Btn_{label}");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(320f, 48f);

        var hit = MakeImg(go, Color.clear, Vector2.zero, new Vector2(320f, 48f));
        var ol  = hit.gameObject.AddComponent<Outline>();
        ol.effectColor    = border;
        ol.effectDistance = new Vector2(1f,-1f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = hit;
        var c = btn.colors;
        c.normalColor      = Color.clear;
        c.highlightedColor = hover;
        c.pressedColor     = new Color(hover.r,hover.g,hover.b,hover.a*2f);
        c.selectedColor    = hover;
        btn.colors = c;
        if (onClick != null) btn.onClick.AddListener(() => onClick());

        MakeTMP(go, label, new Vector2(-10f,  9f), new Vector2(280f, 22f), 13f, text,  TextAlignmentOptions.Left, 2f);
        MakeTMP(go, desc,  new Vector2(-10f, -10f), new Vector2(280f, 16f),  9f, TextDim, TextAlignmentOptions.Left);
    }

    // ── Atualização dos valores ───────────────────────────────────────────

    private void UpdateStats(int compressions, int elapsedSeconds, float accuracy, float avgBpm, bool isTestMode)
    {
        if (titleText  != null) titleText.text  = accuracy >= 0.8f ? "EXCELENTE" : accuracy >= 0.5f ? "BOM TRABALHO" : "A MELHORAR";
        if (titleText  != null) titleText.color = accuracy >= 0.8f ? GreenAccent : TextBright;
        if (badgeText  != null) badgeText.text  = isTestMode ? "// MODO TESTE //" : "// MODO TREINO //";

        if (compressionsVal != null) compressionsVal.text = compressions.ToString();
        if (elapsedVal      != null) elapsedVal.text      = FormatTime(elapsedSeconds);
        if (accuracyVal     != null) accuracyVal.text     = $"{Mathf.RoundToInt(accuracy * 100f)}%";
        if (avgBpmVal       != null) avgBpmVal.text       = avgBpm > 0 ? $"{avgBpm:0}" : "—";

        // Colorir precisão
        if (accuracyVal != null)
            accuracyVal.color = accuracy >= 0.8f ? GreenAccent : accuracy >= 0.5f ? TextMed : new Color(1f,0.5f,0.5f,0.8f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private IEnumerator FadeIn()
    {
        float t = 0f, dur = 0.4f;
        while (t < dur) { t += Time.deltaTime; group.alpha = Mathf.Clamp01(t / dur); yield return null; }
        group.alpha = 1f;
    }

    private void AddCorner(GameObject parent, Vector2 pos, Vector2 size, Vector2 offset)
    {
        MakeImg(parent, Accent, pos + offset, size);
    }

    private Image MakeImg(GameObject parent, Color color, Vector2 pos, Vector2 size, bool stretch = false)
    {
        var go = new GameObject("I");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        if (stretch) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
        else         { rt.anchoredPosition = pos; rt.sizeDelta = size; }
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private TextMeshProUGUI MakeTMP(GameObject parent, string text, Vector2 pos, Vector2 size,
        float fs, Color color, TextAlignmentOptions align, float spacing = 0f)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text             = text;
        tmp.fontSize         = fs;
        tmp.color            = color;
        tmp.alignment        = align;
        tmp.characterSpacing = spacing;
        if (customFont != null) tmp.font = customFont;
        return tmp;
    }

    private string FormatTime(int s) => $"{s/60:D2}:{s%60:D2}";
}
