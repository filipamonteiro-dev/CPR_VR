using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class Start : State
{
    public List<GameObject> ObjectsToSpawnIn;

    [SerializeField] TeleportationAnchor tpSpot;
    private bool hasTeleportedToSpot;


    public override void Enter()
    {
        base.Enter();

        hasTeleportedToSpot = false;
        foreach (var game in ObjectsToSpawnIn)
        {
            game.SetActive(true);
        }
        if (tpSpot != null)
        {
            tpSpot.teleporting.AddListener(OnTeleportingToSpot);
        }


    }
    public override void Execute()
    {
    }

    public override void ForceFinished()
    {
        hasTeleportedToSpot = true;
    }

    public override bool IsFinished()
    {
        return hasTeleportedToSpot;
    }

    public override void Exit()
    {
        if (tpSpot != null)
        {
            tpSpot.teleporting.RemoveListener(OnTeleportingToSpot);
        }

        base.Exit();


    }

    private void OnTeleportingToSpot(TeleportingEventArgs args)
    {
        hasTeleportedToSpot = true;
    }

}
