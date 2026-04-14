using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class AnchorEnterNotifier : MonoBehaviour
{
    [Serializable]
    public class AnchorEnteredEvent : UnityEvent<AnchorEnterNotifier, Collider>
    {
    }

    public static event Action<AnchorEnterNotifier, Collider> AnyAnchorEntered;

    [SerializeField] private Collider anchorCollider;
    [SerializeField] private bool oneShot = true;
    [SerializeField] private bool requireTag;
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private string anchorId;

    [SerializeField] private UnityEvent onAnchorEntered;
    [SerializeField] private AnchorEnteredEvent onAnchorEnteredDetailed;

    public event Action<Collider> AnchorEntered;

    public string AnchorId => string.IsNullOrWhiteSpace(anchorId) ? name : anchorId;

    public Collider AnchorCollider => anchorCollider;

    public bool HasTriggered { get; private set; }

    private void Reset()
    {
        anchorCollider = GetComponent<Collider>();
        if (anchorCollider != null)
            anchorCollider.isTrigger = true;
    }

    private void Awake()
    {
        if (anchorCollider == null)
            anchorCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryNotify(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
            TryNotify(collision.collider);
    }

    public void ResetTriggeredState()
    {
        HasTriggered = false;
    }

    private void TryNotify(Collider other)
    {
        if (other == null)
            return;

        if (oneShot && HasTriggered)
            return;

        if (requireTag && !other.CompareTag(requiredTag))
            return;

        HasTriggered = true;

        AnchorEntered?.Invoke(other);
        onAnchorEntered?.Invoke();
        onAnchorEnteredDetailed?.Invoke(this, other);
        AnyAnchorEntered?.Invoke(this, other);
    }
}
