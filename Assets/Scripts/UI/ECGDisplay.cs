using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Equivalente ao componente ECGDisplay do React.
/// Desenha a onda ECG num RawImage via Texture2D.
///
/// Setup:
///   1. Cria um RawImage (300x64px) no Canvas.
///   2. Adiciona este script ao mesmo GameObject.
///   3. O script cria e actualiza a textura automaticamente.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class ECGDisplay : MonoBehaviour
{
    [Header("Configuração")]
    public int textureWidth  = 280;
    public int textureHeight = 64;

    private RawImage   rawImage;
    private Texture2D  tex;
    private Color[]    pixels;

    private float   offset  = 0f;
    private float   speed   = 1f;
    private float[] data;

    // Cores (equivalente ao gradient do canvas)
    private static readonly Color gridColor     = new Color(1f, 1f, 1f, 0.05f);
    private static readonly Color baselineColor = new Color(1f, 1f, 1f, 0.08f);

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        data     = new float[textureWidth];

        tex    = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        pixels = new Color[textureWidth * textureHeight];

        rawImage.texture = tex;
    }

    /// <summary>Chamado pelo TrainingManager quando o BPM muda.</summary>
    public void SetBpm(int bpm)
    {
        speed = (bpm / 60f) * 1.2f;
    }

    void Update()
    {
        offset += speed * 0.4f * Time.deltaTime * 60f; // normaliza para ~60fps original

        // Shift do buffer e novo valor
        float newVal = GenerateECGSegment(offset);
        System.Array.Copy(data, 1, data, 0, data.Length - 1);
        data[data.Length - 1] = newVal;

        DrawFrame();
    }

    private void DrawFrame()
    {
        int w = textureWidth;
        int h = textureHeight;

        // Limpa
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Grade vertical (cada 20px)
        for (int x = 0; x < w; x += 20)
            for (int y = 0; y < h; y++)
                BlendPixel(x, y, gridColor);

        // Grade horizontal (cada 12px)
        for (int y = 0; y < h; y += 12)
            for (int x = 0; x < w; x++)
                BlendPixel(x, y, gridColor);

        // Baseline a meio
        int mid = h / 2;
        for (int x = 0; x < w; x += 8)           // dash [4,4]
            for (int d = 0; d < 4 && x + d < w; d++)
                BlendPixel(x + d, mid, baselineColor);

        // Linha ECG com gradiente de alpha (esquerda=0 → direita=0.92)
        for (int i = 1; i < data.Length; i++)
        {
            float t     = i / (float)data.Length;           // 0..1
            float alpha = t < 0.4f ? t / 0.4f * 0.5f       // 0→0.5 até 40%
                                   : 0.5f + (t - 0.4f) / 0.6f * 0.42f; // 0.5→0.92

            int y0 = Mathf.Clamp(mid - Mathf.RoundToInt(data[i - 1] * 1.8f), 0, h - 1);
            int y1 = Mathf.Clamp(mid - Mathf.RoundToInt(data[i]     * 1.8f), 0, h - 1);
            int x0 = i - 1;
            int x1 = i;

            Color lineColor = new Color(1f, 1f, 1f, alpha);
            DrawLine(x0, y0, x1, y1, lineColor);
        }

        tex.SetPixels(pixels);
        tex.Apply();
    }

    // Bresenham simples
    private void DrawLine(int x0, int y0, int x1, int y1, Color c)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            SetPixel(x0, y0, c);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 <  dx) { err += dx; y0 += sy; }
        }
    }

    private void SetPixel(int x, int y, Color c)
    {
        if (x < 0 || x >= textureWidth || y < 0 || y >= textureHeight) return;
        pixels[y * textureWidth + x] = c;
    }

    private void BlendPixel(int x, int y, Color src)
    {
        if (x < 0 || x >= textureWidth || y < 0 || y >= textureHeight) return;
        int idx = y * textureWidth + x;
        Color dst = pixels[idx];
        float a = src.a + dst.a * (1f - src.a);
        if (a < 0.0001f) { pixels[idx] = Color.clear; return; }
        pixels[idx] = new Color(
            (src.r * src.a + dst.r * dst.a * (1f - src.a)) / a,
            (src.g * src.a + dst.g * dst.a * (1f - src.a)) / a,
            (src.b * src.a + dst.b * dst.a * (1f - src.a)) / a,
            a);
    }

    /// <summary>Exactamente o mesmo algoritmo do React.</summary>
    private float GenerateECGSegment(float rawOffset)
    {
        float p = ((rawOffset % 100f) + 100f) % 100f;
        if (p < 15f) return 0f;
        if (p < 20f) return Mathf.Sin(((p - 15f) / 5f) * Mathf.PI) * 6f;
        if (p < 30f) return 0f;
        if (p < 33f) return -4f;
        if (p < 37f) return 28f;
        if (p < 40f) return -6f;
        if (p < 50f) return Mathf.Sin(((p - 40f) / 10f) * Mathf.PI) * 9f;
        return 0f;
    }

    void OnDestroy()
    {
        if (tex != null) Destroy(tex);
    }
}
