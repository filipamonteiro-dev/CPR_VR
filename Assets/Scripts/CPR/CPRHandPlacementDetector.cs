using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class CPRHandPlacementDetector : MonoBehaviour
{
    [Serializable]
    public class AlignmentChangedEvent : UnityEvent<CPRHandPlacementDetector, bool>
    {
    }

    [Header("References")]
    [SerializeField] private Transform sourceTransform;
    [SerializeField] private Transform targetTransform;

    [Header("Tolerance")]
    [SerializeField] private float positionTolerance = 0.08f;
    [SerializeField] private float rotationTolerance = 20f;
    [SerializeField] private float holdTimeToLock = 0.2f;

    [Header("Events")]
    [SerializeField] private UnityEvent onAlignmentLocked;
    [SerializeField] private UnityEvent onAlignmentLost;
    [SerializeField] private AlignmentChangedEvent onAlignmentChangedDetailed;

    public event Action<CPRHandPlacementDetector> AlignmentLocked;
    public event Action<CPRHandPlacementDetector> AlignmentLost;

    public bool IsAligned { get; private set; }
    public float AlignmentProgress { get; private set; }

    public Transform SourceTransform => sourceTransform;
    public Transform TargetTransform => targetTransform;

    private float heldTime;

    private void OnEnable()
    {
        ResetDetector();
    }

    private void Update()
    {
        if (sourceTransform == null || targetTransform == null)
            return;

        bool isWithinTolerance = IsSourceWithinTolerance();

        if (isWithinTolerance)
            heldTime += Time.deltaTime;
        else
            heldTime = 0f;

        float lockWindow = Mathf.Max(0.01f, holdTimeToLock);
        AlignmentProgress = Mathf.Clamp01(heldTime / lockWindow);

        bool shouldBeAligned = heldTime >= lockWindow;
        if (shouldBeAligned == IsAligned)
            return;

        IsAligned = shouldBeAligned;

        if (IsAligned)
        {
            AlignmentLocked?.Invoke(this);
            onAlignmentLocked?.Invoke();
        }
        else
        {
            AlignmentLost?.Invoke(this);
            onAlignmentLost?.Invoke();
        }

        onAlignmentChangedDetailed?.Invoke(this, IsAligned);
    }

    public void ResetDetector()
    {
        heldTime = 0f;
        AlignmentProgress = 0f;
        IsAligned = false;
    }

    public bool TryGetTargetPose(out Vector3 position, out Quaternion rotation)
    {
        if (targetTransform == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        position = targetTransform.position;
        rotation = targetTransform.rotation;
        return true;
    }

    public float GetPositionError()
    {
        if (sourceTransform == null || targetTransform == null)
            return float.PositiveInfinity;

        return Vector3.Distance(sourceTransform.position, targetTransform.position);
    }

    public float GetRotationError()
    {
        if (sourceTransform == null || targetTransform == null)
            return float.PositiveInfinity;

        return Quaternion.Angle(sourceTransform.rotation, targetTransform.rotation);
    }

    private bool IsSourceWithinTolerance()
    {
        if (sourceTransform == null || targetTransform == null)
            return false;

        return GetPositionError() <= positionTolerance && GetRotationError() <= rotationTolerance;
    }
}