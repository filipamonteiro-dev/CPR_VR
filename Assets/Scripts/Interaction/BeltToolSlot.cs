using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
public class BeltToolSlot : MonoBehaviour
{
    [Serializable]
    public class SlotInteractedEvent : UnityEvent<BeltToolSlot>
    {
    }

    [SerializeField] private XRSimpleInteractable slotInteractable;
    [SerializeField] private FloatingToolPresenter toolPresenter;
    [SerializeField] private bool toggleBetweenHolsterAndFloating = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onSlotInteracted;
    [SerializeField] private SlotInteractedEvent onSlotInteractedDetailed;

    public event Action<BeltToolSlot> SlotInteracted;

    private void Awake()
    {
        if (slotInteractable == null)
            slotInteractable = GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        if (slotInteractable == null)
            return;

        slotInteractable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        if (slotInteractable == null)
            return;

        slotInteractable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        HandleInteraction();
    }

    [ContextMenu("Debug/Interact Slot")]
    public void HandleInteraction()
    {
        if (toolPresenter != null)
        {
            if (toggleBetweenHolsterAndFloating)
                toolPresenter.TogglePresentation();
            else
                toolPresenter.Present();
        }

        SlotInteracted?.Invoke(this);
        onSlotInteracted?.Invoke();
        onSlotInteractedDetailed?.Invoke(this);
    }
}
