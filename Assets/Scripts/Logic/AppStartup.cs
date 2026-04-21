using UnityEngine;
using UnityEngine.InputSystem;

public class AppStartup : MonoBehaviour
{
    [SerializeField] StateMachine m_StateMachine;
    [SerializeField] private InputActionReference startTutorialAction;
    private bool m_IsRunning;
 [ContextMenu("StartTut")]
private void StartTutorial()
    {
        if (m_IsRunning)
        {
            return;
        }

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

    private void OnEnable()
    {
        if (startTutorialAction != null)
        {
            startTutorialAction.action.performed += OnStartTutorialPerformed;
            startTutorialAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (startTutorialAction != null)
        {
            startTutorialAction.action.performed -= OnStartTutorialPerformed;
            startTutorialAction.action.Disable();
        }
    }

    private void OnStartTutorialPerformed(InputAction.CallbackContext context)
    {
        StartTutorial();
    }
}
