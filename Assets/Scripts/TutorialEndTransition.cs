using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialEndTransition : MonoBehaviour
{
    [SerializeField] private StateMachine m_StateMachine;
    [SerializeField] private FadeScript m_FadeScript;
    [SerializeField] private string m_MainLevelScene = "MainLevel";
    [SerializeField] private float m_LoadDelay;

    private bool m_HasStarted;
    private bool m_HasTriggered;

    private void Update()
    {
        if (m_HasTriggered || m_StateMachine == null)
        {
            return;
        }

        if (!m_HasStarted)
        {
            if (m_StateMachine.CurrentState != null || !m_StateMachine.IsFinished())
            {
                m_HasStarted = true;
            }
            else
            {
                return;
            }
        }

        if (!m_StateMachine.IsFinished())
        {
            return;
        }

        m_HasTriggered = true;

        if (m_FadeScript != null)
        {
            m_FadeScript.Fade(true);
        }

        if (m_LoadDelay > 0f)
        {
            Invoke(nameof(LoadMainLevel), m_LoadDelay);
        }
        else
        {
            LoadMainLevel();
        }
    }

    private void LoadMainLevel()
    {
        if (string.IsNullOrWhiteSpace(m_MainLevelScene))
        {
            return;
        }

        SceneManager.LoadScene(m_MainLevelScene);
    }
}
