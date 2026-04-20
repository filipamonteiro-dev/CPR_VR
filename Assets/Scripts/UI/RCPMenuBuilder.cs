using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.SceneManagement;

namespace VRCPR.UI
{
    public class RCPMenuBuilder : MonoBehaviour
    {
        [Header("Tipografia")]
        [Tooltip("Arraste o Font Asset gerado neste campo pelo Inspector.")]
        [SerializeField] private TMP_FontAsset menuCustomFont;

        [Header("Configuração")]
        public Camera xrCamera;

        private Canvas menuCanvas;
        private GameObject menuRoot;

        // Cores do design (baseadas no Wireframe)
        private readonly Color bgColor = new Color(0.05f, 0.08f, 0.12f, 1f); // Mantido o seu fundo escuro
        private readonly Color borderColor = new Color(1f, 1f, 1f, 0.2f); // rgba(255,255,255,0.2)
        private readonly Color accentBorderColor = new Color(1f, 1f, 1f, 0.55f);
        private readonly Color dangerBorderColor = new Color(1f, 0.31f, 0.31f, 0.45f); // rgba(255,80,80,0.45)
        
        private readonly Color textColor = new Color(1f, 1f, 1f, 0.92f);
        private readonly Color subTextColor = new Color(1f, 1f, 1f, 0.28f);
        private readonly Color highlightTextColor = new Color(1f, 1f, 1f, 0.9f);
        private readonly Color selectTextAccentColor = new Color(1f, 1f, 1f, 0.2f);
        private readonly Color selectTextDangerColor = new Color(1f, 0.39f, 0.39f, 0.35f);

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
            //menuRoot.AddComponent<TrackedDeviceGraphicRaycaster>();
            menuRoot.AddComponent<CanvasScaler>();
            menuRoot.AddComponent<GraphicRaycaster>();

            var rt = menuRoot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 820);
            menuRoot.transform.localScale = Vector3.one * 0.001f;
            
            // Fundo geral preto do Canvas (o "Space" à volta do menu)
            AddImage(menuRoot, new Color(0, 0, 0, 1), Vector2.zero, new Vector2(1195, 682));

            // --- DECORAÇÕES E ANOTAÇÕES FLUTUANTES ---
            BuildFloatingAnnotations();

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
            bool[] isAccent = { true, false, false, false };
            bool[] isDanger = { false, false, false, true };

            // Ações de cada botão
            System.Action[] actions = {
                () => SceneManager.LoadScene("TrainingScene"), // Substitui com o nome exato da tua cena de Treino
                () => SceneManager.LoadScene("TestModeScene"), // Modo Teste
                () => SceneManager.LoadScene("TutorialScene"), // Tutorial
                () => QuitApplication()                        // Sair
            };

            float startY = 120f;
            for (int i = 0; i < labels.Length; i++)
            {
                BuildButton(
                    label: labels[i],
                    subtitle: subtitles[i],
                    posY: startY - i * 110f,
                    isAccent: isAccent[i],
                    isDanger: isDanger[i],
                    onClick: actions[i] // Passamos a ação correspondente
                );
            }

            // --- NOTA DE RODAPÉ ---
            var footer = CreateTMPText(menuRoot, "USE O GATILHO DO CONTROLE PARA SELECIONAR",
                new Vector2(0, -320), new Vector2(400, 20), 10f, new Color(1f, 1f, 1f, 0.15f));
            footer.alignment = TextAlignmentOptions.Center;
            footer.characterSpacing = 3f;
        }

        void BuildFloatingAnnotations()
        {
            // Top Right Version
            var version = CreateTMPText(menuRoot, "VR-RCP // WIREFRAME v0.1",
                new Vector2(350, 380), new Vector2(200, 20), 10f, new Color(1f, 1f, 1f, 0.18f));
            version.alignment = TextAlignmentOptions.Right;
            version.characterSpacing = 3f;

            // Left Annotation (Rodado)
            var leftNote = CreateTMPText(menuRoot, "PAINEL DE NAVEGAÇÃO PRINCIPAL",
                new Vector2(-350, 0), new Vector2(400, 20), 10f, new Color(1f, 1f, 1f, 0.12f));
            leftNote.alignment = TextAlignmentOptions.Center;
            leftNote.characterSpacing = 5f;
            leftNote.rectTransform.localEulerAngles = new Vector3(0, 0, 90);

            // Right Annotation Tree
            string treeTxt = "┌── LARGURA DO PAINEL: 440px\n├── BOTÕES: 4\n├── FONTE: SPACE MONO\n└── PROFUNDIDADE: Z+0,5m";
            var rightTree = CreateTMPText(menuRoot, treeTxt,
                new Vector2(380, 50), new Vector2(250, 100), 9f, new Color(1f, 1f, 1f, 0.14f));
            rightTree.alignment = TextAlignmentOptions.Left;
            rightTree.characterSpacing = 2f;
            rightTree.lineSpacing = 15f;
        }

        void BuildHeader()
        {
            // Ícone Médico (Cruz vazada)
            var iconRoot = new GameObject("MedicalIcon");
            iconRoot.transform.SetParent(menuRoot.transform, false);
            var iconRt = iconRoot.AddComponent<RectTransform>();
            iconRt.anchoredPosition = new Vector2(0, 330);
            iconRt.sizeDelta = new Vector2(48, 48);

            // Borda do ícone
            var iconBorder = AddImage(iconRoot, Color.clear, Vector2.zero, new Vector2(32, 32));
            var iconOutline = iconBorder.gameObject.AddComponent<Outline>();
            iconOutline.effectColor = borderColor;
            iconOutline.effectDistance = new Vector2(1, -1);

            // Linha Horiz (Cruz)
            AddImage(iconRoot, new Color(1f, 1f, 1f, 0.5f), Vector2.zero, new Vector2(48, 1));
            // Linha Vert (Cruz)
            AddImage(iconRoot, new Color(1f, 1f, 1f, 0.5f), Vector2.zero, new Vector2(1, 48));

            // Linha superior: // SISTEMA DE TREINO VR //
            var sys = CreateTMPText(menuRoot, "// SISTEMA DE TREINO VR //",
                new Vector2(0, 275), new Vector2(400, 30), 11f, new Color(1f, 1f, 1f, 0.3f));
            sys.alignment = TextAlignmentOptions.Center;
            sys.characterSpacing = 5f;

            // Título principal
            var title = CreateTMPText(menuRoot, "TREINO DE RCP",
                new Vector2(0, 235), new Vector2(420, 60), 36f, textColor);
            title.alignment = TextAlignmentOptions.Center;
            title.characterSpacing = 8f;
            title.fontStyle = FontStyles.Bold;

            // Subtítulo
            var sub = CreateTMPText(menuRoot, "RESSUSCITAÇÃO CARDIOPULMONAR",
                new Vector2(0, 200), new Vector2(400, 25), 10f, new Color(1f, 1f, 1f, 0.25f));
            sub.alignment = TextAlignmentOptions.Center;
            sub.characterSpacing = 4f;

            // Decorative Divider
            var divRoot = new GameObject("Divider");
            divRoot.transform.SetParent(menuRoot.transform, false);
            var divRt = divRoot.AddComponent<RectTransform>();
            divRt.anchoredPosition = new Vector2(0, 175);
            
            AddImage(divRoot, new Color(1f, 1f, 1f, 0.12f), new Vector2(-40, 0), new Vector2(64, 1)); // Linha Esq
            var diamond = AddImage(divRoot, new Color(1f, 1f, 1f, 0.2f), Vector2.zero, new Vector2(6, 6)); // Losango
            diamond.rectTransform.localEulerAngles = new Vector3(0, 0, 45);
            AddImage(divRoot, new Color(1f, 1f, 1f, 0.12f), new Vector2(40, 0), new Vector2(64, 1)); // Linha Dir
        }

        void BuildButton(string label, string subtitle, float posY, bool isAccent, bool isDanger, System.Action onClick)
        {
            Color currentBorderColor = isDanger ? dangerBorderColor : (isAccent ? accentBorderColor : borderColor);
            Color hoverColor = isDanger ? new Color(1f, 0.31f, 0.31f, 0.08f) : (isAccent ? new Color(1f, 1f, 1f, 0.08f) : new Color(1f, 1f, 1f, 0.04f));
            Color pressedColor = isDanger ? new Color(1f, 0.31f, 0.31f, 0.15f) : (isAccent ? new Color(1f, 1f, 1f, 0.15f) : new Color(1f, 1f, 1f, 0.1f));

            var btnObj = new GameObject($"Button_{label}");
            btnObj.transform.SetParent(menuRoot.transform, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, posY);
            rt.sizeDelta = new Vector2(440, 95);

            // Fundo base do botão e borda tracejada
            var bgImg = AddImage(btnObj, Color.clear, Vector2.zero, new Vector2(440, 95));
            var outline = bgImg.gameObject.AddComponent<Outline>();
            outline.effectColor = currentBorderColor;
            outline.effectDistance = new Vector2(1, -1);

            // Adicionando os cantos acentuados
            AddCorner(btnObj, new Vector2(-220, 47.5f), new Vector2(8, 2), currentBorderColor, new Vector2(4, -1));
            AddCorner(btnObj, new Vector2(-220, 47.5f), new Vector2(2, 8), currentBorderColor, new Vector2(1, -4));
            AddCorner(btnObj, new Vector2(220, 47.5f), new Vector2(8, 2), currentBorderColor, new Vector2(-4, -1));
            AddCorner(btnObj, new Vector2(220, 47.5f), new Vector2(2, 8), currentBorderColor, new Vector2(-1, -4));
            AddCorner(btnObj, new Vector2(-220, -47.5f), new Vector2(8, 2), currentBorderColor, new Vector2(4, 1));
            AddCorner(btnObj, new Vector2(-220, -47.5f), new Vector2(2, 8), currentBorderColor, new Vector2(1, 4));
            AddCorner(btnObj, new Vector2(220, -47.5f), new Vector2(8, 2), currentBorderColor, new Vector2(-4, 1));
            AddCorner(btnObj, new Vector2(220, -47.5f), new Vector2(2, 8), currentBorderColor, new Vector2(-1, 4));

            // Botão Unity
            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = pressedColor;
            colors.selectedColor = hoverColor;
            btn.colors = colors;

            // ----> AQUI CONECTAMOS O EVENTO DE CLIQUE AO BOTÃO <----
            if (onClick != null)
            {
                btn.onClick.AddListener(() => onClick.Invoke());
            }

            // Labels e resto do botão...
            Color labelColor = isDanger ? new Color(1f, 0.39f, 0.39f, 0.8f) : (isAccent ? highlightTextColor : new Color(1f, 1f, 1f, 0.65f));
            var lbl = CreateTMPText(btnObj, label,
                new Vector2(-30, 10), new Vector2(300, 30), 14f, labelColor);
            lbl.characterSpacing = 3f;

            CreateTMPText(btnObj, subtitle,
                new Vector2(-30, -14), new Vector2(320, 22), 10f, subTextColor);

            Color selColor = isDanger ? selectTextDangerColor : selectTextAccentColor;
            var sel = CreateTMPText(btnObj, "[SELECIONAR]",
                new Vector2(140, 0), new Vector2(130, 30), 10f, selColor);
            sel.alignment = TextAlignmentOptions.Right;
            sel.characterSpacing = 2f;
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

        void AddCorner(GameObject parent, Vector2 pos, Vector2 size, Color color, Vector2 offset)
        {
            var img = AddImage(parent, color, pos + offset, size);
            img.gameObject.name = "CornerAccent";
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

            if (menuCustomFont != null)
            {
                tmp.font = menuCustomFont;
            }

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

        void QuitApplication()
        {
            Debug.Log("A fechar aplicação...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}