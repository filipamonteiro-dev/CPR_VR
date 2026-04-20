using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject menuUI;
    public GameObject tutorialUI;

    public enum AppState
    {
        Menu,
        Tutorial
    }

    private AppState currentState; // Adicionado campo para armazenar o estado atual

    void Start()
    {
        SetState(AppState.Menu);
    }

    public void SetState(AppState newState)
    {
        currentState = newState;

        menuUI.SetActive(newState == AppState.Menu);
        tutorialUI.SetActive(newState == AppState.Tutorial);
    }
}