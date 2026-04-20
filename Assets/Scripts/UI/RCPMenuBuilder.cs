using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class RCPMenuBuilder : MonoBehaviour
{
    [Header("Configuração")]
    public Camera xrCamera;

    private Canvas menuCanvas;
    private GameObject menuRoot;

    // Cores do design
    private readonly Color bgColor = new Color(0.05f, 0.08f, 0.12f, 1f);
    private readonly Color borderColor = new Color(0.2f, 0.5f, 0.4f, 1f);
    private readonly Color textColor = Color.white;
    private readonly Color subTextColor = new Color(0.6f, 0.7f, 0.65f, 1f);
    private readonly Color selectColor = new Color(0.4f, 0.8f, 0.6f, 1f);
    private readonly Color exitColor = new Color(0.8f, 0.3f, 0.3f, 1f);

    void Start()
    {
        BuildMenu();
        PositionInFrontOfPlayer();
    }

    void BuildMenu()
    {
        // --- ROOT ---
        menuRoot = new GameObject("RCPMenu");

        // --- CANVAS ---
        menuCanvas = menuRoot.AddComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.WorldSpace;
        menuCanvas.worldCamera = xrCamera;

        // Necessário para interação com controladores XR
        menuRoot.AddComponent<TrackedDeviceGraphicRaycaster>();

        var rt = menuRoot.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(440, 820);
        menuRoot.transform.localScale = Vector3.one * 0.001f;

        // Fundo geral
        AddImage(menuRoot, bgColor, Vector2.zero, new Vector2(440, 820));

        // --- HEADER ---
        BuildHeader();

        // --- BOTÕES ---
        string[] labels = { "INICIAR TREINO", "MODO TESTE", "TUTORIAL", "SAIR" };
        string[] subtitles = {
            "simulação guiada · modo iniciante",
            "avaliação de desempenho · cronometrado",
            "instrução passo a passo · aprender",
            "fechar aplicação"
        };
        System.Action[] actions = {
            () => Debug.Log("Iniciar Treino"),
            () => Debug.Log("Modo Teste"),
            () => Debug.Log("Tutorial"),
            () => Debug.Log("Sair")
        };

        float startY = 180f;
        for (int i = 0; i < labels.Length; i++)
        {
            bool isExit = labels[i] == "SAIR";
            BuildButton(
                label: labels[i],
                subtitle: subtitles[i],
                posY: startY - i * 115f,
                isExit: isExit,
                onClick: actions[i]
            );
        }

        // --- TAB BAR ---
        BuildTabBar();
    }

    void BuildHeader()
    {
        // Linha superior: // SISTEMA DE TREINO VR //
        var sys = CreateTMPText(menuRoot, "// SISTEMA DE TREINO VR //",
            new Vector2(0, 340), new Vector2(400, 30), 10f, subTextColor);
        sys.alignment = TextAlignmentOptions.Center;
        sys.characterSpacing = 4f;

        // Título principal
        var title = CreateTMPText(menuRoot, "TREINO DE RCP",
            new Vector2(0, 295), new Vector2(420, 60), 36f, textColor);
        title.alignment = TextAlignmentOptions.Center;
        title.characterSpacing = 8f;
        title.fontStyle = FontStyles.Bold;

        // Subtítulo
        var sub = CreateTMPText(menuRoot, "RESSUSCITAÇÃO CARDIOPULMONAR",
            new Vector2(0, 258), new Vector2(400, 25), 9f, subTextColor);
        sub.alignment = TextAlignmentOptions.Center;
        sub.characterSpacing = 3f;
    }

    void BuildButton(string label, string subtitle, float posY,
                     bool isExit, System.Action onClick)
    {
        var btnObj = new GameObject($"Button_{label}");
        btnObj.transform.SetParent(menuRoot.transform, false);

        var rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(420, 95);

        // Borda pontilhada (imagem com outline)
        var border = AddImage(btnObj, Color.clear, Vector2.zero, new Vector2(420, 95));
        var outline = border.gameObject.AddComponent<Outline>();
        outline.effectColor = isExit ? exitColor : borderColor;
        outline.effectDistance = new Vector2(1, -1);

        // Botão Unity para capturar clique/XR
        var btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());

        // Highlight ao hover
        var colors = btn.colors;
        colors.normalColor = Color.clear;
        colors.highlightedColor = new Color(0.1f, 0.25f, 0.2f, 0.4f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.3f, 0.6f);
        btn.colors = colors;

        // Label
        Color labelColor = isExit ? exitColor : textColor;
        var lbl = CreateTMPText(btnObj, label,
            new Vector2(-30, 12), new Vector2(300, 30), 14f, labelColor);
        lbl.characterSpacing = 3f;
        lbl.fontStyle = FontStyles.Bold;

        // Subtítulo
        CreateTMPText(btnObj, subtitle,
            new Vector2(-30, -14), new Vector2(320, 22), 8f, subTextColor);

        // [SELECIONAR]
        Color selColor = isExit ? exitColor : selectColor;
        var sel = CreateTMPText(btnObj, "[SELECIONAR]",
            new Vector2(120, 0), new Vector2(130, 30), 9f, selColor);
        sel.alignment = TextAlignmentOptions.Right;
    }

    void BuildTabBar()
    {
        string[] tabs = { "MENU PRINCIPAL", "HUD DE TREINO", "TUTORIAL", "MODO TESTE" };

        var bar = new GameObject("TabBar");
        bar.transform.SetParent(menuRoot.transform, false);
        var barRT = bar.AddComponent<RectTransform>();
        barRT.anchoredPosition = new Vector2(0, -370);
        barRT.sizeDelta = new Vector2(440, 40);

        AddImage(bar, new Color(0.08f, 0.12f, 0.16f), Vector2.zero, new Vector2(440, 40));

        float xStart = -150f;
        for (int i = 0; i < tabs.Length; i++)
        {
            bool active = i == 0;
            var tab = new GameObject($"Tab_{tabs[i]}");
            tab.transform.SetParent(bar.transform, false);

            var rt = tab.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(xStart + i * 100f, 0);
            rt.sizeDelta = new Vector2(95, 38);

            if (active)
                AddImage(tab, new Color(0.1f, 0.3f, 0.25f), Vector2.zero, new Vector2(95, 38));

            Color tabColor = active ? selectColor : subTextColor;
            var txt = CreateTMPText(tab, tabs[i],
                Vector2.zero, new Vector2(90, 30), 6.5f, tabColor);
            txt.alignment = TextAlignmentOptions.Center;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────

    Image AddImage(GameObject parent, Color color, Vector2 pos, Vector2 size)
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

    TextMeshProUGUI CreateTMPText(GameObject parent, string text,
        Vector2 pos, Vector2 size, float fontSize, Color color)
    {
        var go = new GameObject("Text_" + text.Substring(0, Mathf.Min(8, text.Length)));
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        return tmp;
    }

    void PositionInFrontOfPlayer()
    {
        if (xrCamera == null) xrCamera = Camera.main;
        Transform cam = xrCamera.transform;
        menuRoot.transform.position = cam.position + cam.forward * 1.5f;
        menuRoot.transform.LookAt(cam.position);
        menuRoot.transform.Rotate(0, 180, 0);
    }
}