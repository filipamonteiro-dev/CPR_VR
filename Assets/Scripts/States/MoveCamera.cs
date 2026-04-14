using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


    public class MoveCamera : State
    {

        
        [SerializeField]InputActionReference m_LeftTurnAction;
        [SerializeField]InputActionReference m_RightTurnAction;






        private Transform OpenDocPoint;

        private bool isfirstload = false;
        private bool m_HasTurned = false;

    

 
        public override void Enter()
        {
            base.Enter();




            m_HasTurned=false;
         
         
        }

        public override void Execute()
        {
           
            

        }

        public override void Exit()
        {
            base.Exit();
           

        }

        public override void ForceFinished()
        {
        }

        public override bool IsFinished()
        {
           return false;
            
        }

       
    }

