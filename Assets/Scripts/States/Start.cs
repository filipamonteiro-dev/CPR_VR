using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class Start : State
{
    public List<GameObject> ObjectsToSpawnIn;

    [SerializeField] TeleportationAnchor tpSpot;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CharacterController playerCharacterController;
    [SerializeField] private Collider anchorCollider;
    [SerializeField] private float insideTolerance = 0.02f;

    private bool hasTeleportedToSpot;


    public override void Enter()
    {
        base.Enter();

        hasTeleportedToSpot = false;
        foreach (var game in ObjectsToSpawnIn)
        {
            game.SetActive(true);
        }

         AnchorEnterNotifier.AnyAnchorEntered += OnAnyAnchorEntered;


    }

    private void OnAnyAnchorEntered(AnchorEnterNotifier notifier, Collider collider)
    {
         if (notifier.AnchorId == "MoveCloser")
        {
            hasTeleportedToSpot = true;
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
        Debug.Log("Saiu do estado 1");
        return hasTeleportedToSpot;
    }

    public override void Exit()
    {
        base.Exit();


    }

   
   
}
