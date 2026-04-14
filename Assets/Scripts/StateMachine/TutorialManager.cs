using UnityEngine;


public class TutorialManager : MonoBehaviour
{
    [SerializeField] StateMachine m_StateMachine;
    bool m_IsRunning;



    [SerializeField] private string m_TutorialName;
    [SerializeField] private Transform m_ParentTransform;

    public void InitTutorial()
    {    
        

        m_StateMachine.Enter();
        m_IsRunning = true;
    }


    

    void Update()
    {

        if (m_IsRunning)
        {
            m_StateMachine.Execute();

            if (m_StateMachine.IsFinished())
                End();
        }
    }

    void End()
    {
        m_IsRunning = false;
     
    }

    void OnDisable()
    {
    }

    public string GetTutorialName() => m_TutorialName;
}
