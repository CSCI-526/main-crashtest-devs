using UnityEngine;

public class DriftEffect : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float cameraTiltAngle = 12f; // camera tilt during drift
    [SerializeField] private float tiltSpeed = 5f;

    private Camera playerCamera;
    private float currentCameraTilt = 0f;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    public void UpdateDrift(bool isDrifting, float steerInput)
    {
        float targetCameraTilt = 0f;

        if (isDrifting)
        {
            // tilt camera in the direction of the turn
            targetCameraTilt = -steerInput * cameraTiltAngle;
        }

        currentCameraTilt = Mathf.Lerp(currentCameraTilt, targetCameraTilt, Time.deltaTime * tiltSpeed);

        // apply camera rotation on Z axis
        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = new Vector3(
                playerCamera.transform.localEulerAngles.x,
                playerCamera.transform.localEulerAngles.y,
                currentCameraTilt
            );
        }
    }
}
