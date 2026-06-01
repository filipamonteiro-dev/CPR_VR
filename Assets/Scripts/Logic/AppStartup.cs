using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class AppStartup : MonoBehaviour
{
    public const string StartTutorialOnLoadKey = "StartTutorialOnLoad";

    [SerializeField] StateMachine m_StateMachine;
    [SerializeField] private InputActionReference startTutorialAction;
    private bool m_IsRunning;

    public event Action<StateMachine> TutorialStarted;
 [ContextMenu("StartTut")]
private void StartTutorial()
    {
        if (m_IsRunning)
        {
            return;
        }

        m_StateMachine.Enter();
        m_IsRunning = true;
        TutorialStarted?.Invoke(m_StateMachine);
    }

    public void StartTutorialExternal()
    {
        StartTutorial();
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt(StartTutorialOnLoadKey, 0) == 1)
        {
            PlayerPrefs.SetInt(StartTutorialOnLoadKey, 0);
            PlayerPrefs.Save();
            StartTutorial();
        }
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
