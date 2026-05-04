using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[DisallowMultipleComponent]
public class PhoneDialer : MonoBehaviour
{
    [Serializable]
    public class DigitsChangedEvent : UnityEvent<string>
    {
    }

    [Serializable]
    public class NumberDialedEvent : UnityEvent<string>
    {
    }

    [SerializeField] private string emergencyNumber = "112";
    [SerializeField] private int maxDigits = 3;
    [SerializeField] private bool autoSubmitWhenFull = true;
    [SerializeField] private bool clearDigitsOnInvalidCall = true;
    [SerializeField] private bool enableDebugLogs;

    [Header("UI")]
    [SerializeField] private TMP_Text dialedNumberText;

    [Header("Events")]
    [SerializeField] private UnityEvent onEmergencyNumberDialed;
    [SerializeField] private NumberDialedEvent onInvalidNumberDialed;
    [SerializeField] private DigitsChangedEvent onDigitsChanged;

    public event Action<PhoneDialer> EmergencyNumberDialed;
    public event Action<PhoneDialer, string> InvalidNumberDialed;
    public event Action<PhoneDialer, string> DigitsChanged;

    public string CurrentDigits => digitBuffer.ToString();
    public bool IsDialComplete { get; private set; }

    private readonly StringBuilder digitBuffer = new StringBuilder();

    private void OnEnable()
    {
        NotifyDigitsChanged();
    }

    public void ResetDialer()
    {
        IsDialComplete = false;
        digitBuffer.Length = 0;
        if (enableDebugLogs)
            Debug.Log("PhoneDialer reset.", this);
        NotifyDigitsChanged();
    }

    public void InputDigit(int digit)
    {
        if (IsDialComplete)
        {
            if (enableDebugLogs)
                Debug.Log("PhoneDialer ignored digit: dial complete.", this);
            return;
        }

        if (digit < 0 || digit > 9)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"PhoneDialer ignored invalid digit {digit}.", this);
            return;
        }

        if (digitBuffer.Length >= Mathf.Max(1, maxDigits))
        {
            if (enableDebugLogs)
                Debug.Log("PhoneDialer ignored digit: buffer full.", this);
            return;
        }

        digitBuffer.Append(digit);
        if (enableDebugLogs)
            Debug.Log($"PhoneDialer appended digit {digit}. Buffer: '{CurrentDigits}'.", this);
        NotifyDigitsChanged();

        if (autoSubmitWhenFull && digitBuffer.Length >= emergencyNumber.Length)
        {
            if (enableDebugLogs)
                Debug.Log("PhoneDialer auto-submit triggered.", this);
            PressCall();
        }
    }

    public void PressBackspace()
    {
        if (IsDialComplete || digitBuffer.Length == 0)
        {
            if (enableDebugLogs)
                Debug.Log("PhoneDialer backspace ignored.", this);
            return;
        }

        digitBuffer.Length -= 1;
        if (enableDebugLogs)
            Debug.Log($"PhoneDialer backspace. Buffer: '{CurrentDigits}'.", this);
        NotifyDigitsChanged();
    }

    public void PressClear()
    {
        if (IsDialComplete || digitBuffer.Length == 0)
        {
            if (enableDebugLogs)
                Debug.Log("PhoneDialer clear ignored.", this);
            return;
        }

        digitBuffer.Length = 0;
        if (enableDebugLogs)
            Debug.Log("PhoneDialer cleared.", this);
        NotifyDigitsChanged();
    }

    public void PressCall()
    {
        if (IsDialComplete)
        {
            if (enableDebugLogs)
                Debug.Log("PhoneDialer call ignored: dial complete.", this);
            return;
        }

        string dialedNumber = CurrentDigits;
        if (enableDebugLogs)
            Debug.Log($"PhoneDialer call pressed. Dialed '{dialedNumber}'.", this);
        if (string.Equals(dialedNumber, emergencyNumber, StringComparison.Ordinal))
        {
            IsDialComplete = true;
            EmergencyNumberDialed?.Invoke(this);
            onEmergencyNumberDialed?.Invoke();
            return;
        }

        InvalidNumberDialed?.Invoke(this, dialedNumber);
        onInvalidNumberDialed?.Invoke(dialedNumber);

        if (clearDigitsOnInvalidCall)
        {
            digitBuffer.Length = 0;
            if (enableDebugLogs)
                Debug.Log("PhoneDialer cleared after invalid call.", this);
            NotifyDigitsChanged();
        }
    }

    private void NotifyDigitsChanged()
    {
        string digits = CurrentDigits;
        if (dialedNumberText != null)
            dialedNumberText.text = digits;
        if (enableDebugLogs)
            Debug.Log($"PhoneDialer digits changed: '{digits}'.", this);
        DigitsChanged?.Invoke(this, digits);
        onDigitsChanged?.Invoke(digits);
    }
}
