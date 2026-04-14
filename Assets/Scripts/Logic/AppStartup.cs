using UnityEngine;

public class AppStartup : MonoBehaviour
{
    [SerializeField] StateMachine m_StateMachine;
 [ContextMenu("StartTut")]
private void StartTutorial()
    {
        m_StateMachine.Enter();
    }
}
