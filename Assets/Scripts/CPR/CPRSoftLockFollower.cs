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
    [SerializeField] private bool resetWhenUnaligned = true;
    [SerializeField] private bool resetToSourceTransform = true;
    [SerializeField] private float resetPositionSpeed = 12f;
    [SerializeField] private float resetRotationSpeed = 16f;

    [Header("Reset Pose Override")]
    [SerializeField] private bool useResetPoseOverride;
    [SerializeField] private Vector3 resetLocalPosition;
    [SerializeField] private Vector3 resetLocalEuler;
    [SerializeField] private bool useResetWorldPoseOverride;
    [SerializeField] private Vector3 resetWorldPosition;
    [SerializeField] private Vector3 resetWorldEuler;
    [SerializeField] private bool useFixedLocalReset = true;
    [SerializeField] private Vector3 fixedLocalPosition = new Vector3(0f, 0f, -0.05f);
    [SerializeField] private Vector3 fixedLocalEuler = new Vector3(0f, 180f, 0f);

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private bool hasInitialPose;

    private void Awake()
    {
        if (proxyRoot == null)
            proxyRoot = transform;

        if (useFixedLocalReset)
        {
            initialLocalPosition = fixedLocalPosition;
            initialLocalRotation = Quaternion.Euler(fixedLocalEuler);
        }
        else if (useResetWorldPoseOverride)
        {
            initialLocalPosition = proxyRoot.localPosition;
            initialLocalRotation = proxyRoot.localRotation;
        }
        else if (useResetPoseOverride)
        {
            initialLocalPosition = resetLocalPosition;
            initialLocalRotation = Quaternion.Euler(resetLocalEuler);
        }
        else
        {
            initialLocalPosition = proxyRoot.localPosition;
            initialLocalRotation = proxyRoot.localRotation;
        }

        hasInitialPose = true;
    }

    private void LateUpdate()
    {
        if (placementDetector == null || proxyRoot == null)
            return;

        if (!placementDetector.TryGetTargetPose(out var targetPosition, out var targetRotation))
            return;

        if (followOnlyWhenAligned && !placementDetector.IsAligned)
        {
            if (resetWhenUnaligned)
                ResetToInitialPose();

            return;
        }

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

    private void OnDisable()
    {
        if (proxyRoot == null)
            return;

        if (useFixedLocalReset)
        {
            proxyRoot.localPosition = fixedLocalPosition;
            proxyRoot.localRotation = Quaternion.Euler(fixedLocalEuler);
            return;
        }

        if (useResetWorldPoseOverride)
        {
            proxyRoot.position = resetWorldPosition;
            proxyRoot.rotation = Quaternion.Euler(resetWorldEuler);
            return;
        }

        if (useResetPoseOverride)
        {
            proxyRoot.localPosition = resetLocalPosition;
            proxyRoot.localRotation = Quaternion.Euler(resetLocalEuler);
            return;
        }

        if (hasInitialPose)
        {
            proxyRoot.localPosition = initialLocalPosition;
            proxyRoot.localRotation = initialLocalRotation;
        }
    }

    private void ResetToInitialPose()
    {
        if (!hasInitialPose || proxyRoot == null)
            return;

        float positionT = Mathf.Clamp01(Time.deltaTime * resetPositionSpeed);
        float rotationT = Mathf.Clamp01(Time.deltaTime * resetRotationSpeed);

        if (useFixedLocalReset)
        {
            proxyRoot.localPosition = fixedLocalPosition;
            proxyRoot.localRotation = Quaternion.Euler(fixedLocalEuler);
            return;
        }

        if (resetToSourceTransform && placementDetector != null && placementDetector.SourceTransform != null)
        {
            var source = placementDetector.SourceTransform;
            proxyRoot.position = Vector3.Lerp(proxyRoot.position, source.position, positionT);
            proxyRoot.rotation = Quaternion.Slerp(proxyRoot.rotation, source.rotation, rotationT);
            return;
        }

        if (useResetWorldPoseOverride)
        {
            proxyRoot.position = Vector3.Lerp(proxyRoot.position, resetWorldPosition, positionT);
            proxyRoot.rotation = Quaternion.Slerp(proxyRoot.rotation, Quaternion.Euler(resetWorldEuler), rotationT);
            return;
        }

        proxyRoot.localPosition = Vector3.Lerp(proxyRoot.localPosition, initialLocalPosition, positionT);
        proxyRoot.localRotation = Quaternion.Slerp(proxyRoot.localRotation, initialLocalRotation, rotationT);
    }
}