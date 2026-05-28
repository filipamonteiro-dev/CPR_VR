using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Whiteboard de tutorial que flutua no mundo VR.
/// Mostra a lista de passos à esquerda e a instrução atual à direita.
///
/// Configuração mínima no Editor:
///   1. Adiciona este componente a um GameObject na cena.
///   2. Atribui a camera XR em xrCamera.
///   3. Liga TutorialManagerUI.whiteboard → este componente.
///   4. (Opcional) Atribui a fonte TMP em customFont.
/// </summary>
[DisallowMultipleComponent]
public class TutorialWhiteboardUI : MonoBehaviour
{
    [Header("Configuração")]
    public Camera         xrCamera;
    public TMP_FontAsset  customFont;
    [Tooltip("Distância à frente da câmara (metros)")]
    public float          distanceFromPlayer = 1.8f;
    [Tooltip("Deslocamento vertical relativo à câmara (metros)")]
    public float          verticalOffset     = 0f;

    // ── Paleta (wireframe) ───────────────────────────────────────────────
    private static readonly Color bgMain     = new Color(0.02f, 0.03f, 0.06f, 0.96f);
    private static readonly Color bgPanel    = new Color(0.02f, 0.03f, 0.06f, 0.60f);
    private static readonly Color bgActive   = new Color(1f, 1f, 1f, 0.06f);
    private static readonly Color borderMain = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color borderMed  = new Color(1f, 1f, 1f, 0.18f);
    private static readonly Color borderDash = new Color(1f, 1f, 1f, 0.10f);
    private static readonly Color accent     = new Color(1f, 1f, 1f, 0.55f);

    private static readonly Color textDim    = new Color(1f, 1f, 1f, 0.22f);
    private static readonly Color textMed    = new Color(1f, 1f, 1f, 0.45f);
    private static readonly Color textBright = new Color(1f, 1f, 1f, 0.88f);
    private static readonly Color textFaint  = new Color(1f, 1f, 1f, 0.15f);

    // ── Estado interno ────────────────────────────────────────────────────
    private TutorialStep[]   steps;
    private int              currentStep = 0;

    // Referências para atualizar dinamicamente
    private GameObject[]     stepRows;          // uma row por passo
    private TextMeshProUGUI[] stepNumberTexts;
    private TextMeshProUGUI[] stepLabelTexts;
    private Image[]           stepRowBgs;
    private TextMeshProUGUI  bigStepNumber;
    private TextMeshProUGUI  instructionTitle;
    private TextMeshProUGUI  instructionDesc;
    private TextMeshProUGUI  stepCounter;

    // ── Ciclo de vida ────────────────────────────────────────────────────
    void Awake()
    {
        steps = TutorialStepsCatalog.All;
        BuildBoard();
        PositionBoard();
        ApplyStep(0);
    }

    // ── API pública ──────────────────────────────────────────────────────
    public void SetStep(int index)
    {
        if (index < 0 || index >= steps.Length) return;
        currentStep = index;
        ApplyStep(index);
    }

    // ── Atualizar conteúdo ───────────────────────────────────────────────
    private void ApplyStep(int index)
    {
        TutorialStep step = steps[index];

        // Contador de etapa
        if (stepCounter != null)
            stepCounter.text = $"ETAPA  {index + 1} / {steps.Length}";

        // Número grande
        if (bigStepNumber != null)
            bigStepNumber.text = $"0{step.id}";

        // Instrução
        if (instructionTitle != null) instructionTitle.text = step.title;
        if (instructionDesc  != null) instructionDesc.text  = step.instruction;

        // Lista de passos — destaque da row ativa
        for (int i = 0; i < stepRows.Length; i++)
        {
            bool active   = i == index;
            bool complete = i < index;

            if (stepRowBgs[i]      != null) stepRowBgs[i].color  = active ? bgActive : Color.clear;
            if (stepNumberTexts[i] != null) stepNumberTexts[i].color = active  ? textBright
                                                                      : complete ? textMed
                                                                                 : textFaint;
            if (stepLabelTexts[i]  != null) stepLabelTexts[i].color  = active  ? textBright
                                                                      : complete ? textMed
                                                                                 : textDim;
        }
    }

    // ── Posicionamento no mundo ──────────────────────────────────────────
    private void PositionBoard()
    {
        if (xrCamera == null) xrCamera = Camera.main;
        if (xrCamera == null) return;

        Transform cam = xrCamera.transform;
        Vector3   fwd = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        transform.position = cam.position + fwd * distanceFromPlayer
                           + Vector3.up * verticalOffset;
        transform.LookAt(cam.position);
        transform.Rotate(0f, 180f, 0f);
    }

    // ── Construção do board ──────────────────────────────────────────────
    private void BuildBoard()
    {
        // Canvas World Space
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.worldCamera = xrCamera;

        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();
        gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();

        var rt = GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(820f, 500f);
        transform.localScale = Vector3.one * 0.001f;

        // ── Fundo geral ──────────────────────────────────────────────────
        var bg = MakeImage(gameObject, bgMain, Vector2.zero, new Vector2(820f, 500f));

        // Grid subtil de fundo
        BuildBackgroundGrid(gameObject);

        // Cantos decorativos do board
        AddCorner(gameObject, new Vector2(-406f,  246f), new Vector2(16f, 2f),  new Vector2( 8f,-1f), accent);
        AddCorner(gameObject, new Vector2(-406f,  246f), new Vector2(2f,  16f), new Vector2( 1f,-8f), accent);
        AddCorner(gameObject, new Vector2( 406f,  246f), new Vector2(16f, 2f),  new Vector2(-8f,-1f), accent);
        AddCorner(gameObject, new Vector2( 406f,  246f), new Vector2(2f,  16f), new Vector2(-1f,-8f), accent);
        AddCorner(gameObject, new Vector2(-406f, -246f), new Vector2(16f, 2f),  new Vector2( 8f, 1f), accent);
        AddCorner(gameObject, new Vector2(-406f, -246f), new Vector2(2f,  16f), new Vector2( 1f, 8f), accent);
        AddCorner(gameObject, new Vector2( 406f, -246f), new Vector2(16f, 2f),  new Vector2(-8f, 1f), accent);
        AddCorner(gameObject, new Vector2( 406f, -246f), new Vector2(2f,  16f), new Vector2(-1f, 8f), accent);

        // ── Header ───────────────────────────────────────────────────────
        BuildHeader(gameObject);

        // Separador horizontal header
        MakeImage(gameObject, borderMain, new Vector2(0f, 212f), new Vector2(820f, 1f));

        // ── Divisor vertical (separador esquerda/direita) ─────────────────
        MakeImage(gameObject, borderDash, new Vector2(-185f, -14f), new Vector2(1f, 426f));

        // ── Painel esquerdo — lista de passos ─────────────────────────────
        BuildStepList(gameObject);

        // ── Painel direito — instrução ────────────────────────────────────
        BuildInstructionPanel(gameObject);
    }

    // ── Header ──────────────────────────────────────────────────────────
    private void BuildHeader(GameObject root)
    {
        var header = new GameObject("Header");
        header.transform.SetParent(root.transform, false);
        var rt = header.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, 235f);
        rt.sizeDelta        = new Vector2(780f, 44f);

        // Etiqueta módulo
        MakeTMP(header, "// MÓDULO TUTORIAL //",
            new Vector2(-290f, 8f),  new Vector2(220f, 16f), 9f, textDim, align: TextAlignmentOptions.Left, spacing: 5f);

        // Título
        MakeTMP(header, "PROCEDIMENTO DE RCP",
            new Vector2(-290f, -10f), new Vector2(280f, 20f), 14f, textBright, align: TextAlignmentOptions.Left, spacing: 2f);

        // Contador de etapa (atualizado dinamicamente)
        stepCounter = MakeTMP(header, "ETAPA  1 / 6",
            new Vector2(280f, 0f), new Vector2(180f, 20f), 11f, textMed, align: TextAlignmentOptions.Right, spacing: 2f);
    }

    // ── Lista de passos (esquerda) ───────────────────────────────────────
    private void BuildStepList(GameObject root)
    {
        var panel = new GameObject("StepList");
        panel.transform.SetParent(root.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(-295f, -14f);
        rt.sizeDelta        = new Vector2(228f, 420f);

        // Título da coluna
        MakeTMP(panel, "PASSOS DO PROCEDIMENTO",
            new Vector2(0f, 190f), new Vector2(210f, 16f), 8f, textDim, spacing: 3f);

        stepRows        = new GameObject[steps.Length];
        stepRowBgs      = new Image[steps.Length];
        stepNumberTexts = new TextMeshProUGUI[steps.Length];
        stepLabelTexts  = new TextMeshProUGUI[steps.Length];

        float startY  = 158f;
        float rowH    = 52f;

        for (int i = 0; i < steps.Length; i++)
        {
            float y = startY - i * rowH;
            var row = BuildStepRow(panel, i, new Vector2(4f, y));
            stepRows[i] = row;
        }
    }

    private GameObject BuildStepRow(GameObject parent, int index, Vector2 pos)
    {
        bool isFirst = index == 0;

        var row = new GameObject($"StepRow_{index + 1}");
        row.transform.SetParent(parent.transform, false);
        var rt = row.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(220f, 48f);

        // Fundo da row (highlight quando ativo)
        stepRowBgs[index] = MakeImage(row, Color.clear, Vector2.zero, new Vector2(220f, 48f));

        // Borda lateral esquerda (indicador de estado)
        var indicator = MakeImage(row, isFirst ? textBright : textFaint,
            new Vector2(-105f, 0f), new Vector2(2f, 32f));
        stepNumberTexts[index] = indicator.GetComponent<TextMeshProUGUI>(); // repurposed below

        // Número do passo
        stepNumberTexts[index] = MakeTMP(row, $"0{index + 1}",
            new Vector2(-82f, 8f), new Vector2(36f, 18f), 13f,
            isFirst ? textBright : textFaint, spacing: 1f);

        // Label do passo
        stepLabelTexts[index] = MakeTMP(row, steps[index].label,
            new Vector2(20f, 8f), new Vector2(160f, 16f), 9f,
            isFirst ? textBright : textDim,
            align: TextAlignmentOptions.Left, spacing: 2f);

        // Separador entre rows
        if (index < steps.Length - 1)
            MakeImage(row, borderDash, new Vector2(5f, -24f), new Vector2(210f, 1f));

        return row;
    }

    // ── Painel de instrução (direita) ────────────────────────────────────
    private void BuildInstructionPanel(GameObject root)
    {
        var panel = new GameObject("InstructionPanel");
        panel.transform.SetParent(root.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(145f, -14f);
        rt.sizeDelta        = new Vector2(570f, 420f);

        // Número grande da etapa (decorativo)
        bigStepNumber = MakeTMP(panel, "01",
            new Vector2(-240f, 145f), new Vector2(120f, 80f), 64f,
            new Color(1f, 1f, 1f, 0.06f), spacing: 2f);

        // Título da instrução
        instructionTitle = MakeTMP(panel, "VERIFIQUE O LOCAL",
            new Vector2(10f, 130f), new Vector2(510f, 32f), 18f, textBright,
            align: TextAlignmentOptions.Left, spacing: 1.5f);

        // Separador
        MakeImage(panel, borderMed, new Vector2(10f, 107f), new Vector2(510f, 1f));

        // Descrição
        instructionDesc = MakeTMP(panel, "",
            new Vector2(10f, 40f), new Vector2(510f, 110f), 12f, textMed,
            align: TextAlignmentOptions.TopLeft);
        if (instructionDesc != null) instructionDesc.lineSpacing = 18f;

        // Separador inferior
        MakeImage(panel, borderDash, new Vector2(10f, -75f), new Vector2(510f, 1f));

        // Caixa INTERAÇÃO VR
        BuildVRHintBox(panel, new Vector2(10f, -110f));

        // Especificações de compressão (apenas passos 5 e 6)
        BuildCompressionSpec(panel, new Vector2(10f, -155f));
    }

    private void BuildVRHintBox(GameObject parent, Vector2 pos)
    {
        var box = new GameObject("VRHint");
        box.transform.SetParent(parent.transform, false);
        var rt = box.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(510f, 52f);

        var bg = MakeImage(box, bgPanel, Vector2.zero, new Vector2(510f, 52f));
        var ol = bg.gameObject.AddComponent<Outline>();
        ol.effectColor    = borderDash;
        ol.effectDistance = new Vector2(1f, -1f);

        MakeTMP(box, "INTERAÇÃO VR",
            new Vector2(-180f, 14f), new Vector2(200f, 16f), 8f, textDim,
            align: TextAlignmentOptions.Left, spacing: 4f);
        MakeTMP(box, "OLHE PARA A ZONA DESTACADA PARA CONFIRMAR",
            new Vector2(-180f, -4f), new Vector2(400f, 16f), 9.5f, textMed,
            align: TextAlignmentOptions.Left, spacing: 1f);
    }

    private void BuildCompressionSpec(GameObject parent, Vector2 pos)
    {
        // Especificações de compressão sempre visíveis (info útil mesmo nos primeiros passos)
        var box = new GameObject("CompressionSpec");
        box.transform.SetParent(parent.transform, false);
        var rt = box.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(510f, 52f);

        var bg = MakeImage(box, bgPanel, Vector2.zero, new Vector2(510f, 52f));
        var ol = bg.gameObject.AddComponent<Outline>();
        ol.effectColor    = borderDash;
        ol.effectDistance = new Vector2(1f, -1f);

        MakeTMP(box, "SPEC. DE COMPRESSÃO",
            new Vector2(-190f, 14f), new Vector2(200f, 16f), 8f, textDim,
            align: TextAlignmentOptions.Left, spacing: 4f);

        // Especificações inline: PROF · TAXA · RETORNO · CICLO
        string[] labels = { "PROF.",      "TAXA",       "RETORNO",   "CICLO" };
        string[] values = { "5–6 cm",     "100–110/min","COMPLETO",  "30 : 2" };
        float startX = -185f;
        float stepX  = 126f;
        for (int i = 0; i < labels.Length; i++)
        {
            float x = startX + i * stepX;
            MakeTMP(box, labels[i], new Vector2(x, -2f),  new Vector2(110f, 14f), 8f,  textDim, align: TextAlignmentOptions.Left, spacing: 2f);
            MakeTMP(box, values[i], new Vector2(x, -16f), new Vector2(110f, 16f), 9.5f, textMed, align: TextAlignmentOptions.Left, spacing: 1f);
        }
    }

    // ── Grelha de fundo ──────────────────────────────────────────────────
    private void BuildBackgroundGrid(GameObject root)
    {
        // Simulamos a grelha com linhas finas
        int cols = 13, rows = 8;
        float w = 820f, h = 500f;

        for (int c = 1; c < cols; c++)
        {
            float x = -w / 2f + c * (w / cols);
            MakeImage(root, new Color(1f, 1f, 1f, 0.025f), new Vector2(x, 0f), new Vector2(1f, h));
        }
        for (int r = 1; r < rows; r++)
        {
            float y = -h / 2f + r * (h / rows);
            MakeImage(root, new Color(1f, 1f, 1f, 0.025f), new Vector2(0f, y), new Vector2(w, 1f));
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────
    private Image MakeImage(GameObject parent, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Img");
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
        TextAlignmentOptions align   = TextAlignmentOptions.Center,
        float spacing = 0f)
    {
        var go = new GameObject($"T_{text[..Mathf.Min(8, text.Length)]}");
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
        tmp.enableWordWrapping = true;
        if (customFont != null) tmp.font = customFont;
        return tmp;
    }

    private void AddCorner(GameObject parent, Vector2 pos, Vector2 size, Vector2 offset, Color color)
    {
        MakeImage(parent, color, pos + offset, size);
    }
}
