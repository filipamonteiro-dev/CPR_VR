using UnityEngine;
using UnityEngine.UI;

public class GazeCheck : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private Image radialImage;
    [SerializeField] private float fillDuration = 3f;
    [SerializeField] private float drainDuration = 10f;

    private BoxCollider targetCollider;
    private bool hasTriggered;

    public bool IsFilled { get; private set; }

    private void Awake()
    {
        targetCollider = GetComponent<BoxCollider>();

        if (radialImage != null)
        {
            radialImage.fillAmount = 0f;
        }

        IsFilled = false;
    }

    void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                return;
            }
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool isLookingAtTarget = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            if (targetCollider != null && hit.collider == targetCollider)
            {
                isLookingAtTarget = true;
            }
        }

        UpdateRadialFill(isLookingAtTarget);
    }

    private void UpdateRadialFill(bool isLookingAtTarget)
    {
        if (radialImage == null)
        {
            return;
        }

        if (isLookingAtTarget)
        {
            radialImage.fillAmount += Time.deltaTime / fillDuration;
            radialImage.fillAmount = Mathf.Clamp01(radialImage.fillAmount);

            if (!hasTriggered && radialImage.fillAmount >= 1f)
            {
                hasTriggered = true;
                OnFill();
            }
        }
        else
        {
            radialImage.fillAmount -= Time.deltaTime / drainDuration;
            radialImage.fillAmount = Mathf.Clamp01(radialImage.fillAmount);

            if (radialImage.fillAmount <= 0f)
            {
                hasTriggered = false;
            }
        }
    }

    private void OnFill()
    {
        IsFilled = true;
    }


    private void OnDrawGizmos()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                return;
            }
        }

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, direction * maxDistance);
    }
}

