using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class FloatingToolPresenter : MonoBehaviour
{
    [Serializable]
    public class PresentationChangedEvent : UnityEvent<FloatingToolPresenter>
    {
    }

    [Header("References")]
    [SerializeField] private Transform toolRoot;
    [SerializeField] private Transform holsterAnchor;
    [SerializeField] private Transform floatingAnchor;
    [SerializeField] private Transform headTransform;

    [Header("Floating Pose")]
    [SerializeField] private Vector3 floatingLocalOffset = new Vector3(0f, -0.12f, 0.45f);
    [SerializeField] private Vector3 floatingEulerOffset = Vector3.zero;

    [Header("Motion")]
    [SerializeField] private float transitionDuration = 0.2f;
    [SerializeField] private bool followHeadWhilePresented = true;
    [SerializeField] private float followPositionLerpSpeed = 10f;
    [SerializeField] private float followRotationLerpSpeed = 12f;

    [Header("Mode Objects")]
    [SerializeField] private GameObject[] enableWhenPresented;
    [SerializeField] private GameObject[] disableWhenPresented;

    [Header("Events")]
    [SerializeField] private UnityEvent onPresented;
    [SerializeField] private UnityEvent onHolstered;
    [SerializeField] private PresentationChangedEvent onPresentationChanged;

    public event Action<FloatingToolPresenter> Presented;
    public event Action<FloatingToolPresenter> Holstered;

    public bool IsPresented { get; private set; }
    public bool IsTransitioning => transitionRoutine != null;

    private Coroutine transitionRoutine;

    private void Awake()
    {
        if (toolRoot == null)
            toolRoot = transform;

        if (headTransform == null && Camera.main != null)
            headTransform = Camera.main.transform;

        ApplyModeObjects(false);
    }

    private void Start()
    {
        SnapToHolster();
    }

    private void Update()
    {
        if (!IsPresented || !followHeadWhilePresented || IsTransitioning)
            return;

        if (!TryGetPresentedPose(out var targetPosition, out var targetRotation))
            return;

        toolRoot.position = Vector3.Lerp(toolRoot.position, targetPosition, Time.deltaTime * followPositionLerpSpeed);
        toolRoot.rotation = Quaternion.Slerp(toolRoot.rotation, targetRotation, Time.deltaTime * followRotationLerpSpeed);
    }

    public void TogglePresentation()
    {
        if (IsTransitioning)
            return;

        if (IsPresented)
            Holster();
        else
            Present();
    }

    public void Present()
    {
        if (IsTransitioning || IsPresented)
            return;

        if (!TryGetPresentedPose(out var targetPosition, out var targetRotation))
            return;

        BeginTransition(true, targetPosition, targetRotation);
    }

    public void Holster()
    {
        if (IsTransitioning || !IsPresented)
            return;

        if (!TryGetHolsterPose(out var targetPosition, out var targetRotation))
            return;

        BeginTransition(false, targetPosition, targetRotation);
    }

    public void SnapToHolster()
    {
        if (!TryGetHolsterPose(out var targetPosition, out var targetRotation))
            return;

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        toolRoot.SetPositionAndRotation(targetPosition, targetRotation);
        IsPresented = false;
        ApplyModeObjects(false);
    }

    private void BeginTransition(bool presentedState, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(AnimateTransition(presentedState, targetPosition, targetRotation));
    }

    private IEnumerator AnimateTransition(bool presentedState, Vector3 targetPosition, Quaternion targetRotation)
    {
        Vector3 startPosition = toolRoot.position;
        Quaternion startRotation = toolRoot.rotation;

        float duration = Mathf.Max(0.01f, transitionDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            toolRoot.position = Vector3.LerpUnclamped(startPosition, targetPosition, smooth);
            toolRoot.rotation = Quaternion.SlerpUnclamped(startRotation, targetRotation, smooth);

            yield return null;
        }

        toolRoot.SetPositionAndRotation(targetPosition, targetRotation);
        transitionRoutine = null;

        IsPresented = presentedState;
        ApplyModeObjects(IsPresented);

        onPresentationChanged?.Invoke(this);

        if (IsPresented)
        {
            Presented?.Invoke(this);
            onPresented?.Invoke();
        }
        else
        {
            Holstered?.Invoke(this);
            onHolstered?.Invoke();
        }
    }

    private bool TryGetPresentedPose(out Vector3 position, out Quaternion rotation)
    {
        if (floatingAnchor != null)
        {
            position = floatingAnchor.position;
            rotation = floatingAnchor.rotation;
            return true;
        }

        if (headTransform == null)
        {
            if (Camera.main != null)
                headTransform = Camera.main.transform;
        }

        if (headTransform == null)
        {
            position = toolRoot.position;
            rotation = toolRoot.rotation;
            return false;
        }

        position = headTransform.TransformPoint(floatingLocalOffset);
        rotation = Quaternion.Euler(headTransform.eulerAngles + floatingEulerOffset);
        return true;
    }

    private bool TryGetHolsterPose(out Vector3 position, out Quaternion rotation)
    {
        if (holsterAnchor == null)
        {
            position = toolRoot.position;
            rotation = toolRoot.rotation;
            return false;
        }

        position = holsterAnchor.position;
        rotation = holsterAnchor.rotation;
        return true;
    }

    private void ApplyModeObjects(bool presented)
    {
        SetObjectsActive(enableWhenPresented, presented);
        SetObjectsActive(disableWhenPresented, !presented);
    }

    private static void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(active);
        }
    }
}
