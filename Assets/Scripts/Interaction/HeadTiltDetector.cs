using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class HeadTiltDetector : MonoBehaviour
{
    public enum TiltAxis
    {
        X,
        Y,
        Z,
    }

    [Serializable]
    public class TiltValidatedEvent : UnityEvent<HeadTiltDetector>
    {
    }

    public static event Action<HeadTiltDetector> AnyTiltValidated;

    [Header("References")]
    [SerializeField] private XRGrabPointNotifier headGrabPoint;
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform referenceTransform;

    [Header("Tilt")]
    [SerializeField] private TiltAxis tiltAxis = TiltAxis.X;
    [SerializeField] private float requiredTiltDegrees = 20f;
    [SerializeField] private bool requireHeadGrab = true;
    [SerializeField] private bool oneShotUntilReset = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onTiltValidated;
    [SerializeField] private TiltValidatedEvent onTiltValidatedDetailed;

    public event Action<HeadTiltDetector> TiltValidated;

    public bool IsValidated { get; private set; }

    private float baselineAxisAngle;
    private bool hasBaseline;

    private void Awake()
    {
        if (headTransform == null)
            headTransform = transform;

        if (referenceTransform == null)
            referenceTransform = headTransform.parent;
    }

    private void OnEnable()
    {
        ResetDetector();
    }

    private void Update()
    {
        if (IsValidated && oneShotUntilReset)
            return;

        if (headTransform == null)
            return;

        if (requireHeadGrab && (headGrabPoint == null || !headGrabPoint.IsGrabbed))
            return;

        if (!hasBaseline)
            CaptureBaseline();

        float currentAxisAngle = GetLocalAxisAngle(headTransform.localEulerAngles);
        float tiltDelta = Mathf.Abs(Mathf.DeltaAngle(baselineAxisAngle, currentAxisAngle));

        if (tiltDelta < requiredTiltDegrees)
            return;

        ValidateTilt();
    }

    public void ResetDetector()
    {
        IsValidated = false;
        hasBaseline = false;
    }

    private void CaptureBaseline()
    {
        if (headTransform == null)
            return;

        if (referenceTransform != null)
        {
            Quaternion localRotation = Quaternion.Inverse(referenceTransform.rotation) * headTransform.rotation;
            baselineAxisAngle = GetLocalAxisAngle(localRotation.eulerAngles);
        }
        else
        {
            baselineAxisAngle = GetLocalAxisAngle(headTransform.localEulerAngles);
        }

        hasBaseline = true;
    }

    private void ValidateTilt()
    {
        IsValidated = true;

        TiltValidated?.Invoke(this);
        onTiltValidated?.Invoke();
        onTiltValidatedDetailed?.Invoke(this);
        AnyTiltValidated?.Invoke(this);
    }

    private float GetLocalAxisAngle(Vector3 eulerAngles)
    {
        switch (tiltAxis)
        {
            case TiltAxis.X:
                return eulerAngles.x;
            case TiltAxis.Y:
                return eulerAngles.y;
            case TiltAxis.Z:
                return eulerAngles.z;
            default:
                return eulerAngles.x;
        }
    }
}
