using System;
using UnityEngine;

public interface IBreathingDevice
{
    // Called to attach the device to a receiver (e.g., mannequin face)
    void AttachTo(Transform attachPoint);

    // Called to detach the device from any receiver
    void Detach();

    // Called when the user performs a pump action
    void Pump();

    // Whether the device is currently attached to a receiver
    bool IsAttached { get; }
}
