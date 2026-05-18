using UnityEngine;

/// <summary>
/// Equivalente à interface Step e ao array STEPS do Tutorial.tsx.
/// Dados puros — sem dependências de MonoBehaviour.
/// </summary>

public enum HighlightMode { Full, Chest, Hands, Head, None }
public enum AnnotationDir  { Left, Right, Up, Down }

[System.Serializable]
public struct Annotation
{
    [Tooltip("Posição horizontal normalizada 0–1 (ex: '15%' → 0.15)")]
    public float  xNorm;
    [Tooltip("Posição vertical normalizada 0–1 (ex: '30%' → 0.30)")]
    public float  yNorm;
    public string text;
    public AnnotationDir dir;
}

[System.Serializable]
public class TutorialStep
{
    public int           id;
    public string        label;
    public string        title;
    [TextArea(2, 4)]
    public string        instruction;
    public HighlightMode highlight;
    public bool          showArrow;
    public bool          showHandPlacement;
    public Annotation[]  annotations;
}

/// <summary>
/// Equivalente ao array STEPS[] hardcoded no Tutorial.tsx.
/// Acedido via TutorialStepData.All.
/// </summary>
public static class TutorialStepData
{
    public static readonly TutorialStep[] All = new TutorialStep[]
    {
        new TutorialStep {
            id = 1,
            label = "VERIFICAÇÃO DO LOCAL",
            title = "VERIFIQUE O LOCAL",
            instruction = "Certifique-se de que a área é segura antes de se aproximar. Procure possíveis perigos. Confirme que o paciente está inconsciente e não responde.",
            highlight = HighlightMode.Full,
            showArrow = false,
            showHandPlacement = false,
            annotations = new Annotation[] {
                new Annotation { xNorm = 0.15f, yNorm = 0.30f, text = "VERIFICAR AMBIENTE", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.75f, yNorm = 0.55f, text = "CHECAR RESPOSTA",    dir = AnnotationDir.Left  },
            }
        },
        new TutorialStep {
            id = 2,
            label = "PEDIR AJUDA",
            title = "LIGAR PARA EMERGÊNCIA",
            instruction = "Ligue para o SAMU (192) ou instrua alguém a ligar. Solicite um DEA se disponível. Posicione o paciente em decúbito dorsal sobre uma superfície firme.",
            highlight = HighlightMode.None,
            showArrow = false,
            showHandPlacement = false,
            annotations = new Annotation[] {
                new Annotation { xNorm = 0.18f, yNorm = 0.45f, text = "SUPERFÍCIE FIRME", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.72f, yNorm = 0.30f, text = "LIGUE 192",        dir = AnnotationDir.Left  },
            }
        },
        new TutorialStep {
            id = 3,
            label = "LOCAL DO TÓRAX",
            title = "LOCALIZAR ZONA DE COMPRESSÃO",
            instruction = "Encontre a metade inferior do esterno. Coloque o calcanhar de uma mão no centro do peito do paciente, entre os mamilos.",
            highlight = HighlightMode.Chest,
            showArrow = false,
            showHandPlacement = false,
            annotations = new Annotation[] {
                new Annotation { xNorm = 0.16f, yNorm = 0.42f, text = "PARTE INF. DO ESTERNO", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.70f, yNorm = 0.42f, text = "CENTRO DO PEITO",       dir = AnnotationDir.Left  },
            }
        },
        new TutorialStep {
            id = 4,
            label = "POSIÇÃO DAS MÃOS",
            title = "POSICIONE SUAS MÃOS",
            instruction = "Coloque o calcanhar de uma mão na zona de compressão. Sobreponha a outra mão por cima. Entrelace os dedos e mantenha-os levantados sem tocar o tórax.",
            highlight = HighlightMode.Chest,
            showArrow = false,
            showHandPlacement = true,
            annotations = new Annotation[] {
                new Annotation { xNorm = 0.14f, yNorm = 0.38f, text = "CALCANHAR DA MÃO", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.68f, yNorm = 0.38f, text = "DEDOS LEVANTADOS", dir = AnnotationDir.Left  },
            }
        },
        new TutorialStep {
            id = 5,
            label = "COMPRESSÕES",
            title = "INICIAR COMPRESSÕES",
            instruction = "Pressione forte e rápido — pelo menos 5cm de profundidade a 100–110 BPM. Permita o retorno completo do tórax entre cada compressão.",
            highlight = HighlightMode.Chest,
            showArrow = true,
            showHandPlacement = true,
            annotations = new Annotation[] {
                new Annotation { xNorm = 0.14f, yNorm = 0.30f, text = "PRESSIONAR 5–6cm", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.68f, yNorm = 0.30f, text = "100–110 BPM",      dir = AnnotationDir.Left  },
            }
        },
        new TutorialStep {
            id = 6,
            label = "CICLO CONTÍNUO",
            title = "MANTER O CICLO",
            instruction = "Realize 30 compressões seguidas de 2 ventilações de resgate. Continue o ciclo até a chegada do DEA ou de socorro especializado.",
            highlight = HighlightMode.Chest,
            showArrow = true,
            showHandPlacement = true,
            annotations = new Annotation[] {
                new Annotation { xNorm = 0.14f, yNorm = 0.35f, text = "30 COMPRESSÕES", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.68f, yNorm = 0.35f, text = "2 VENTILAÇÕES",  dir = AnnotationDir.Left  },
            }
        },
    };
}
