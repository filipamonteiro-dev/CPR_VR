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
            label = "APROXIME-SE",
            title = "MOVIMENTE-SE PARA PERTO DO PACIENTE",
            instruction = "Use o comando e teleporte-se para o circulo no chão.",
            highlight = HighlightMode.Full,
            showArrow = false,
            showHandPlacement = false,
            annotations = new[]
            {
                new Annotation { xNorm = 0.15f, yNorm = 0.30f, text = "USE O TELEPORTE", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.75f, yNorm = 0.55f, text = "MOVIMENTE-SE ATÉ AO PACIENTE", dir = AnnotationDir.Left }
            }
        },
        new TutorialStep
        {
            id = 2,
            label = "VERIFICAÇÃO DO LOCAL",
            title = "VERIFIQUE O LOCAL",
            instruction = "Certifique-se de que a area é segura olhando para os lados.",
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
            id = 3,
            label = "VERIFICAR CONSCIENCIA",
            title = "AGITE OS OMBROS DO PACIENTE",
            instruction = "Pegue em um dos ombros do paciente e abane-o.",
            highlight = HighlightMode.None,
            showArrow = false,
            showHandPlacement = false,
            annotations = new[]
            {
                new Annotation { xNorm = 0.18f, yNorm = 0.45f, text = "AGITE O PACIENTE", dir = AnnotationDir.Right }
          
            }
        },
            new TutorialStep
        {
            id = 4,
            label = " LIGUE PARA O 112",
            title = "CHAME UMA AMBULÂNCIA",
            instruction = "Aponte o comando ao telefone no seu cinto, clique no botão grip do comando e marque 1-1-2",
            highlight = HighlightMode.None,
            showArrow = false,
            showHandPlacement = false,
            annotations = new[]
            {
                new Annotation { xNorm = 0.18f, yNorm = 0.45f, text = "PARA MARCAR USE O GATILHO", dir = AnnotationDir.Right }
          
            }
        },
        new TutorialStep
        {
            id = 5,
            label = "RESPIRAÇÃO",
            title = "VERIFICAR RESPIRAÇÃO",
            instruction = "Incline a cabeca do paciente para trás e verifique se o paciente respira.",
            highlight = HighlightMode.Chest,
            showArrow = false,
            showHandPlacement = false,
            annotations = new[]
            {
                new Annotation { xNorm = 0.16f, yNorm = 0.42f, text = "PEGUE NO QUEIXO", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.70f, yNorm = 0.42f, text = "PRESSIONE O GRIP DO COMANDO", dir = AnnotationDir.Left }
            }
        },
       
        new TutorialStep
        {
            id = 6,
            label = "COMPRESSOES",
            title = "INICIAR COMPRESSOES",
            instruction = "Posicione o comando por cima do torax do paciente até ele ficar preso e realize 30 compressões.",
            highlight = HighlightMode.Chest,
            showArrow = true,
            showHandPlacement = true,
            annotations = new[]
            {
                new Annotation { xNorm = 0.14f, yNorm = 0.30f, text = "PRESSIONAR 5-6cm", dir = AnnotationDir.Right },
                new Annotation { xNorm = 0.68f, yNorm = 0.30f, text = "100-110 BPM", dir = AnnotationDir.Left }
            }
        },
        
    };
}
