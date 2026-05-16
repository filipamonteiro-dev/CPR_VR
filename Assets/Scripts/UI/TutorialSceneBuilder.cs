using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.SceneManagement;

namespace VRCPR.UI
{
    public class TutorialSceneBuilder : MonoBehaviour
    {
        [Header("Tipografia")]
        [Tooltip("Arraste o Font Asset gerado neste campo pelo Inspector.")]
        [SerializeField] private TMP_FontAsset tutorialFont;

        [Header("Configuração")]
        public Camera xrCamera;

        private Canvas tutorialCanvas;
        private GameObject rootUI;

        // Referências para atualizar a UI dinamicamente no futuro
        private TextMeshProUGUI stepCountText;
        private TextMeshProUGUI stepNumberText;
        private TextMeshProUGUI stepLabelText;
        private TextMeshProUGUI instructionTitleText;
        private TextMeshProUGUI instructionDescText;
        
        // Cores baseadas no wireframe React
        private readonly Color mainBgColor = new Color(0.05f, 0.08f, 0.12f, 1f); // Semelhante ao WFBackground
        private readonly Color panelBgColor = new Color(0.02f, 0.03f, 0.06f, 0.5f);
        private readonly Color borderLight = new Color(1f, 1f, 1f, 0.06f);
        private readonly Color borderMedium = new Color(1f, 1f, 1f, 0.15f);
        
        private readonly Color textDim = new Color(1f, 1f, 1f, 0.22f);
        private readonly Color textMedium = new Color(1f, 1f, 1f, 0.4f);
        private readonly Color textBright = new Color(1f, 1f, 1f, 0.82f);

        void Start()
        {
            BuildUI();
            PositionInFrontOfPlayer();
        }

        void BuildUI()
        {
            rootUI = new GameObject("TutorialMenu");
            
            // Setup Canvas
            tutorialCanvas = rootUI.AddComponent<Canvas>();
            tutorialCanvas.renderMode = RenderMode.WorldSpace;
            tutorialCanvas.worldCamera = xrCamera;

            rootUI.AddComponent<CanvasScaler>();
            rootUI.AddComponent<GraphicRaycaster>();
            // rootUI.AddComponent<TrackedDeviceGraphicRaycaster>(); // Descomentar se for usar botões no VR

            var rt = rootUI.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1000, 600);
            rootUI.transform.localScale = Vector3.one * 0.001f;

            // Fundo Principal
            AddImage(rootUI, mainBgColor, Vector2.zero, new Vector2(1000, 600));

            BuildHeader();
            BuildLeftPanel();
            BuildRightPanel();
            BuildBottomPanel();
        }

        void BuildHeader()
        {
            var headerObj = new GameObject("Header");
            headerObj.transform.SetParent(rootUI.transform, false);
            var rt = headerObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 260); // Topo
            rt.sizeDelta = new Vector2(960, 60);

            // Linha separadora do cabeçalho
            AddImage(headerObj, borderLight, new Vector2(0, -30), new Vector2(1000, 1));

            // Esquerda: Título
            CreateTMPText(headerObj, "// MÓDULO TUTORIAL //", new Vector2(-360, 10), new Vector2(200, 20), 9f, textDim).characterSpacing = 5f;
            CreateTMPText(headerObj, "PROCEDIMENTO DE RCP", new Vector2(-360, -10), new Vector2(250, 20), 14f, textBright).characterSpacing = 2f;

            // Centro: Pontos/Dots de progresso (Espaço reservado mockado)
            var dotsObj = new GameObject("ProgressDots");
            dotsObj.transform.SetParent(headerObj.transform, false);
            CreateTMPText(dotsObj, "[ 1 ] -- 2 -- 3 -- 4 -- 5 -- 6", Vector2.zero, new Vector2(300, 20), 12f, textMedium)
                .alignment = TextAlignmentOptions.Center;

            // Direita: Contagem
            CreateTMPText(headerObj, "ETAPA", new Vector2(380, 10), new Vector2(160, 20), 9f, textDim)
                .alignment = TextAlignmentOptions.Right;
            stepCountText = CreateTMPText(headerObj, "1 / 6", new Vector2(380, -10), new Vector2(160, 20), 14f, textBright);
            stepCountText.alignment = TextAlignmentOptions.Right;
        }

        void BuildLeftPanel()
        {
            var leftObj = new GameObject("LeftPanel");
            leftObj.transform.SetParent(rootUI.transform, false);
            var rt = leftObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-350, 0);

            CreateTMPText(leftObj, "ETAPA DO PROCEDIMENTO", new Vector2(0, 50), new Vector2(240, 20), 9f, textDim).characterSpacing = 4f;
            
            stepNumberText = CreateTMPText(leftObj, "01", new Vector2(0, 0), new Vector2(200, 60), 50f, new Color(1f, 1f, 1f, 0.08f));
            
            stepLabelText = CreateTMPText(leftObj, "VERIFICAÇÃO DO LOCAL", new Vector2(0, -50), new Vector2(260, 20), 10f, new Color(1f, 1f, 1f, 0.35f));
            stepLabelText.characterSpacing = 3.5f;

            // Legenda Mock
            CreateTMPText(leftObj, "--- ZONA DE INTERAÇÃO", new Vector2(0, -120), new Vector2(240, 15), 8f, textDim);
            CreateTMPText(leftObj, "── CONTORNO DO CORPO", new Vector2(0, -140), new Vector2(240, 15), 8f, textDim);
            CreateTMPText(leftObj, "... GUIA DE SETA", new Vector2(0, -160), new Vector2(240, 15), 8f, textDim);
        }

        void BuildRightPanel()
        {
            var rightObj = new GameObject("RightPanel");
            rightObj.transform.SetParent(rootUI.transform, false);
            var rt = rightObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(350, 0);

            // Quadro VR Interaction
            var vrBox = new GameObject("VRHint");
            vrBox.transform.SetParent(rightObj.transform, false);
            var vrRt = vrBox.AddComponent<RectTransform>();
            vrRt.anchoredPosition = new Vector2(0, 40);
            
            var bg = AddImage(vrBox, panelBgColor, Vector2.zero, new Vector2(180, 60));
            var outline = bg.gameObject.AddComponent<Outline>();
            outline.effectColor = borderMedium; outline.effectDistance = new Vector2(1, -1);

            CreateTMPText(vrBox, "INTERAÇÃO VR", new Vector2(0, 15), new Vector2(160, 20), 8f, textDim)
                .alignment = TextAlignmentOptions.Left;
            CreateTMPText(vrBox, "OLHE PARA A ZONA DESTACADA PARA CONFIRMAR", new Vector2(0, -5), new Vector2(160, 30), 9f, textMedium)
                .alignment = TextAlignmentOptions.TopLeft;

            // Barra de Progresso
            CreateTMPText(rightObj, "PROGRESSO", new Vector2(-10, -50), new Vector2(160, 15), 8f, textDim);
            AddImage(rightObj, borderLight, new Vector2(0, -65), new Vector2(180, 6)); // bg da barra
            AddImage(rightObj, textMedium, new Vector2(-75, -65), new Vector2(30, 6)); // progresso mock (largura 30)
        }

        void BuildBottomPanel()
        {
            var bottomObj = new GameObject("BottomPanel");
            bottomObj.transform.SetParent(rootUI.transform, false);
            var rt = bottomObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -250); // Fundo
            rt.sizeDelta = new Vector2(960, 80);

            // Linha superior do rodapé
            AddImage(bottomObj, borderLight, new Vector2(0, 40), new Vector2(1000, 1));

            // Texto de Instrução
            instructionTitleText = CreateTMPText(bottomObj, "VERIFIQUE O LOCAL", new Vector2(-220, 15), new Vector2(500, 20), 14f, textBright);
            instructionTitleText.characterSpacing = 1.5f;

            instructionDescText = CreateTMPText(bottomObj, "Certifique-se de que a área é segura antes de se aproximar. Procure possíveis perigos. Confirme que o paciente está inconsciente e não responde.", 
                new Vector2(-220, -15), new Vector2(520, 40), 11f, textMedium);
            instructionDescText.alignment = TextAlignmentOptions.TopLeft;
            instructionDescText.lineSpacing = 15f;

            // Botões (Estáticos no momento, mas com mesma estrutura do RCPMenu)
            var btnPrev = BuildNavButton(bottomObj, "← ANTERIOR", new Vector2(220, 0), true);
            var btnNext = BuildNavButton(bottomObj, "PRÓXIMO →", new Vector2(380, 0), false);
        }

        GameObject BuildNavButton(GameObject parent, string text, Vector2 pos, bool isSecondary)
        {
            var btnObj = new GameObject($"Btn_{text}");
            btnObj.transform.SetParent(parent.transform, false);
            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(140, 40);

            var bgImg = AddImage(btnObj, isSecondary ? Color.clear : new Color(1f, 1f, 1f, 0.08f), Vector2.zero, new Vector2(140, 40));
            var outline = bgImg.gameObject.AddComponent<Outline>();
            outline.effectColor = isSecondary ? borderMedium : new Color(1f, 1f, 1f, 0.35f);
            outline.effectDistance = new Vector2(1, -1);

            var lbl = CreateTMPText(btnObj, text, Vector2.zero, new Vector2(140, 40), 11f, isSecondary ? textMedium : textBright);
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.characterSpacing = 3f;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bgImg;

            return btnObj;
        }

        // ── Helpers Utilitários (Mesmos usados no RCPMenuBuilder) ─────────────────────

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

        TextMeshProUGUI CreateTMPText(GameObject parent, string text, Vector2 pos, Vector2 size, float fontSize, Color color)
        {
            var go = new GameObject($"Text_{text.Substring(0, Mathf.Min(6, text.Length))}");
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;

            if (tutorialFont != null) tmp.font = tutorialFont;

            return tmp;
        }

        void PositionInFrontOfPlayer()
        {
            if (xrCamera == null) xrCamera = Camera.main;
            Transform cam = xrCamera.transform;
            
            rootUI.transform.position = cam.position + cam.forward * 2.2f;
            
            rootUI.transform.LookAt(cam.position);
            rootUI.transform.Rotate(0, 180, 0);
        }
    }
}
