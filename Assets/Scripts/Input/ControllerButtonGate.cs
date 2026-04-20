using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class ControllerButtonGate : MonoBehaviour
{
    [SerializeField] private InputActionReference[] triggerActions;
    [SerializeField] private bool oneShotUntilReset;

    public event Action CallRequested;

    public bool HasTriggered { get; private set; }

    private void OnEnable()
    {
        SubscribeActions();
    }

    private void OnDisable()
    {
        UnsubscribeActions();
    }

    public void ResetGate()
    {
        HasTriggered = false;
    }

    private void SubscribeActions()
    {
        if (triggerActions == null)
            return;

        for (int i = 0; i < triggerActions.Length; i++)
        {
            var action = triggerActions[i] != null ? triggerActions[i].action : null;
            if (action == null)
                continue;

            action.performed += OnActionPerformed;
        }
    }

    private void UnsubscribeActions()
    {
        if (triggerActions == null)
            return;

        for (int i = 0; i < triggerActions.Length; i++)
        {
            var action = triggerActions[i] != null ? triggerActions[i].action : null;
            if (action == null)
                continue;

            action.performed -= OnActionPerformed;
        }
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        if (oneShotUntilReset && HasTriggered)
            return;

        HasTriggered = true;
        CallRequested?.Invoke();
    }
}
