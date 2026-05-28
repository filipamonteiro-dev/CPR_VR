using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD flutuante junto ao telefone que guia o jogador a marcar 1-1-2.
/// Mostra os dígitos já marcados e a instrução "MARQUE 1-1-2".
///
/// Setup no Editor:
///   1. Cria um GameObject vazio perto do telefone (ou filho do telefone).
///   2. Adiciona este script.
///   3. Liga phoneDialer, xrCamera e customFont no Inspector.
///   4. Opcional: ajusta offsetFromPhone para posicionar o painel.
/// </summary>
[DisallowMultipleComponent]
public class DialerFeedbackHUD : MonoBehaviour
{
    [Header("Ligação")]
    [SerializeField] private PhoneDialer   phoneDialer;
    [SerializeField] private Camera        xrCamera;
    [SerializeField] private TMP_FontAsset customFont;

    [Header("Posicionamento")]
    [SerializeField] private Vector3 offsetFromPhone = new Vector3(0f, 0.25f, 0f);

    // ── Refs internas ─────────────────────────────────────────────────────
    private TextMeshProUGUI[] digitSlots = new TextMeshProUGUI[3];
    private CanvasGroup        group;

    private static readonly Color BgColor     = new Color(0.02f, 0.03f, 0.06f, 0.93f);
    private static readonly Color Accent      = new Color(1f, 1f, 1f, 0.40f);
    private static readonly Color TextDim     = new Color(1f, 1f, 1f, 0.28f);
    private static readonly Color SlotEmpty   = new Color(1f, 1f, 1f, 0.20f);
    private static readonly Color SlotFilled  = new Color(1f, 1f, 1f, 0.92f);
    private static readonly Color SlotCorrect = new Color(0.55f, 1f, 0.60f, 0.90f);

    // dígitos correctos esperados
    private static readonly char[] Expected = { '1', '1', '2' };

    void Awake()
    {
        BuildHUD();
        group.alpha = 0f;
    }

    void Start()
    {
        if (phoneDialer != null)
        {
            phoneDialer.DigitsChanged         += OnDigitsChanged;
            phoneDialer.EmergencyNumberDialed += OnDialComplete;
        }

        // Fade in imediato (o state activa o GameObject)
        if (group != null) group.alpha = 1f;
    }

    void OnDestroy()
    {
        if (phoneDialer != null)
        {
            phoneDialer.DigitsChanged         -= OnDigitsChanged;
            phoneDialer.EmergencyNumberDialed -= OnDialComplete;
        }
    }

    void LateUpdate()
    {
        FaceCamera();
    }

    // ── Resposta ao PhoneDialer ───────────────────────────────────────────

    private void OnDigitsChanged(PhoneDialer dialer, string digits)
    {
        RefreshSlots(digits);
    }

    private void OnDialComplete(PhoneDialer dialer)
    {
        // Mostra todos verdes e faz fade out suave
        for (int i = 0; i < digitSlots.Length; i++)
        {
            if (digitSlots[i] != null)
            {
                digitSlots[i].text  = Expected[i].ToString();
                digitSlots[i].color = SlotCorrect;
            }
        }
        StartCoroutine(FadeOut(1.2f));
    }

    private void RefreshSlots(string digits)
    {
        for (int i = 0; i < digitSlots.Length; i++)
        {
            if (digitSlots[i] == null) continue;

            if (i < digits.Length)
            {
                digitSlots[i].text  = digits[i].ToString();
                digitSlots[i].color = digits[i] == Expected[i] ? SlotCorrect : SlotFilled;
            }
            else
            {
                digitSlots[i].text  = "_";
                digitSlots[i].color = SlotEmpty;
            }
        }
    }

    // ── Posicionamento ────────────────────────────────────────────────────

    private void FaceCamera()
    {
        if (xrCamera == null) xrCamera = Camera.main;
        if (xrCamera == null) return;

        transform.position = transform.parent != null
            ? transform.parent.position + offsetFromPhone
            : transform.position;

        transform.LookAt(xrCamera.transform.position);
        transform.Rotate(0f, 180f, 0f);
    }

    // ── Construção ────────────────────────────────────────────────────────

    private void BuildHUD()
    {
        const float W = 260f, H = 130f;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.worldCamera = xrCamera;
        gameObject.AddComponent<CanvasScaler>();

        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(W, H);
        transform.localScale = Vector3.one * 0.001f;

        group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        // Fundo
        var bg = MakeImg(gameObject, BgColor, Vector2.zero, new Vector2(W, H));
        var outline = bg.gameObject.AddComponent<Outline>();
        outline.effectColor    = new Color(1f, 1f, 1f, 0.14f);
        outline.effectDistance = new Vector2(1f, -1f);

        // Cantos
        float hw = W / 2f + 3f, hh = H / 2f + 3f;
        MakeImg(gameObject, Accent, new Vector2(-hw + 7f, hh),   new Vector2(14f, 1.5f));
        MakeImg(gameObject, Accent, new Vector2(-hw, hh - 7f),   new Vector2(1.5f, 14f));
        MakeImg(gameObject, Accent, new Vector2( hw - 7f, hh),   new Vector2(14f, 1.5f));
        MakeImg(gameObject, Accent, new Vector2( hw, hh - 7f),   new Vector2(1.5f, 14f));
        MakeImg(gameObject, Accent, new Vector2(-hw + 7f, -hh),  new Vector2(14f, 1.5f));
        MakeImg(gameObject, Accent, new Vector2(-hw, -hh + 7f),  new Vector2(1.5f, 14f));
        MakeImg(gameObject, Accent, new Vector2( hw - 7f, -hh),  new Vector2(14f, 1.5f));
        MakeImg(gameObject, Accent, new Vector2( hw, -hh + 7f),  new Vector2(1.5f, 14f));

        // Instrução topo
        MakeTMP(gameObject, "// EMERGÊNCIA //", new Vector2(0f, 48f), new Vector2(240f, 14f),
            8f, TextDim, TextAlignmentOptions.Center, 5f);

        MakeTMP(gameObject, "MARQUE  1 - 1 - 2", new Vector2(0f, 28f), new Vector2(240f, 20f),
            13f, new Color(1f, 1f, 1f, 0.75f), TextAlignmentOptions.Center, 2f);

        // Separador
        MakeImg(gameObject, new Color(1f, 1f, 1f, 0.08f), new Vector2(0f, 12f), new Vector2(220f, 1f));

        // Slots de dígitos
        float[] xPositions = { -54f, 0f, 54f };
        for (int i = 0; i < 3; i++)
        {
            // Caixa de cada dígito
            var slotBg = MakeImg(gameObject, new Color(1f, 1f, 1f, 0.05f),
                new Vector2(xPositions[i], -12f), new Vector2(44f, 40f));
            var slotOutline = slotBg.gameObject.AddComponent<Outline>();
            slotOutline.effectColor    = new Color(1f, 1f, 1f, 0.12f);
            slotOutline.effectDistance = new Vector2(1f, -1f);

            digitSlots[i] = MakeTMP(gameObject, "_",
                new Vector2(xPositions[i], -12f), new Vector2(44f, 40f),
                22f, SlotEmpty, TextAlignmentOptions.Center, 0f);
        }

        // Nota de rodapé
        MakeTMP(gameObject, "pressione ligar após marcar", new Vector2(0f, -50f), new Vector2(240f, 13f),
            8f, TextDim, TextAlignmentOptions.Center, 2f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private System.Collections.IEnumerator FadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        float t = 0f, dur = 0.5f;
        float start = group.alpha;
        while (t < dur)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, 0f, t / dur);
            yield return null;
        }
        group.alpha = 0f;
    }

    private Image MakeImg(GameObject parent, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("I");
        go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        r.anchoredPosition = pos;
        r.sizeDelta        = size;
        var img = go.AddComponent<Image>();
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
