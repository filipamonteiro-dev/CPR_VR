using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class BVMFaceReceiver : MonoBehaviour
{
    [Serializable]
    public class BreathsDeliveredEvent : UnityEvent<int>
    {
    }

    [Header("Attachment Point")]
    [SerializeField] private Transform attachPoint;

    [Header("Events")]
    [SerializeField] private UnityEvent onDeviceAttached;
    [SerializeField] private BreathsDeliveredEvent onBreathsDelivered;

    // How many breaths have been delivered to the patient via this receiver
    public int TotalBreaths { get; private set; }

    private BagValveMask currentDevice;

    private void Reset()
    {
        // Try to find a sensible default attach point
        if (attachPoint == null)
            attachPoint = transform;
    }

    private void OnValidate()
    {
        if (attachPoint == null)
            attachPoint = transform;
    }

    // Automatic placement when a BagValveMask enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        if (currentDevice != null)
            return;

        var device = other.GetComponentInParent<BagValveMask>();
        if (device == null)
            return;

        PlaceDevice(device);
    }

    public void PlaceDevice(BagValveMask device)
    {
        if (device == null)
            return;

        currentDevice = device;
        currentDevice.AttachTo(attachPoint);
        currentDevice.PumpsCompleted += OnDevicePumpsCompleted;
        onDeviceAttached?.Invoke();
    }

    public void RemoveDevice()
    {
        if (currentDevice == null)
            return;

        currentDevice.PumpsCompleted -= OnDevicePumpsCompleted;
        currentDevice.Detach();
        currentDevice = null;
    }

    private void OnDevicePumpsCompleted(BagValveMask device)
    {
        // Increment breaths and notify listeners
        TotalBreaths++;
        onBreathsDelivered?.Invoke(TotalBreaths);

        // Optionally keep the device attached or auto-detach. We keep attached by default.
    }
}
