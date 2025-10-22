using UnityEngine;
using TMPro;

public class WrongWayDetector : MonoBehaviour
{
    public Racetrack racetrack;
    public GameObject warningUI;

    private float minSpeedToDetect = 10f; // player maybe just back up
    private float wrongWayThreshold = -0.3f;
    private float checkInterval = 0.2f; // check every 0.2 sec

    private Rigidbody rb;
    private float checkTimer = 0f;
    private bool isGoingWrongWay = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // hide ui at the start
        warningUI.SetActive(false);
    }

    void Update()
    {
        // no need to check every frame?
        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval)
            return;

        checkTimer = 0f;

        // Don't check if player is moving too slowly, player maybe just back up
        if (rb.linearVelocity.magnitude < minSpeedToDetect)
        {
            HideWarning();
            return;
        }

        // Don't check before race starts
        if (!racetrack.lightsOutAndAwayWeGOOOOO)
        {
            HideWarning();
            return;
        }

        int currentSection = FindClosestCurveSection(); // find section

        if (currentSection < 0 || currentSection >= racetrack.GetCurveCount())
        {
            HideWarning();
            return;
        }

        // Get the track's forward direction at this section
        BezierCurve currentCurve = racetrack.GetCurve(currentSection);
        // Get the direction of the section, mid to end
        Vector3 trackForward = currentCurve.GetTangent(0.5f).normalized;

        // Get player's movement direction
        Vector3 playerDirection = rb.linearVelocity.normalized;

        // Compare directions using dot product
        float directionDot = Vector3.Dot(playerDirection, trackForward);

        if (directionDot < wrongWayThreshold)  // player is going wrong way
        {
            ShowWarning();
        }
        else
        {
            HideWarning();
        }
    }

    private int FindClosestCurveSection()
    {
        float closestDistance = float.MaxValue;
        int closestSection = 0;

        for (int i = 0; i < racetrack.GetCurveCount(); i++)
        {
            BezierCurve curve = racetrack.GetCurve(i);
            Vector3 curvePoint = curve.GetPoint(0.5f);
            float distance = Vector3.Distance(transform.position, curvePoint);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSection = i;
            }
        }

        return closestSection;
    }

    private void ShowWarning()
    {
        if (!isGoingWrongWay && warningUI != null)
        {
            warningUI.SetActive(true);
            isGoingWrongWay = true;
        }
    }

    private void HideWarning()
    {
        if (isGoingWrongWay && warningUI != null)
        {
            warningUI.SetActive(false);
            isGoingWrongWay = false;
        }
    }
}
