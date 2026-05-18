using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Equivalente ao componente ArrowAnnotation do Tutorial.tsx.
/// Gere um pool de anotações (ponto + linha tracejada + label) no World Space Canvas.
///
/// ── Setup ──────────────────────────────────────────────────────────────
///  1. Coloca este script num GameObject filho do Canvas ("AnnotationLayer").
///  2. Cria um prefab AnnotationItem com a estrutura abaixo e liga em annotationPrefab.
///  3. Liga a RectTransform do painel do paciente em patientRect
///     (usado para converter xNorm/yNorm em posição de ecrã).
///
/// ── Estrutura do prefab AnnotationItem ────────────────────────────────
///   AnnotationItem (RectTransform)
///   ├── Dot         (Image circular pequena, ~6px)
///   ├── Line        (Image 1px altura, pivot à esquerda — esticada via width)
///   └── Label       (TextMeshProUGUI)
/// </summary>
public class TutorialAnnotationManager : MonoBehaviour
{
    [Header("Referências")]
    public RectTransform    patientRect;      // área do paciente no Canvas
    public GameObject       annotationPrefab; // prefab de uma anotação

    [Header("Aparência")]
    public float lineLength     = 80f;        // equivalente a lineLength = 48 do SVG (ajustado para VR)
    public float fadeDuration   = 0.4f;

    // Pool
    private List<AnnotationItem> pool = new List<AnnotationItem>();

    // ── API pública ────────────────────────────────────────────────────
    /// <summary>Mostra as anotações do passo actual, esconde as anteriores.</summary>
    public void ShowAnnotations(Annotation[] annotations)
    {
        // Desactiva todas
        foreach (var item in pool)
            item.gameObject.SetActive(false);

        // Activa / expande o pool conforme necessário
        for (int i = 0; i < annotations.Length; i++)
        {
            if (i >= pool.Count)
                pool.Add(CreateItem());

            AnnotationItem item = pool[i];
            item.gameObject.SetActive(true);
            Apply(item, annotations[i]);
            StartCoroutine(FadeIn(item));
        }
    }

    // ── Internos ───────────────────────────────────────────────────────
    private void Apply(AnnotationItem item, Annotation ann)
    {
        // Converte xNorm / yNorm para posição local no Canvas
        Vector2 rectSize = patientRect.rect.size;
        float   px       = (ann.xNorm - 0.5f) * rectSize.x;
        float   py       = (0.5f - ann.yNorm) * rectSize.y; // Y invertido (UI)

        item.rt.anchoredPosition = new Vector2(px, py);

        // Label
        item.label.text = ann.text;

        // Posição e rotação da linha + label consoante a direcção
        float lineSign = (ann.dir == AnnotationDir.Left || ann.dir == AnnotationDir.Up) ? -1f : 1f;
        bool  horizontal = ann.dir == AnnotationDir.Left || ann.dir == AnnotationDir.Right;

        if (horizontal)
        {
            item.line.sizeDelta = new Vector2(lineLength, 1f);
            item.line.localRotation = Quaternion.identity;
            item.line.anchoredPosition = new Vector2(lineSign * lineLength * 0.5f, 0f);

            item.label.rectTransform.anchoredPosition = new Vector2(lineSign * (lineLength + 8f), 0f);
            item.label.alignment = ann.dir == AnnotationDir.Left
                ? TextAlignmentOptions.Right
                : TextAlignmentOptions.Left;
        }
        else
        {
            item.line.sizeDelta = new Vector2(1f, lineLength);
            item.line.localRotation = Quaternion.identity;
            item.line.anchoredPosition = new Vector2(0f, lineSign * lineLength * 0.5f);

            item.label.rectTransform.anchoredPosition = new Vector2(0f, lineSign * (lineLength + 14f));
            item.label.alignment = TextAlignmentOptions.Center;
        }
    }

    private IEnumerator FadeIn(AnnotationItem item)
    {
        CanvasGroup cg = item.GetComponent<CanvasGroup>();
        if (cg == null) cg = item.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private AnnotationItem CreateItem()
    {
        GameObject go   = Instantiate(annotationPrefab, transform);
        var        item = go.GetComponent<AnnotationItem>();
        if (item == null)
        {
            Debug.LogError("[TutorialAnnotationManager] O prefab não tem o componente AnnotationItem.");
        }
        return item;
    }
}

/// <summary>
/// Componente auxiliar que deve estar no prefab de anotação.
/// Liga os sub-elementos via Inspector.
/// </summary>
public class AnnotationItem : MonoBehaviour
{
    [HideInInspector] public RectTransform    rt;
    public                   RectTransform    line;   // Image de 1px
    public                   Image            dot;    // círculo central
    public                   TextMeshProUGUI  label;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        // Animação de pulso no dot (equivalente ao animate scale do React)
        StartCoroutine(PulseDot());
    }

    private IEnumerator PulseDot()
    {
        float period = 1.8f; // mesma duração do React
        while (true)
        {
            float t = 0f;
            while (t < period)
            {
                t += Time.deltaTime;
                // scale: 1 → 1.3 → 1
                float s = 1f + 0.3f * Mathf.Sin((t / period) * Mathf.PI);
                dot.transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }
    }
}
