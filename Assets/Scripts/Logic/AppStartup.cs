using UnityEngine;

public class AppStartup : MonoBehaviour
{
    [SerializeField] StateMachine m_StateMachine;
    private bool m_IsRunning;
 [ContextMenu("StartTut")]
private void StartTutorial()
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
                m_IsRunning = false;
        }
    }
}
