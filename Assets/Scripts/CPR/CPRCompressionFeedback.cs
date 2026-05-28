using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DisallowMultipleComponent]
public class CPRCompressionFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CPRCompressionPulseDetector pulseDetector;
    [SerializeField] private XRBaseInteractor[] hapticInteractors;
    [SerializeField] private Rigidbody targetRigidbody;
    [SerializeField] private Transform forcePoint;
    [SerializeField] private Transform forceDirectionSource;
    [SerializeField] private bool autoFindHaptics = true;

    [Header("Haptic Actions")]
    [SerializeField] private bool useInputActionHaptics = true;
    [SerializeField] private InputActionReference leftHapticAction;
    [SerializeField] private InputActionReference rightHapticAction;

    [Header("Haptics")]
    [SerializeField] private bool hapticsOnPress = true;
    [SerializeField, Range(0f, 1f)] private float pressAmplitude = 0.6f;
    [SerializeField] private float pressDuration = 0.06f;
    [SerializeField] private bool hapticsOnRelease;
    [SerializeField, Range(0f, 1f)] private float releaseAmplitude = 0.2f;
    [SerializeField] private float releaseDuration = 0.04f;

    [Header("Mannequin Force")]
    [SerializeField] private bool applyForceOnPress = true;
    [SerializeField] private float pressForce = 25f;
    [SerializeField] private bool applyContinuousForce = true;
    [SerializeField] private float continuousForce = 40f;
    [SerializeField] private Vector3 localForceDirection = new Vector3(0f, -1f, 0f);
    [SerializeField] private ForceMode pressForceMode = ForceMode.Impulse;
    [SerializeField] private ForceMode continuousForceMode = ForceMode.Force;

    [Header("Mannequin Transform")]
    [SerializeField] private bool driveChestTransform = true;
    [SerializeField] private Transform chestTransform;
    [SerializeField] private Vector3 chestLocalAxis = new Vector3(0f, -1f, 0f);
    [SerializeField] private float maxChestOffset = 0.03f;
    [SerializeField] private float chestReturnSpeed = 14f;

    private readonly List<HapticImpulsePlayer> hapticPlayers = new List<HapticImpulsePlayer>();
    private HapticImpulsePlayer leftHapticPlayer;
    private HapticImpulsePlayer rightHapticPlayer;
    private Vector3 chestInitialLocalPosition;
    private bool hasChestInitialPose;

    private void Awake()
    {
        if (forceDirectionSource == null)
            forceDirectionSource = transform;

        if (autoFindHaptics)
            RefreshHapticPlayers();

        if (useInputActionHaptics)
            ConfigureInputActionHaptics();

        if (chestTransform != null)
        {
            chestInitialLocalPosition = chestTransform.localPosition;
            hasChestInitialPose = true;
        }
    }

    private void Start()
    {
        if (autoFindHaptics)
            RefreshHapticPlayers();

        if (useInputActionHaptics)
            ConfigureInputActionHaptics();
    }

    private void OnEnable()
    {
        if (pulseDetector != null)
        {
            pulseDetector.CompressionPressed += OnCompressionPressed;
            pulseDetector.CompressionReleased += OnCompressionReleased;
        }
    }

    private void OnDisable()
    {
        if (pulseDetector != null)
        {
            pulseDetector.CompressionPressed -= OnCompressionPressed;
            pulseDetector.CompressionReleased -= OnCompressionReleased;
        }
    }

    private void FixedUpdate()
    {
        UpdateChestTransform();

        if (!applyContinuousForce || pulseDetector == null || targetRigidbody == null)
            return;

        if (!pulseDetector.IsCompressionActive)
            return;

        float strength = Mathf.Clamp01(pulseDetector.CompressionProgress);
        ApplyForce(GetForceDirection() * (continuousForce * strength), continuousForceMode);
    }

    private void UpdateChestTransform()
    {
        if (!driveChestTransform || chestTransform == null)
            return;

        if (!hasChestInitialPose)
        {
            chestInitialLocalPosition = chestTransform.localPosition;
            hasChestInitialPose = true;
        }

        float strength = 0f;
        if (pulseDetector != null && pulseDetector.IsCompressionActive)
            strength = Mathf.Clamp01(pulseDetector.CompressionProgress);

        Vector3 axis = chestLocalAxis.sqrMagnitude > 0.0001f ? chestLocalAxis.normalized : Vector3.down;
        Vector3 targetLocalPosition = chestInitialLocalPosition + axis * (maxChestOffset * strength);
        float t = Mathf.Clamp01(Time.deltaTime * chestReturnSpeed);
        chestTransform.localPosition = Vector3.Lerp(chestTransform.localPosition, targetLocalPosition, t);
    }

    private void OnCompressionPressed(CPRCompressionPulseDetector detector)
    {
        if (hapticsOnPress)
            SendHaptic(pressAmplitude, pressDuration);

        if (applyForceOnPress)
            ApplyForce(GetForceDirection() * pressForce, pressForceMode);
    }

    private void OnCompressionReleased(CPRCompressionPulseDetector detector)
    {
        if (hapticsOnRelease)
            SendHaptic(releaseAmplitude, releaseDuration);
    }

    private void SendHaptic(float amplitude, float duration)
    {
        if (useInputActionHaptics && TrySendInputActionHaptics(amplitude, duration))
            return;

        if (hapticPlayers.Count == 0 && autoFindHaptics)
            RefreshHapticPlayers();

        if (hapticPlayers.Count == 0)
            return;

        float safeAmplitude = Mathf.Clamp01(amplitude);
        float safeDuration = Mathf.Max(0.01f, duration);

        foreach (var player in hapticPlayers)
        {
            if (player == null)
                continue;

            player.SendHapticImpulse(safeAmplitude, safeDuration);
        }
    }

    private void ApplyForce(Vector3 force, ForceMode mode)
    {
        if (targetRigidbody == null)
            return;

        Vector3 position = forcePoint != null ? forcePoint.position : targetRigidbody.worldCenterOfMass;
        targetRigidbody.AddForceAtPosition(force, position, mode);
    }

    private Vector3 GetForceDirection()
    {
        if (forceDirectionSource == null)
            return localForceDirection.normalized;

        return forceDirectionSource.TransformDirection(localForceDirection).normalized;
    }

    private void RefreshHapticPlayers()
    {
        hapticPlayers.Clear();

        if (hapticInteractors == null || hapticInteractors.Length == 0)
        {
            if (!autoFindHaptics)
                return;

            hapticInteractors = FindObjectsOfType<XRBaseInteractor>(true);
        }

        if (hapticInteractors == null || hapticInteractors.Length == 0)
            return;

        foreach (var interactor in hapticInteractors)
        {
            if (interactor == null)
                continue;

            var player = interactor.GetComponentInParent<HapticImpulsePlayer>(true);
            if (player == null)
                player = interactor.gameObject.AddComponent<HapticImpulsePlayer>();

            if (!hapticPlayers.Contains(player))
                hapticPlayers.Add(player);
        }
    }

    private void ConfigureInputActionHaptics()
    {
        leftHapticPlayer = CreateActionHapticPlayer(leftHapticPlayer, leftHapticAction, "LeftHapticPlayer");
        rightHapticPlayer = CreateActionHapticPlayer(rightHapticPlayer, rightHapticAction, "RightHapticPlayer");
    }

    private HapticImpulsePlayer CreateActionHapticPlayer(HapticImpulsePlayer existing, InputActionReference actionRef, string name)
    {
        if (actionRef == null)
            return existing;

        if (existing == null)
        {
            var playerObject = new GameObject(name);
            playerObject.transform.SetParent(transform, false);
            existing = playerObject.AddComponent<HapticImpulsePlayer>();
        }

        var output = existing.hapticOutput;
        output.inputSourceMode = XRInputHapticImpulseProvider.InputSourceMode.InputActionReference;
        output.inputActionReference = actionRef;
        existing.hapticOutput = output;

        return existing;
    }

    private bool TrySendInputActionHaptics(float amplitude, float duration)
    {
        if (leftHapticPlayer == null && rightHapticPlayer == null)
            return false;

        float safeAmplitude = Mathf.Clamp01(amplitude);
        float safeDuration = Mathf.Max(0.01f, duration);

        bool sent = false;

        if (leftHapticPlayer != null)
            sent |= leftHapticPlayer.SendHapticImpulse(safeAmplitude, safeDuration);

        if (rightHapticPlayer != null)
            sent |= rightHapticPlayer.SendHapticImpulse(safeAmplitude, safeDuration);

        return sent;
    }
}
