using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
public class PhoneDialButton : MonoBehaviour
{
    public enum ButtonType
    {
        Digit,
        Call,
        Clear,
        Backspace,
    }

    [Header("Dialing")]
    [SerializeField] private PhoneDialer dialer;
    [SerializeField] private ButtonType buttonType = ButtonType.Digit;
    [SerializeField] [Range(0, 9)] private int digit;

    [Header("Interaction")]
    [SerializeField] private XRSimpleInteractable interactable;

    [Header("Visual Press")]
    [SerializeField] private Transform buttonVisual;
    [SerializeField] private Vector3 localPressDirection = Vector3.down;
    [SerializeField] private float pressDepth = 0.0035f;
    [SerializeField] private float pressDuration = 0.045f;
    [SerializeField] private float releaseDuration = 0.06f;

    private Vector3 initialLocalPosition;
    private Coroutine pressRoutine;

    private void Awake()
    {
        if (interactable == null)
            interactable = GetComponent<XRSimpleInteractable>();

        if (buttonVisual == null)
            buttonVisual = transform;

        initialLocalPosition = buttonVisual.localPosition;
    }

    private void OnDisable()
    {
        if (interactable != null)
            interactable.hoverEntered.RemoveListener(OnHoverEntered);

        if (pressRoutine != null)
            StopCoroutine(pressRoutine);

        buttonVisual.localPosition = initialLocalPosition;
    }

    private void OnEnable()
    {
        if (interactable != null)
            interactable.hoverEntered.AddListener(OnHoverEntered);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        TryTrigger();
    }

    private void TryTrigger()
    {
        if (pressRoutine != null)
            return;

        TriggerButton();
    }

    [ContextMenu("Debug/Trigger Button")]
    public void TriggerButton()
    {
        if (dialer == null)
            return;

        switch (buttonType)
        {
            case ButtonType.Digit:
                dialer.InputDigit(digit);
                break;
            case ButtonType.Call:
                dialer.PressCall();
                break;
            case ButtonType.Clear:
                dialer.PressClear();
                break;
            case ButtonType.Backspace:
                dialer.PressBackspace();
                break;
        }

        PlayPressAnimation();
    }

    private void PlayPressAnimation()
    {
        if (buttonVisual == null)
            return;

        if (pressRoutine != null)
            StopCoroutine(pressRoutine);

        pressRoutine = StartCoroutine(AnimatePress());
    }

    private IEnumerator AnimatePress()
    {
        Vector3 pressOffset = localPressDirection.normalized * pressDepth;
        Vector3 pressedPosition = initialLocalPosition + pressOffset;

        yield return LerpLocalPosition(buttonVisual, initialLocalPosition, pressedPosition, pressDuration);
        yield return LerpLocalPosition(buttonVisual, pressedPosition, initialLocalPosition, releaseDuration);

        buttonVisual.localPosition = initialLocalPosition;
        pressRoutine = null;
    }

    private static IEnumerator LerpLocalPosition(Transform target, Vector3 from, Vector3 to, float duration)
    {
        if (target == null)
            yield break;

        if (duration <= 0f)
        {
            target.localPosition = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localPosition = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }

        target.localPosition = to;
    }
}
