using System.Collections.Generic;
using UnityEngine;

public class MannequinResetter : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private Collider allowedArea;
    [SerializeField] private float maxSecondsOutside = 3f;

    [Header("Ragdoll")]
    [SerializeField] private Rigidbody[] ragdollBodies;

    private Vector3 _rootStartPosition;
    private Quaternion _rootStartRotation;
    private float _outsideTimer;

    private struct BodyPose
    {
        public Transform Transform;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public bool WasKinematic;
    }

    private readonly List<BodyPose> _poses = new List<BodyPose>();

    private void Awake()
    {
        _rootStartPosition = transform.position;
        _rootStartRotation = transform.rotation;

        if (ragdollBodies == null || ragdollBodies.Length == 0)
        {
            ragdollBodies = GetComponentsInChildren<Rigidbody>();
        }

        _poses.Clear();
        foreach (var rb in ragdollBodies)
        {
            if (rb == null)
            {
                continue;
            }

            _poses.Add(new BodyPose
            {
                Transform = rb.transform,
                LocalPosition = rb.transform.localPosition,
                LocalRotation = rb.transform.localRotation,
                WasKinematic = rb.isKinematic
            });
        }
    }

    private void Update()
    {
        if (allowedArea == null)
        {
            return;
        }

        if (!IsAnyBodyOutside())
        {
            _outsideTimer = 0f;
            return;
        }

        _outsideTimer += Time.deltaTime;
        if (_outsideTimer >= maxSecondsOutside)
        {
            ResetMannequin();
        }
    }

    private bool IsAnyBodyOutside()
    {
        if (ragdollBodies == null || ragdollBodies.Length == 0)
        {
            return !IsInsideArea(transform.position);
        }

        foreach (var rb in ragdollBodies)
        {
            if (rb == null)
            {
                continue;
            }

            if (!IsInsideArea(rb.worldCenterOfMass))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInsideArea(Vector3 point)
    {
        // ClosestPoint returns the point itself when inside the collider volume.
        var closest = allowedArea.ClosestPoint(point);
        return closest == point;
    }

    private void ResetMannequin()
    {
        _outsideTimer = 0f;

        transform.position = _rootStartPosition;
        transform.rotation = _rootStartRotation;

        foreach (var pose in _poses)
        {
            if (pose.Transform == null)
            {
                continue;
            }

            var rb = pose.Transform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            pose.Transform.localPosition = pose.LocalPosition;
            pose.Transform.localRotation = pose.LocalRotation;

            if (rb != null)
            {
                rb.isKinematic = pose.WasKinematic;
                rb.Sleep();
            }
        }
    }
}
