using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class CPRCompressionPulseDetector : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z,
    }

    [Serializable]
    public class CompressionPulseEvent : UnityEvent<CPRCompressionPulseDetector>
    {
    }

    [Header("References")]
    [SerializeField] private Transform compressionSource;
    [SerializeField] private Transform referenceSpace;
    [SerializeField] private CPRHandPlacementDetector placementDetector;

    [Header("Compression")]
    [SerializeField] private Axis compressionAxis = Axis.Y;
    [SerializeField] private bool invertCompressionDirection;
    [SerializeField] private float pressDepth = 0.05f;
    [SerializeField] private float releaseDepth = 0.02f;
    [SerializeField] private float maxCompressionDepth = 0.08f;

    [Header("Events")]
    [SerializeField] private UnityEvent onCompressionPressed;
    [SerializeField] private UnityEvent onCompressionReleased;
    [SerializeField] private CompressionPulseEvent onCompressionPressedDetailed;

    public event Action<CPRCompressionPulseDetector> CompressionPressed;
    public event Action<CPRCompressionPulseDetector> CompressionReleased;

    public bool IsCompressionActive { get; private set; }
    public float CompressionProgress { get; private set; }

    private float restAxisValue;
    private bool hasRestAxisValue;

    private void Awake()
    {
        if (referenceSpace == null)
            referenceSpace = transform;

        if (compressionSource == null)
            compressionSource = transform;
    }

    private void OnEnable()
    {
        ResetDetector();
    }

    private void Update()
    {
        if (compressionSource == null || referenceSpace == null)
            return;

        if (placementDetector != null && !placementDetector.IsAligned)
        {
            IsCompressionActive = false;
            hasRestAxisValue = false;
            return;
        }

        if (!hasRestAxisValue)
            CaptureRestAxisValue();

        float currentDepth = GetCompressionDepth();
        CompressionProgress = Mathf.Clamp01(currentDepth / Mathf.Max(0.0001f, maxCompressionDepth));

        if (!IsCompressionActive)
        {
            if (currentDepth < pressDepth)
                return;

            IsCompressionActive = true;
            CompressionPressed?.Invoke(this);
            onCompressionPressed?.Invoke();
            onCompressionPressedDetailed?.Invoke(this);
            return;
        }

        if (currentDepth <= releaseDepth)
        {
            IsCompressionActive = false;
            CompressionReleased?.Invoke(this);
            onCompressionReleased?.Invoke();
        }
    }

    public void ResetDetector()
    {
        IsCompressionActive = false;
        CompressionProgress = 0f;
        hasRestAxisValue = false;

        if (placementDetector == null || placementDetector.IsAligned)
            CaptureRestAxisValue();
    }

    public float GetCompressionDepth()
    {
        if (compressionSource == null || referenceSpace == null)
            return 0f;

        float currentAxisValue = GetAxisValue(referenceSpace.InverseTransformPoint(compressionSource.position));
        float depth = invertCompressionDirection ? currentAxisValue - restAxisValue : restAxisValue - currentAxisValue;
        return Mathf.Max(0f, depth);
    }

    private void CaptureRestAxisValue()
    {
        if (compressionSource == null || referenceSpace == null)
            return;

        restAxisValue = GetAxisValue(referenceSpace.InverseTransformPoint(compressionSource.position));
        hasRestAxisValue = true;
    }

    private float GetAxisValue(Vector3 localPosition)
    {
        switch (compressionAxis)
        {
            case Axis.X:
                return localPosition.x;
            case Axis.Y:
                return localPosition.y;
            case Axis.Z:
                return localPosition.z;
            default:
                return localPosition.y;
        }
    }
}