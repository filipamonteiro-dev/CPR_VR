using UnityEngine;

[DisallowMultipleComponent]
public class CPRSoftLockFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CPRHandPlacementDetector placementDetector;
    [SerializeField] private Transform proxyRoot;

    [Header("Follow")]
    [SerializeField] private bool followOnlyWhenAligned = true;
    [SerializeField] private float positionFollowSpeed = 14f;
    [SerializeField] private float rotationFollowSpeed = 18f;
    [SerializeField] private float snapPositionDistance = 0.01f;
    [SerializeField] private float snapRotationDegrees = 8f;

    private void Awake()
    {
        if (proxyRoot == null)
            proxyRoot = transform;
    }

    private void LateUpdate()
    {
        if (placementDetector == null || proxyRoot == null)
            return;

        if (!placementDetector.TryGetTargetPose(out var targetPosition, out var targetRotation))
            return;

        if (followOnlyWhenAligned && !placementDetector.IsAligned)
            return;

        float alignmentStrength = Mathf.Clamp01(placementDetector.AlignmentProgress);
        float positionT = Mathf.Clamp01(Time.deltaTime * positionFollowSpeed * Mathf.Max(0.15f, alignmentStrength));
        float rotationT = Mathf.Clamp01(Time.deltaTime * rotationFollowSpeed * Mathf.Max(0.15f, alignmentStrength));

        if (Vector3.Distance(proxyRoot.position, targetPosition) <= snapPositionDistance)
            proxyRoot.position = targetPosition;
        else
            proxyRoot.position = Vector3.Lerp(proxyRoot.position, targetPosition, positionT);

        if (Quaternion.Angle(proxyRoot.rotation, targetRotation) <= snapRotationDegrees)
            proxyRoot.rotation = targetRotation;
        else
            proxyRoot.rotation = Quaternion.Slerp(proxyRoot.rotation, targetRotation, rotationT);
    }
}