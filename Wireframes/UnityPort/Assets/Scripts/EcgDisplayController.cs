using System.Collections.Generic;
using UnityEngine;

namespace VrCpr
{
    [RequireComponent(typeof(LineRenderer))]
    public class EcgDisplayController : MonoBehaviour
    {
        [SerializeField] private float bpm = 102f;
        [SerializeField] private int sampleCount = 200;
        [SerializeField] private float width = 2.8f;
        [SerializeField] private float height = 0.64f;

        private readonly List<float> samples = new List<float>();
        private LineRenderer lineRenderer;
        private float offset;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = sampleCount;
            lineRenderer.useWorldSpace = false;

            samples.Clear();
            for (int i = 0; i < sampleCount; i += 1)
            {
                samples.Add(0f);
            }
        }

        private void Update()
        {
            float speed = bpm / 60f * 1.2f;
            offset += Time.deltaTime * speed;

            samples.Add(GenerateSegment(offset));
            if (samples.Count > sampleCount)
            {
                samples.RemoveAt(0);
            }

            for (int i = 0; i < samples.Count; i += 1)
            {
                float x = Mathf.Lerp(-width * 0.5f, width * 0.5f, i / (float)(samples.Count - 1));
                float y = samples[i] * height * 0.5f;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }
        }

        public void SetBpm(float value)
        {
            bpm = value;
        }

        private static float GenerateSegment(float currentOffset)
        {
            float p = currentOffset % 100f;

            if (p < 15f)
            {
                return 0f;
            }

            if (p < 20f)
            {
                return Mathf.Sin(((p - 15f) / 5f) * Mathf.PI) * 0.35f;
            }

            if (p < 30f)
            {
                return 0f;
            }

            if (p < 33f)
            {
                return -0.22f;
            }

            if (p < 37f)
            {
                return 1.0f;
            }

            if (p < 40f)
            {
                return -0.26f;
            }

            if (p < 50f)
            {
                return Mathf.Sin(((p - 40f) / 10f) * Mathf.PI) * 0.45f;
            }

            return 0f;
        }
    }
}