using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class BagValveMask : MonoBehaviour, IBreathingDevice
{
    [Serializable]
    public class PumpEvent : UnityEvent<int>
    {
    }

    [Header("Attachment")]
    [SerializeField] private Vector3 attachLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 attachLocalEuler = Vector3.zero;
    [SerializeField] private bool lockTransformWhenAttached = true;

    [Header("Pumping")]
    [SerializeField] private int pumpsRequired = 2;

    [Header("Events")]
    [SerializeField] private PumpEvent onPump;
    [SerializeField] private UnityEvent onPumpsCompleted;
    [SerializeField] private UnityEvent onAttached;
    [SerializeField] private UnityEvent onDetached;

    public bool IsAttached { get; private set; }

    private Transform attachPoint;
    private int pumpCount;

    public event Action<BagValveMask, int> PumpPerformed;
    public event Action<BagValveMask> PumpsCompleted;

    private void Update()
    {
#if UNITY_EDITOR
        // Small helper for testing in editor: press P to pump when attached
        if (IsAttached && Input.GetKeyDown(KeyCode.P))
            Pump();
#endif
    }

    public void AttachTo(Transform attach)
    {
        if (attach == null)
            throw new ArgumentNullException(nameof(attach));

        attachPoint = attach;
        transform.SetParent(attachPoint, true);
        transform.localPosition = attachLocalPosition;
        transform.localEulerAngles = attachLocalEuler;

        if (lockTransformWhenAttached)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;
        }

        IsAttached = true;
        pumpCount = 0;
        onAttached?.Invoke();
    }

    public void Detach()
    {
        if (lockTransformWhenAttached)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = false;
        }

        transform.SetParent(null, true);
        attachPoint = null;
        IsAttached = false;
        pumpCount = 0;
        onDetached?.Invoke();
    }

    public void Pump()
    {
        if (!IsAttached)
            return;

        pumpCount = Mathf.Clamp(pumpCount + 1, 0, pumpsRequired);
        onPump?.Invoke(pumpCount);
        PumpPerformed?.Invoke(this, pumpCount);

        if (pumpCount >= pumpsRequired)
        {
            onPumpsCompleted?.Invoke();
            PumpsCompleted?.Invoke(this);
        }
    }

    // Optional helper to reset count (e.g., when device is detached or session restarted)
    public void ResetPumps()
    {
        pumpCount = 0;
    }
}
