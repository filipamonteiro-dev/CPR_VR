using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

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
        NotifyDigitsChanged();
    }

    public void InputDigit(int digit)
    {
        if (IsDialComplete)
            return;

        if (digit < 0 || digit > 9)
            return;

        if (digitBuffer.Length >= Mathf.Max(1, maxDigits))
            return;

        digitBuffer.Append(digit);
        NotifyDigitsChanged();

        if (autoSubmitWhenFull && digitBuffer.Length >= emergencyNumber.Length)
            PressCall();
    }

    public void PressBackspace()
    {
        if (IsDialComplete || digitBuffer.Length == 0)
            return;

        digitBuffer.Length -= 1;
        NotifyDigitsChanged();
    }

    public void PressClear()
    {
        if (IsDialComplete || digitBuffer.Length == 0)
            return;

        digitBuffer.Length = 0;
        NotifyDigitsChanged();
    }

    public void PressCall()
    {
        if (IsDialComplete)
            return;

        string dialedNumber = CurrentDigits;
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
            NotifyDigitsChanged();
        }
    }

    private void NotifyDigitsChanged()
    {
        string digits = CurrentDigits;
        DigitsChanged?.Invoke(this, digits);
        onDigitsChanged?.Invoke(digits);
    }
}
