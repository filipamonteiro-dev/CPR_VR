using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

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
            menuRoot.AddComponent<TrackedDeviceGraphicRaycaster>(); // <-- Descomentado para o VR funcionar no Menu
            menuRoot.AddComponent<CanvasScaler>();
            menuRoot.AddComponent<GraphicRaycaster>();

            var rt = menuRoot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 820);
            menuRoot.transform.localScale = Vector3.one * 0.001f;
            
            // Fundo geral preto do Canvas (o "Space" à volta do menu)
            AddImage(menuRoot, new Color(0, 0, 0, 1), Vector2.zero, new Vector2(1195, 682));

            // --- HEADER ---
            BuildHeader();

            // --- BOTÕES ---
            // Verifica se o tutorial foi completado
            bool tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;

            // Treino e Teste dependem do tutorial. Se o tutorial não for concluído, estão bloqueados.
            bool lockTraining = !tutorialCompleted;
            bool lockTest = !tutorialCompleted; 

            // Tutorial é sempre desbloqueado
            BuildButton("TUTORIAL", "APRENDA OS PASSOS BÁSICOS", 80, true, false, false, () => SceneManager.LoadScene("TutorialScene"));
            
            // Treino
            System.Action trainingAction = lockTraining ? (System.Action)null : () => SceneManager.LoadScene("TrainingScene");
            BuildButton("TREINO", "PRATIQUE COM ASSISTÊNCIA", 0, false, false, lockTraining, trainingAction);
            
            // Teste
            System.Action testAction = lockTest ? (System.Action)null : () => SceneManager.LoadScene("TestModeScene");
            BuildButton("TESTE", "AVALIAÇÃO SEM AUXÍLIO", -80, false, false, lockTest, testAction);
            
            // Sair
            BuildButton("SAIR DO SISTEMA", "ENCERRAR SIMULAÇÃO", -160, false, true, false, QuitApplication);

            // --- NOTA DE RODAPÉ ---
            var footer = CreateTMPText(menuRoot, "USE O GATILHO DO CONTROLE PARA SELECIONAR",
                new Vector2(0, -320), new Vector2(400, 20), 10f, new Color(1f, 1f, 1f, 0.15f));
            footer.alignment = TextAlignmentOptions.Center;
            footer.characterSpacing = 3f;
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

        void BuildButton(string label, string subtitle, float posY, bool isAccent, bool isDanger, bool isLocked, System.Action onClick)
        {
            Color currentBorderColor = isDanger ? dangerBorderColor : (isAccent ? accentBorderColor : borderColor);
            Color hoverColor = isDanger ? new Color(1f, 0.31f, 0.31f, 0.08f) : (isAccent ? new Color(1f, 1f, 1f, 0.08f) : new Color(1f, 1f, 1f, 0.04f));
            Color pressedColor = isDanger ? new Color(1f, 0.31f, 0.31f, 0.15f) : (isAccent ? new Color(1f, 1f, 1f, 0.15f) : new Color(1f, 1f, 1f, 0.1f));

            var btnObj = new GameObject($"Button_{label}");
            btnObj.transform.SetParent(menuRoot.transform, false);

            float btnWidth = 440f;
            float btnHeight = 70f;     // <-- Altura reduzida (era 95)
            float halfHeight = 35f;    // btnHeight / 2

            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, posY);
            rt.sizeDelta = new Vector2(btnWidth, btnHeight);

            // Hitbox invisível do botão (responsável por receber o Hover e Clique da Unity)
            var bgHitbox = AddImage(btnObj, Color.clear, Vector2.zero, new Vector2(btnWidth, btnHeight));

            // Borda contínua interna
            // Definimos as margens um pouco menores para garantir que fica por dentro dos cantos (ex: -6px)
            var innerBorder = AddImage(btnObj, Color.clear, Vector2.zero, new Vector2(btnWidth - 8f, btnHeight - 8f));
            var outline = innerBorder.gameObject.AddComponent<Outline>();
            outline.effectColor = currentBorderColor;
            outline.effectDistance = new Vector2(1, -1); // Linha contínua

            // Adicionando os cantos acentuados usando as medidas do halfHeight
            AddCorner(btnObj, new Vector2(-220, halfHeight), new Vector2(8, 2), currentBorderColor, new Vector2(4, -1));
            AddCorner(btnObj, new Vector2(-220, halfHeight), new Vector2(2, 8), currentBorderColor, new Vector2(1, -4));
            AddCorner(btnObj, new Vector2(220, halfHeight), new Vector2(8, 2), currentBorderColor, new Vector2(-4, -1));
            AddCorner(btnObj, new Vector2(220, halfHeight), new Vector2(2, 8), currentBorderColor, new Vector2(-1, -4));
            AddCorner(btnObj, new Vector2(-220, -halfHeight), new Vector2(8, 2), currentBorderColor, new Vector2(4, 1));
            AddCorner(btnObj, new Vector2(-220, -halfHeight), new Vector2(2, 8), currentBorderColor, new Vector2(1, 4));
            AddCorner(btnObj, new Vector2(220, -halfHeight), new Vector2(8, 2), currentBorderColor, new Vector2(-4, 1));
            AddCorner(btnObj, new Vector2(220, -halfHeight), new Vector2(2, 8), currentBorderColor, new Vector2(-1, 4));

            // Botão Unity
            var btn = btnObj.AddComponent<Button>();
            
            // Passar o objeto que a Unity vai considerar como "Target Graphic" (A Hitbox)
            btn.targetGraphic = bgHitbox;

            var colors = btn.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = pressedColor;
            colors.selectedColor = hoverColor;
            btn.colors = colors;

            // ----> AQUI ADICIONAMOS A LÓGICA DE HOVER <----
            if (!isLocked)
            {
                var trigger = btnObj.AddComponent<EventTrigger>();

                // Aumentar no Hover (nota: aumentei a escala para 1.05f (5%) pois 0.1% é quase impercetível em VR)
                var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                pointerEnter.callback.AddListener((data) => { btnObj.transform.localScale = Vector3.one * 1.05f; });
                trigger.triggers.Add(pointerEnter);

                // Voltar ao normal quando sai do Hover
                var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                pointerExit.callback.AddListener((data) => { btnObj.transform.localScale = Vector3.one; });
                trigger.triggers.Add(pointerExit);
            }

            // ----> AQUI CONECTAMOS O EVENTO DE CLIQUE AO BOTÃO <----
            if (onClick != null)
            {
                btn.onClick.AddListener(() => onClick.Invoke());
            }

            // Labels e resto do botão... (Ajustei os Y dos textos pela altura nova)
            Color labelColor = isDanger ? new Color(1f, 0.39f, 0.39f, 0.8f) : (isAccent ? highlightTextColor : new Color(1f, 1f, 1f, 0.65f));
            var lbl = CreateTMPText(btnObj, label,
                new Vector2(-30, 8), new Vector2(300, 30), 14f, labelColor); // PosY antes 10, agora 8
            lbl.characterSpacing = 3f;

            CreateTMPText(btnObj, subtitle,
                new Vector2(-30, -12), new Vector2(320, 22), 10f, subTextColor); // PosY antes -14, agora -12

            Color selColor = isDanger ? selectTextDangerColor : selectTextAccentColor;
            var sel = CreateTMPText(btnObj, "[SELECIONAR]",
                new Vector2(140, 0), new Vector2(130, 30), 10f, selColor);
            sel.alignment = TextAlignmentOptions.Right;
            sel.characterSpacing = 2f;

            // Texto de bloqueado
            if (isLocked)
            {
                CreateTMPText(btnObj, "BLOQUEADO",
                    new Vector2(0, -12), new Vector2(400, 22), 10f, new Color(1f, 0.2f, 0.2f, 0.85f))
                    .alignment = TextAlignmentOptions.Center;
            }

            // Escurecer o botão se estiver bloqueado
            if (isLocked)
            {
                AddImage(btnObj, new Color(0f, 0f, 0f, 0.65f), Vector2.zero, new Vector2(btnWidth, btnHeight));
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