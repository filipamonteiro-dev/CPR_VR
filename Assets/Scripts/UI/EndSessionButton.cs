using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Botão flutuante "TERMINAR SESSÃO" em world space.
/// Posiciona-se à direita do jogador ao nível da mão.
///
/// Setup no Editor:
///   1. Cria um GameObject vazio na cena de treino/teste.
///   2. Adiciona este script.
///   3. Liga trainingManager, xrCamera e customFont no Inspector.
/// </summary>
[DisallowMultipleComponent]
public class EndSessionButton : MonoBehaviour
{
    [Header("Ligação")]
    [SerializeField] private TrainingManager trainingManager;
    [SerializeField] private Camera          xrCamera;
    [SerializeField] private TMP_FontAsset   customFont;

    [Header("Posicionamento")]
    [SerializeField] private float distanceForward = 0.6f;
    [SerializeField] private float distanceRight   = 0.35f;
    [SerializeField] private float verticalOffset  = -0.45f;

    private static readonly Color BgColor     = new Color(0.02f, 0.03f, 0.06f, 0.92f);
    private static readonly Color BorderColor = new Color(1f, 0.35f, 0.35f, 0.45f);
    private static readonly Color CornerColor = new Color(1f, 0.35f, 0.35f, 0.55f);
    private static readonly Color TextColor   = new Color(1f, 0.45f, 0.45f, 0.85f);
    private static readonly Color TextDim     = new Color(1f, 1f, 1f, 0.25f);

    void Awake()
    {
        BuildButton();
        PositionButton();
    }

    private void BuildButton()
    {
        const float W = 200f, H = 56f;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.worldCamera = xrCamera;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();

        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(W, H);
        transform.localScale = Vector3.one * 0.001f;

        // Fundo
        var bg = MakeImg(gameObject, BgColor, Vector2.zero, new Vector2(W, H));
        var ol = bg.gameObject.AddComponent<Outline>();
        ol.effectColor    = BorderColor;
        ol.effectDistance = new Vector2(1f, -1f);

        // Cantos
        float hw = W / 2f, hh = H / 2f;
        MakeImg(gameObject, CornerColor, new Vector2(-hw + 6f,  hh),  new Vector2(12f, 1.5f));
        MakeImg(gameObject, CornerColor, new Vector2(-hw,  hh - 6f),  new Vector2(1.5f, 12f));
        MakeImg(gameObject, CornerColor, new Vector2( hw - 6f,  hh),  new Vector2(12f, 1.5f));
        MakeImg(gameObject, CornerColor, new Vector2( hw,  hh - 6f),  new Vector2(1.5f, 12f));
        MakeImg(gameObject, CornerColor, new Vector2(-hw + 6f, -hh),  new Vector2(12f, 1.5f));
        MakeImg(gameObject, CornerColor, new Vector2(-hw, -hh + 6f),  new Vector2(1.5f, 12f));
        MakeImg(gameObject, CornerColor, new Vector2( hw - 6f, -hh),  new Vector2(12f, 1.5f));
        MakeImg(gameObject, CornerColor, new Vector2( hw, -hh + 6f),  new Vector2(1.5f, 12f));

        // Hitbox invisível para o botão
        var hitGO  = new GameObject("Hit");
        hitGO.transform.SetParent(transform, false);
        var hitRt  = hitGO.AddComponent<RectTransform>();
        hitRt.sizeDelta = new Vector2(W, H);
        var hitImg = hitGO.AddComponent<Image>();
        hitImg.color = Color.clear;

        var btn    = gameObject.AddComponent<Button>();
        btn.targetGraphic = hitImg;
        var colors = btn.colors;
        colors.normalColor      = Color.clear;
        colors.highlightedColor = new Color(1f, 0.35f, 0.35f, 0.08f);
        colors.pressedColor     = new Color(1f, 0.35f, 0.35f, 0.18f);
        colors.selectedColor    = colors.highlightedColor;
        btn.colors = colors;
        btn.onClick.AddListener(OnClick);

        // Textos
        MakeTMP(gameObject, "TERMINAR SESSÃO", new Vector2(0f, 9f),  new Vector2(180f, 22f),
            12f, TextColor, TextAlignmentOptions.Center, 2f);
        MakeTMP(gameObject, "guardar e ver resultados", new Vector2(0f, -10f), new Vector2(180f, 14f),
            8f, TextDim, TextAlignmentOptions.Center, 1f);
    }

    private void PositionButton()
    {
        if (xrCamera == null) xrCamera = Camera.main;
        if (xrCamera == null) return;

        Transform cam = xrCamera.transform;
        Vector3   fwd = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        transform.position = cam.position
            + fwd   * distanceForward
            + right * distanceRight
            + Vector3.up * (cam.position.y + verticalOffset - cam.position.y + verticalOffset);

        // mais simples: posição absoluta
        transform.position = new Vector3(
            cam.position.x + fwd.x * distanceForward + right.x * distanceRight,
            cam.position.y + verticalOffset,
            cam.position.z + fwd.z * distanceForward + right.z * distanceRight
        );

        transform.LookAt(cam.position);
        transform.Rotate(0f, 180f, 0f);
    }

    private void OnClick()
    {
        if (trainingManager != null)
            trainingManager.EndSession();
    }

    private Image MakeImg(GameObject parent, Color color, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject("I");
        go.transform.SetParent(parent.transform, false);
        var r   = go.AddComponent<RectTransform>();
        r.anchoredPosition = pos;
        r.sizeDelta        = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private TextMeshProUGUI MakeTMP(GameObject parent, string text, Vector2 pos, Vector2 size,
        float fs, Color color, TextAlignmentOptions align, float spacing = 0f)
    {
        var go  = new GameObject("T");
        go.transform.SetParent(parent.transform, false);
        var r   = go.AddComponent<RectTransform>();
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
