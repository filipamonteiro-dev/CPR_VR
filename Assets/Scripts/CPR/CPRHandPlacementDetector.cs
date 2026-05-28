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
    [SerializeField] private float enterTolerance = 0.08f;
    [SerializeField] private float exitTolerance = 0.12f;
    [SerializeField] private float holdTimeToLock = 0.2f;
    [SerializeField] private float lostGraceSeconds = 0.2f;

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
    private float lostTimer;

    private void OnEnable()
    {
        ResetDetector();
    }

    private void Update()
    {
        if (sourceTransform == null || targetTransform == null)
            return;

        bool isWithinEnter = IsSourceWithinTolerance(enterTolerance);
        bool isWithinExit = IsSourceWithinTolerance(exitTolerance);

        if (!IsAligned)
        {
            if (isWithinEnter)
                heldTime += Time.deltaTime;
            else
                heldTime = 0f;

            float lockWindow = Mathf.Max(0.01f, holdTimeToLock);
            AlignmentProgress = Mathf.Clamp01(heldTime / lockWindow);

            if (heldTime >= lockWindow)
            {
                IsAligned = true;
                lostTimer = 0f;
                AlignmentLocked?.Invoke(this);
                onAlignmentLocked?.Invoke();
                onAlignmentChangedDetailed?.Invoke(this, true);
            }

            return;
        }

        AlignmentProgress = 1f;

        if (isWithinExit)
        {
            lostTimer = 0f;
            return;
        }

        lostTimer += Time.deltaTime;
        if (lostTimer < Mathf.Max(0f, lostGraceSeconds))
            return;

        IsAligned = false;
        heldTime = 0f;
        lostTimer = 0f;
        AlignmentLost?.Invoke(this);
        onAlignmentLost?.Invoke();
        onAlignmentChangedDetailed?.Invoke(this, false);
    }

    public void ResetDetector()
    {
        heldTime = 0f;
        lostTimer = 0f;
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

    private bool IsSourceWithinTolerance(float tolerance)
    {
        if (sourceTransform == null || targetTransform == null)
            return false;

        return GetPositionError() <= tolerance;
    }
}