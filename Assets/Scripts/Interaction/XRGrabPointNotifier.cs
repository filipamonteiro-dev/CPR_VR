using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DisallowMultipleComponent]
public class XRGrabPointNotifier : MonoBehaviour
{
    public static event Action<XRGrabPointNotifier, IXRSelectInteractor> AnyGrabStarted;
    public static event Action<XRGrabPointNotifier, IXRSelectInteractor> AnyGrabEnded;

    [SerializeField] private XRGrabInteractable interactable;
    [SerializeField] private string grabPointId;

    public event Action<IXRSelectInteractor> GrabStarted;
    public event Action<IXRSelectInteractor> GrabEnded;

    public string GrabPointId => string.IsNullOrWhiteSpace(grabPointId) ? name : grabPointId;

    public bool IsGrabbed => currentInteractor != null;

    public IXRSelectInteractor CurrentInteractor => currentInteractor;

    private IXRSelectInteractor currentInteractor;

    private void Awake()
    {
        if (interactable == null)
            interactable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (interactable == null)
            interactable = GetComponent<XRGrabInteractable>();

        if (interactable == null)
            return;

        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        if (interactable == null)
            return;

        interactable.selectEntered.RemoveListener(OnSelectEntered);
        interactable.selectExited.RemoveListener(OnSelectExited);

        if (currentInteractor != null)
            NotifyGrabEnded(currentInteractor);

        currentInteractor = null;
    }

    public bool TryGetInteractorTransform(out Transform interactorTransform)
    {
        if (currentInteractor is Component component)
        {
            interactorTransform = component.transform;
            return true;
        }

        interactorTransform = null;
        return false;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject as IXRSelectInteractor;
        if (interactor == null)
            return;

        currentInteractor = interactor;

        GrabStarted?.Invoke(interactor);
        AnyGrabStarted?.Invoke(this, interactor);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        var interactor = args.interactorObject as IXRSelectInteractor;
        if (interactor == null)
            return;

        if (currentInteractor == interactor)
        {
            NotifyGrabEnded(interactor);
            currentInteractor = null;
        }
    }

    private void NotifyGrabEnded(IXRSelectInteractor interactor)
    {
        GrabEnded?.Invoke(interactor);
        AnyGrabEnded?.Invoke(this, interactor);
    }
}
