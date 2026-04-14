using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportationActivator : MonoBehaviour
{
    public XRRayInteractor teleport;
    public InputActionProperty teleportAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        teleport.gameObject.SetActive(false);

        teleportAction.action.performed += Action_performed;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        teleport.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (teleportAction.action.WasReleasedThisFrame())
        {
            teleport.gameObject.SetActive(false);
        }
    }
}
