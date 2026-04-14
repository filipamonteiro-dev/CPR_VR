using System.Collections.Generic;
using UnityEngine;

public class LookAround : State
{
    [SerializeField] private GameObject Checkpoint;
    [SerializeField] private GameObject WalkArea;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float rightAngle = 75f;
    [SerializeField] private float leftAngle = -75f;

    private readonly List<GameObject> spawnedCheckpoints = new List<GameObject>(2);
    private readonly List<GazeCheck> spawnedCheckpointChecks = new List<GazeCheck>(2);
    private bool allCheckpointsFilled;

    public override void Enter()
    {
        base.Enter();
        WalkArea.SetActive(true);
        allCheckpointsFilled = false;
        SpawnCheckpointsAroundPlayer();
        Debug.Log("State 2");

    }
    public override void Execute()
    {
        allCheckpointsFilled = AreAllCheckpointsFilled();
    }

    public override void ForceFinished()
    {
        allCheckpointsFilled = true;
    }

    public override bool IsFinished()
    {
        return allCheckpointsFilled;
    }

    public override void Exit()
    {
        ClearSpawnedCheckpoints();

        base.Exit();


    }

    private void SpawnCheckpointsAroundPlayer()
    {
        ClearSpawnedCheckpoints();

        if (Checkpoint == null)
        {
            return;
        }

        Transform targetTransform = ResolvePlayerTransform();
        if (targetTransform == null)
        {
            return;
        }

        Vector3 center = targetTransform.position;
        Vector3 flatForward = Vector3.ProjectOnPlane(targetTransform.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = targetTransform.forward;
        }

        SpawnAtAngle(center, flatForward, rightAngle);
        SpawnAtAngle(center, flatForward, leftAngle);
    }

    private void SpawnAtAngle(Vector3 center, Vector3 flatForward, float angle)
    {
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * flatForward;
        Vector3 spawnPosition = center + direction.normalized * radius;
        Quaternion spawnRotation = Quaternion.LookRotation(-direction, Vector3.up);

        GameObject spawned = Instantiate(Checkpoint, spawnPosition, spawnRotation);
        spawnedCheckpoints.Add(spawned);

        GazeCheck gazeCheck = spawned.GetComponent<GazeCheck>();
        if (gazeCheck != null)
        {
            spawnedCheckpointChecks.Add(gazeCheck);
        }
    }

    private Transform ResolvePlayerTransform()
    {
        if (playerTransform != null)
        {
            return playerTransform;
        }

        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        return null;
    }

    private void ClearSpawnedCheckpoints()
    {
        for (int i = 0; i < spawnedCheckpoints.Count; i++)
        {
            if (spawnedCheckpoints[i] != null)
            {
                Destroy(spawnedCheckpoints[i]);
            }
        }

        spawnedCheckpoints.Clear();
        spawnedCheckpointChecks.Clear();
    }

    private bool AreAllCheckpointsFilled()
    {
        if (spawnedCheckpointChecks.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < spawnedCheckpointChecks.Count; i++)
        {
            if (spawnedCheckpointChecks[i] == null || !spawnedCheckpointChecks[i].IsFilled)
            {
                return false;
            }
        }

        return true;
    }

}
