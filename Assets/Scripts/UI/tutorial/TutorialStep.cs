using UnityEngine;

public enum HighlightMode
{
    Full,
    Chest,
    Hands,
    Head,
    None
}

public enum AnnotationDir
{
    Left,
    Right,
    Up,
    Down
}

[System.Serializable]
public struct Annotation
{
    public float xNorm;
    public float yNorm;
    public string text;
    public AnnotationDir dir;
}

[System.Serializable]
public class TutorialStep
{
    public int id;
    public string label;
    public string title;

    [TextArea(2, 5)]
    public string instruction;

    public HighlightMode highlight;
    public bool showArrow;
    public bool showHandPlacement;
    public Annotation[] annotations;
}

public static class TutorialStepsCatalog
{
    public static readonly TutorialStep[] All =
    {
        new TutorialStep
        {
            id = 1,
            label = "VERIFICACAO DO LOCAL",
            title = "VERIFIQUE O LOCAL",
            instruction = "Certifique-se de que a area e segura antes de se aproximar. Procure perigos e confirme que o paciente nao responde.",
            highlight = HighlightMode.Full,
            showArrow = false,
            showHandPlacement = false,
            annotations = new[]
            {
                new Annotation { xNorm = 0.15f, yNorm = 0.30f, text = "VERIFICAR AMBIENTE", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.75f, yNorm = 0.55f, text = "CHECAR RESPOSTA", dir = AnnotationDir.Left }
            }
        },
        new TutorialStep
        {
            id = 2,
            label = "PEDIR AJUDA",
            title = "LIGAR PARA EMERGENCIA",
            instruction = "Ligue para o 112 ",
            highlight = HighlightMode.None,
            showArrow = false,
            showHandPlacement = false,
            annotations = new[]
            {
                new Annotation { xNorm = 0.18f, yNorm = 0.45f, text = "Pegue no telefone no seu cinto", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.72f, yNorm = 0.30f, text = "LIGUE 112", dir = AnnotationDir.Left }
            }
        },
        new TutorialStep
        {
            id = 3,
            label = "LOCAL DO TORAX",
            title = "LOCALIZAR ZONA DE COMPRESSAO",
            instruction = "Encontre a metade inferior do esterno e posicione a base da mao no centro do peito.",
            highlight = HighlightMode.Chest,
            showArrow = false,
            showHandPlacement = false,
            annotations = new[]
            {
                new Annotation { xNorm = 0.16f, yNorm = 0.42f, text = "PARTE INF. DO ESTERNO", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.70f, yNorm = 0.42f, text = "CENTRO DO PEITO", dir = AnnotationDir.Left }
            }
        },
        new TutorialStep
        {
            id = 4,
            label = "POSICAO DAS MAOS",
            title = "POSICIONE SUAS MAOS",
            instruction = "Sobreponha as maos, entrelace os dedos e mantenha os dedos sem tocar o torax.",
            highlight = HighlightMode.Chest,
            showArrow = false,
            showHandPlacement = true,
            annotations = new[]
            {
                new Annotation { xNorm = 0.14f, yNorm = 0.38f, text = "CALCANHAR DA MAO", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.68f, yNorm = 0.38f, text = "DEDOS LEVANTADOS", dir = AnnotationDir.Left }
            }
        },
        new TutorialStep
        {
            id = 5,
            label = "COMPRESSOES",
            title = "INICIAR COMPRESSOES",
            instruction = "Pressione forte e rapido, com retorno completo do torax entre cada compressao.",
            highlight = HighlightMode.Chest,
            showArrow = true,
            showHandPlacement = true,
            annotations = new[]
            {
                new Annotation { xNorm = 0.14f, yNorm = 0.30f, text = "PRESSIONAR 5-6cm", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.68f, yNorm = 0.30f, text = "100-110 BPM", dir = AnnotationDir.Left }
            }
        },
        new TutorialStep
        {
            id = 6,
            label = "CICLO CONTINUO",
            title = "MANTER O CICLO",
            instruction = "Realize 30 compressoes e 2 ventilacoes ate a chegada do suporte avancado.",
            highlight = HighlightMode.Chest,
            showArrow = true,
            showHandPlacement = true,
            annotations = new[]
            {
                new Annotation { xNorm = 0.14f, yNorm = 0.35f, text = "30 COMPRESSOES", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.68f, yNorm = 0.35f, text = "2 VENTILACOES", dir = AnnotationDir.Left }
            }
        }
    };
}
