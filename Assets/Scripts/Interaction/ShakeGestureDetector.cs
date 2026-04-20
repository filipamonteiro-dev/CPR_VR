using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class ShakeGestureDetector : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z,
    }

    [Serializable]
    public class ProgressChangedEvent : UnityEvent<float>
    {
    }

    public static event Action<ShakeGestureDetector> AnyShakeValidated;

    [SerializeField] private XRGrabPointNotifier[] grabPoints;
    [SerializeField] private Transform referenceSpace;
    [SerializeField] private bool requireAllGrabPoints = true;
    [SerializeField] private Axis shakeAxis = Axis.X;

    [Header("Shake thresholds")]
    [SerializeField] private float minFrameDisplacement = 0.005f;
    [SerializeField] private int requiredDirectionChanges = 5;
    [SerializeField] private float requiredActiveTime = 0.9f;
    [SerializeField] private float maxPauseTime = 0.35f;
    [SerializeField] private float progressDecayPerSecond = 0.75f;

    [Header("Events")]
    [SerializeField] private UnityEvent onShakeValidated;
    [SerializeField] private ProgressChangedEvent onProgressChanged;

    public event Action<ShakeGestureDetector> ShakeValidated;

    public bool IsValidated { get; private set; }
    public float Progress { get; private set; }

    private readonly Dictionary<XRGrabPointNotifier, float> lastAxisValue = new Dictionary<XRGrabPointNotifier, float>();

    private float activeTime;
    private int directionChanges;
    private float lastMotionTime;
    private float lastVelocitySign;
    private bool hasVelocitySign;

    private void Awake()
    {
        if (referenceSpace == null)
            referenceSpace = transform;
    }

    private void OnEnable()
    {
        ResetDetector();
    }

    public void ResetDetector()
    {
        IsValidated = false;
        Progress = 0f;
        activeTime = 0f;
        directionChanges = 0;
        lastMotionTime = Time.time;
        hasVelocitySign = false;
        lastVelocitySign = 0f;
        lastAxisValue.Clear();
        onProgressChanged?.Invoke(Progress);
    }

    private void Update()
    {
        if (IsValidated)
            return;

        if (!HasRequiredActiveGrabPoints())
        {
            DecayProgress(Time.deltaTime);
            ClearCachedAxisValues();
            return;
        }

        if (!TrySampleMotion(Time.deltaTime, out var frameDisplacement, out var averageVelocitySign))
            return;

        if (frameDisplacement >= minFrameDisplacement)
        {
            activeTime += Time.deltaTime;
            lastMotionTime = Time.time;

            if (Mathf.Abs(averageVelocitySign) > 0.01f)
            {
                if (hasVelocitySign && Mathf.Sign(averageVelocitySign) != Mathf.Sign(lastVelocitySign))
                    directionChanges++;

                lastVelocitySign = averageVelocitySign;
                hasVelocitySign = true;
            }
        }
        else if (Time.time - lastMotionTime > maxPauseTime)
        {
            DecayProgress(Time.deltaTime);
        }

        UpdateProgress();
        TryValidate();
    }

    private bool HasRequiredActiveGrabPoints()
    {
        int configuredPoints = 0;
        int activePoints = 0;

        for (int i = 0; i < grabPoints.Length; i++)
        {
            var point = grabPoints[i];
            if (point == null)
                continue;

            configuredPoints++;
            if (point.IsGrabbed)
                activePoints++;
        }

        if (configuredPoints == 0)
            return false;

        if (requireAllGrabPoints)
            return activePoints == configuredPoints;

        return activePoints > 0;
    }

    private bool TrySampleMotion(float deltaTime, out float averageDisplacement, out float averageVelocitySign)
    {
        averageDisplacement = 0f;
        averageVelocitySign = 0f;

        if (deltaTime <= 0f)
            return false;

        int sampledCount = 0;
        float displacementSum = 0f;
        float velocitySum = 0f;

        for (int i = 0; i < grabPoints.Length; i++)
        {
            var point = grabPoints[i];
            if (point == null || !point.IsGrabbed)
                continue;

            if (!point.TryGetInteractorTransform(out var interactorTransform) || interactorTransform == null)
                continue;

            float axisValue = GetAxisValue(referenceSpace.InverseTransformPoint(interactorTransform.position));

            if (lastAxisValue.TryGetValue(point, out var previousAxisValue))
            {
                float deltaAxis = axisValue - previousAxisValue;
                displacementSum += Mathf.Abs(deltaAxis);
                velocitySum += deltaAxis / deltaTime;
                sampledCount++;
            }

            lastAxisValue[point] = axisValue;
        }

        if (sampledCount == 0)
            return false;

        averageDisplacement = displacementSum / sampledCount;
        float averageVelocity = velocitySum / sampledCount;
        averageVelocitySign = Mathf.Sign(averageVelocity);
        return true;
    }

    private void ClearCachedAxisValues()
    {
        if (lastAxisValue.Count > 0)
            lastAxisValue.Clear();

        hasVelocitySign = false;
        lastVelocitySign = 0f;
    }

    private void DecayProgress(float deltaTime)
    {
        activeTime = Mathf.Max(0f, activeTime - progressDecayPerSecond * deltaTime);
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        float timeProgress = requiredActiveTime > 0f ? Mathf.Clamp01(activeTime / requiredActiveTime) : 1f;
        float directionProgress = requiredDirectionChanges > 0 ? Mathf.Clamp01((float)directionChanges / requiredDirectionChanges) : 1f;

        float newProgress = Mathf.Min(timeProgress, directionProgress);
        if (Mathf.Approximately(newProgress, Progress))
            return;

        Progress = newProgress;
        onProgressChanged?.Invoke(Progress);
    }

    private void TryValidate()
    {
        if (activeTime < requiredActiveTime)
            return;

        if (directionChanges < requiredDirectionChanges)
            return;

        IsValidated = true;
        Progress = 1f;
        onProgressChanged?.Invoke(Progress);

        ShakeValidated?.Invoke(this);
        onShakeValidated?.Invoke();
        AnyShakeValidated?.Invoke(this);
    }

    private float GetAxisValue(Vector3 localPosition)
    {
        switch (shakeAxis)
        {
            case Axis.X:
                return localPosition.x;
            case Axis.Y:
                return localPosition.y;
            case Axis.Z:
                return localPosition.z;
            default:
                return localPosition.x;
        }
    }
}
