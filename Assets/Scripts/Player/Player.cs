
using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
    public bool player0 = true;
    public Racetrack racetrack;
    public GameObject canvas;
    [SerializeField] private LayerMask roadLayer;
    public bool hasCrashed = false;
    public bool hasFinished = false;
    [Header("Analytics")]
    public SendToGoogle analytics;

    private Rigidbody rb;
    private RoadType currentRoadType = RoadType.Normal;
    private RoadMesh currentRoadMesh;
    private float previousSpeed = 0f;
    private float t = 0f;
    private bool analyticsAlreadySent = false;
    private float points = 0f;
    //private AutoDrive autoDrive;

    [Header("Drift Assist")]
    [SerializeField] private float driftAssistStrength = 5f;
    [SerializeField] private float driftAssistDuration = 3f;
    private bool wasDrifting = false;
    private float driftStartTime = 0f;

    // drift data collection
    private bool driftUsedRecently = false;
    private float lastDriftTime = 0f;
    [SerializeField] private float driftTrackingWindow = 2f;

    // progress track
    private float raceStartTime = -1f;
    private int raceCrashCount = 0;
    private bool raceCompletionSent = false;

    //respawn timer
    private float p0RespawnTimer = 0f;
    private float p1RespawnTimer = 0f;



    private bool shield = false;
    private bool isAutoDriveActive = false;
    private int pointsUsed = 500;




    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 100f;
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // lowers center for stability
        rb.linearDamping = BotPlayer.normalDrag;
        rb.angularDamping = 2f;

        // Get AutoDrive component (only for single player)
        //autoDrive = GetComponent<AutoDrive>();
    }

    void FixedUpdate()
    {
        string speedGO = "speed1";
        if (!player0) speedGO = "speed2";

        p0RespawnTimer += Time.deltaTime;
        p1RespawnTimer += Time.deltaTime;

        // Track race start time (use realtimeSinceStartup to avoid scene reload issues)
        if (racetrack.lightsOutAndAwayWeGOOOOO && raceStartTime < 0f)
        {
            raceStartTime = Time.realtimeSinceStartup;
        }

        // Reset analytics flag when player is no longer crashed (after respawn)
        if (!hasCrashed)
        {
            analyticsAlreadySent = false;
        }

        if ((previousSpeed - rb.linearVelocity.magnitude * 2.237f >= BotPlayer.playerDeltaSpeed ||
            (player0 && Input.GetKey(KeyCode.R) && p0RespawnTimer >= 5.0f) ||
            (!player0 && Input.GetKey(KeyCode.Slash) && p1RespawnTimer >= 5.0f)) && !isAutoDriveActive)
        {
            if (shield)
            {
                shield = false;
                transform.Find("shield").gameObject.SetActive(false);
                previousSpeed = 0;
                return;
            }
            GetComponent<CrashEffect>().TriggerCrash();
            hasCrashed = true;
            raceCrashCount++; // Track crashes for progress track
            points -= 10;

            if (player0)
                p0RespawnTimer = 0f;
            else
                p1RespawnTimer = 0f;

            UpdateUI();

            // Send crash analytics (only once per crash)
            if (analytics != null && !analyticsAlreadySent && currentRoadMesh != null)
            {
                string segmentType = currentRoadMesh.segmentName;
                string surfaceType = currentRoadMesh.roadType.ToString();
                string eventType = "crash";
                float playerSpeed = previousSpeed;

                // FOV data from headlights at crash time
                float headlightIntensity = -1f;
                float headlightRange = -1f;
                Transform frontLight = transform.Find("lights/front/light 0");
                if (frontLight != null)
                {
                    Light light = frontLight.GetComponent<Light>();
                    if (light != null)
                    {
                        headlightIntensity = light.intensity;
                        headlightRange = light.range;
                    }
                }

                analytics.Send(segmentType, surfaceType, eventType, playerSpeed, headlightIntensity, headlightRange, driftUsedRecently);
                analyticsAlreadySent = true;
            }
        }
        previousSpeed = rb.linearVelocity.magnitude * 2.237f;

        if (hasCrashed)
        {
            GameObject flashLight = transform.Find("crashLight").gameObject;

            if (t == 0) flashLight.GetComponent<LensFlareComponentSRP>().enabled = true;
            if (t < 1f)
            {
                t += Time.deltaTime;
                flashLight.GetComponent<Light>().intensity = 200 * (1 - t);
            }
            else if (t > .5f) flashLight.GetComponent<LensFlareComponentSRP>().enabled = false;
            else flashLight.GetComponent<Light>().intensity = 0;
        }

        Vector3 forward = transform.forward;
        float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

        canvas.transform.Find(speedGO).GetComponent<TMP_Text>().text = $"{Mathf.Abs(Mathf.RoundToInt(rb.linearVelocity.magnitude * 2.237f))} mph";

        // freeze physics when player finishes the race
        if (hasFinished)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            // Send race completion analytics (only once)
            if (!raceCompletionSent && analytics != null && raceStartTime > 0f)
            {
                float completionTime = Time.realtimeSinceStartup - raceStartTime;
                float progressPercentage = 100f;
                analytics.SendRaceCompletion("race_complete", completionTime, progressPercentage, raceCrashCount);
                raceCompletionSent = true;
            }

            return;
        }

        if (!racetrack.lightsOutAndAwayWeGOOOOO || hasCrashed) return;
        else t = 0;

        (int wheelsInContact, RoadMesh roadMesh) = BotPlayer.IsGrounded(transform.gameObject, BotPlayer.groundCheckDistance, roadLayer);

        if (wheelsInContact == 0)
        {
            rb.AddForce(BotPlayer.downforce * rb.linearVelocity.magnitude * Vector3.down, ForceMode.Force);
            return;
        }
        else
        {
            if (wheelsInContact == -1) wheelsInContact = 2;
            if (roadMesh != null)
            {
                currentRoadType = roadMesh.roadType;
                currentRoadMesh = roadMesh; // Track current segment for analytics
            }
            else currentRoadType = RoadType.Wet;
        }

        if (SceneManager.GetActiveScene().name != "MultiPlayer")
        {
            if (points >= 499)
            {
                canvas.transform.Find("playerStats/leftSide/shield").GetComponent<TMP_Text>().color = Color.white;
                canvas.transform.Find("playerStats/leftSide/shield/text").GetComponent<TMP_Text>().color = Color.white;

                if (points >= 999)
                {
                    canvas.transform.Find("playerStats/leftSide/auto").GetComponent<TMP_Text>().color = Color.white;
                    canvas.transform.Find("playerStats/leftSide/auto/text").GetComponent<TMP_Text>().color = Color.white;
                }
                else
                {
                    canvas.transform.Find("playerStats/leftSide/auto").GetComponent<TMP_Text>().color = Color.gray;
                    canvas.transform.Find("playerStats/leftSide/auto/text").GetComponent<TMP_Text>().color = Color.gray;
                }
            }
            else
            {
                canvas.transform.Find("playerStats/leftSide/shield").GetComponent<TMP_Text>().color = Color.gray;
                canvas.transform.Find("playerStats/leftSide/shield/text").GetComponent<TMP_Text>().color = Color.gray;

                canvas.transform.Find("playerStats/leftSide/auto").GetComponent<TMP_Text>().color = Color.gray;
                canvas.transform.Find("playerStats/leftSide/auto/text").GetComponent<TMP_Text>().color = Color.gray;

            }


            if (Input.GetKey(KeyCode.Alpha1) && points >= 499 || shield)
            {
                shield = true;
                pointsUsed -= 2;
                points -= 2;
                transform.Find("shield").gameObject.SetActive(true);
                if (pointsUsed < 0)
                {
                    shield = false;
                    transform.Find("shield").gameObject.SetActive(false);
                    pointsUsed = 500;
                }

            }
            else if (Input.GetKey(KeyCode.Alpha2) && points >= 999 || isAutoDriveActive)
            {
                isAutoDriveActive = true;
                points -= 3;
                if (points < 0)
                {
                    isAutoDriveActive = false;
                    rb.useGravity = true;
                    rb.linearDamping = BotPlayer.normalDrag;
                    rb.angularDamping = 2f;
                    rb.linearVelocity = transform.forward * BotPlayer.maxSpeed;
                    return;
                }

                AutoDriveTHingidk();

                rb.useGravity = false;
                rb.linearDamping = 0f;
                rb.angularDamping = 0f;

                Vector3 pos = transform.position;
                transform.position = pos;
                Vector3 toTarget = target - transform.position;
                toTarget.y += .5f;

                if (toTarget.sqrMagnitude > 1f)
                {
                    float autoDriveSpeed = BotPlayer.maxSpeed * 1.1f;
                    transform.position += autoDriveSpeed * Time.fixedDeltaTime * transform.forward;

                    Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                    transform.rotation = targetRot;
                }
                else
                {
                    rb.linearVelocity = Vector3.zero;
                }
                UpdateUI();
                return;
            }
        }









        float accel = 0f;
        float steer = 0f;
        bool braking;
        bool attemptDrift;

        // Check if autodrive is active - if so, skip manual input
        //bool isAutoDriveActive = (autoDrive != null && autoDrive.IsAutoDriving());

        if (isAutoDriveActive)
        {
            // AutoDrive handles all movement, so set defaults
            braking = false;
            attemptDrift = false;
        }
        else
        {
            // Normal player input
            switch (player0)
            {
                case true:
                    if (Input.GetKey(KeyCode.W)) accel = 1f;
                    if (Input.GetKey(KeyCode.S)) accel = -.75f;

                    if (Input.GetKey(KeyCode.D)) steer = 1f;
                    else if (Input.GetKey(KeyCode.A)) steer = -1f;

                    braking = Input.GetKey(KeyCode.LeftCommand);
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        attemptDrift = true;
                        points += 2;
                    }
                    else attemptDrift = false;
                    break;
                case false:
                    if (Input.GetKey(KeyCode.UpArrow)) accel = 1f;
                    else if (Input.GetKey(KeyCode.DownArrow)) accel = -.75f;

                    if (Input.GetKey(KeyCode.RightArrow)) steer = 1f;
                    else if (Input.GetKey(KeyCode.LeftArrow)) steer = -1f;

                    braking = Input.GetKey(KeyCode.RightCommand);
                    attemptDrift = Input.GetKey(KeyCode.RightShift);
                    break;
            }
        }

        Transform rearLights = transform.Find("lights/rear");
        if (braking || (Input.GetKey(KeyCode.S) && player0) || (Input.GetKey(KeyCode.DownArrow) && !player0)) for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 25;
        else for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 1;

        // Apply road type effects
        float accelMultiplier = 1.0f;
        float steerRoadMultiplier = 1.5f;
        float lateralGripMultiplier = .9f;
        float roadDragMultiplier = BotPlayer.normalDrag;

        switch (currentRoadType)
        {
            case RoadType.Wet:
                accelMultiplier = BotPlayer.wetAccelMultiplier;
                steerRoadMultiplier = BotPlayer.wetSteerMultiplier / 2f;
                lateralGripMultiplier = BotPlayer.wetLateralGrip;
                roadDragMultiplier = BotPlayer.wetDrag;
                break;
            case RoadType.Dirt:
                accelMultiplier = BotPlayer.dirtAccelMultiplier;
                steerRoadMultiplier = BotPlayer.dirtSteerMultiplier / 2f;
                lateralGripMultiplier = BotPlayer.dirtLateralGrip;
                roadDragMultiplier = BotPlayer.dirtDrag;
                break;
        }

        // drift only when turning + holding shift + going fast enough
        bool isSteering = Mathf.Abs(steer) > 0.1f;
        bool hasSpeed = Mathf.Abs(forwardVel) > BotPlayer.minDriftSpeed;
        bool drifting = attemptDrift && isSteering && hasSpeed;

        // Track drift usage for analytics
        if (drifting)
        {
            driftUsedRecently = true;
            lastDriftTime = Time.time;
        }
        else
        {
            // Check if drift was used within the tracking window
            driftUsedRecently = (Time.time - lastDriftTime) <= driftTrackingWindow;
        }

        // apply drift assist to help players navigate turns
        ApplyDriftAssist(drifting);

        // apply drift visual effects (car tilt, camera tilt)
        GetComponent<DriftEffect>()?.UpdateDrift(drifting, steer);

        // Skip manual physics if autodrive is active (autodrive handles it)
        if (!isAutoDriveActive)
        {

            // steering: apply torque, boosted during drift for tighter turns
            float steerMultiplier = drifting ? BotPlayer.driftSteerBoost : 1f;
            //rb.AddRelativeTorque(Vector3.up * steer * 100 * steerMultiplier * steerRoadMultiplier * wheelsInContact / 4f, ForceMode.Acceleration);

            float rotationSpeed = steer * steerRoadMultiplier * steerMultiplier * wheelsInContact / 4f;

            Quaternion turnRotation = Quaternion.Euler(0f, rotationSpeed, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);


            // friction: reduced grip while drifting allows sliding
            Vector3 right = transform.right;
            //float lateralVel = Vector3.Dot(rb.linearVelocity, right);
            float gripStrength = (drifting ? BotPlayer.driftGrip : BotPlayer.normalGrip) * lateralGripMultiplier;
            //Vector3 lateralImpulse = -right * lateralVel * gripStrength * wheelsInContact / 4f;
            //rb.AddForce(lateralImpulse, ForceMode.VelocityChange);

            Vector3 lateralVel = Vector3.Dot(rb.linearVelocity, right) * right;
            Vector3 correctedVel = rb.linearVelocity - lateralVel * gripStrength;
            rb.linearVelocity = correctedVel;

            // braking: space for hard brake, drift also slows you down, or dirt slows you down
            float activeDrag = braking ? BotPlayer.brakeDrag : (drifting ? BotPlayer.driftDrag : roadDragMultiplier);
            rb.linearDamping = activeDrag;

            if (!braking && forwardVel < BotPlayer.maxSpeed)
                rb.AddForce(accel * accelMultiplier * BotPlayer.motorPower * wheelsInContact * forward / 4f, ForceMode.Acceleration);

        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (SceneManager.GetActiveScene().name == "MultiPlayer") return;

        float max = 1000;
        if (points < 0) points = 0;
        if (points > max) points = max;


        RectTransform gasRect = canvas.transform.Find("playerStats/leftSide/gas/grey/Image").GetComponent<RectTransform>();
        float fill = Mathf.Clamp01(points / max);

        gasRect.anchorMin = new Vector2(gasRect.anchorMin.x, 0f);
        gasRect.anchorMax = new Vector2(gasRect.anchorMax.x, fill);

        gasRect.offsetMin = Vector2.zero;
        gasRect.offsetMax = Vector2.zero;

        /*
        gasRect = canvas.transform.Find("playerStats/leftSide/brake/grey/Image").GetComponent<RectTransform>();
        fill = Mathf.Clamp01(points / max);

        gasRect.anchorMin = new Vector2(gasRect.anchorMin.x, 0f);
        gasRect.anchorMax = new Vector2(gasRect.anchorMax.x, fill);

        gasRect.offsetMin = Vector2.zero;
        gasRect.offsetMax = Vector2.zero;*/

        // Update AutoDrive UI
        //UpdateAutoDriveUI();
    }

    private Vector3 target = new();
    private int currentTargetIndex = 1;
    private void AutoDriveTHingidk()
    {
        BezierCurve currentCurve = racetrack.GetCurve(currentTargetIndex);
        float currentT = currentCurve.GetClosestTOnCurve(transform.position);


        int curveIndex = currentTargetIndex;
        float tAhead = currentT + 0.2f;

        while (tAhead > 1f)
        {
            tAhead -= 1f;
            curveIndex++;
            if (curveIndex >= racetrack.GetCurveCount())
                curveIndex = racetrack.GetCurveCount() - 1;
        }

        BezierCurve curve = racetrack.GetCurve(curveIndex);
        target = curve.GetPoint(tAhead);
    }

    public void ChangeTarget(int newTarget)
    {
        currentTargetIndex = newTarget + 1;
    }

    private void UpdateAutoDriveUI()
    {
        /*
        if (autoDrive == null) return;

        // Update autodrive uses remaining display
        Transform autoDriveUsesTransform = canvas.transform.Find("playerStats/leftSide/autoDrive/usesText");
        if (autoDriveUsesTransform != null)
        {
            TMP_Text usesText = autoDriveUsesTransform.GetComponent<TMP_Text>();
            if (usesText != null)
            {
                usesText.text = $"{autoDrive.GetAutoDriveUsesRemaining()}/{autoDrive.GetMaxAutoDriveUses()}";
            }
        }

        // Update autodrive active indicator and timer
        Transform autoDriveActiveTransform = canvas.transform.Find("playerStats/leftSide/autoDrive/activeIndicator");
        if (autoDriveActiveTransform != null)
        {
            GameObject activeIndicator = autoDriveActiveTransform.gameObject;
            if (autoDrive.IsAutoDriving())
            {
                activeIndicator.SetActive(true);
                TMP_Text timerText = autoDriveActiveTransform.Find("timerText")?.GetComponent<TMP_Text>();
                if (timerText != null)
                {
                    timerText.text = $"{Mathf.Ceil(autoDrive.GetAutoDriveTimeRemaining())}s";
                }
            }
            else
            {
                activeIndicator.SetActive(false);
            }
        }*/
    }

    private void OnDestroy() // to check if game shut down before finished
    {
        // Send dropout analytics if race started but not finished
        if (raceStartTime > 0f && !hasFinished && !raceCompletionSent && analytics != null)
        {
            float elapsedTime = Time.realtimeSinceStartup - raceStartTime;

            // Safety check: Only send if elapsed time is positive and reasonable
            if (elapsedTime > 0f && elapsedTime < 3600f) // Max 1 hour
            {
                float progressPercentage = CalculateProgressPercentage();
                analytics.SendRaceCompletion("dropout", elapsedTime, progressPercentage, raceCrashCount);
                raceCompletionSent = true;
            }
        }
    }

    private float CalculateProgressPercentage()
    {
        // Calculate approximate progress based on position along track
        if (racetrack == null || racetrack.GetCurveCount() == 0)
            return 0f;

        // Find closest curve to player
        int closestSection = 0;
        float closestDist = float.MaxValue;

        for (int i = 0; i < racetrack.GetCurveCount(); i++)
        {
            BezierCurve curve = racetrack.GetCurve(i);
            Vector3 closestPoint = curve.GetPoint(curve.GetClosestTOnCurve(transform.position));
            float dist = Vector3.Distance(transform.position, closestPoint);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestSection = i;
            }
        }

        // Calculate percentage: currentSection / totalSections * 100
        float progress = (closestSection / (float)(racetrack.GetCurveCount() - 1)) * 100f;
        return Mathf.Clamp(progress, 0f, 100f);
    }

    private Vector3 CalculateTargetDirection()
    {
        // get the player's current section from the racetrack

        // find closest curve point to determine where on the track
        int currentSection = 0;
        float closestDist = float.MaxValue;

        for (int i = 0; i < racetrack.GetCurveCount(); i++)
        {
            BezierCurve curve = racetrack.GetCurve(i);
            Vector3 closestPoint = curve.GetPoint(curve.GetClosestTOnCurve(transform.position));
            float dist = Vector3.Distance(transform.position, closestPoint);

            if (dist < closestDist)
            {
                closestDist = dist;
                currentSection = i;
            }
        }

        // get the target direction
        int lookAheadSection = Mathf.Min(currentSection + 2, racetrack.GetCurveCount() - 1);

        if (lookAheadSection < racetrack.GetCurveCount())
        {
            BezierCurve targetCurve = racetrack.GetCurve(lookAheadSection);
            float t = targetCurve.GetClosestTOnCurve(transform.position);

            // get forward direction
            Vector3 targetDirection = targetCurve.GetTangent(Mathf.Clamp01(t + 0.3f)).normalized;

            return new Vector3(targetDirection.x, 0f, targetDirection.z).normalized;
        }

        return transform.forward;
    }

    private void ApplyDriftAssist(bool isDrifting)
    {
        // track when drift starts
        if (isDrifting && !wasDrifting)
        {
            driftStartTime = Time.time;
        }

        wasDrifting = isDrifting;

        // only apply assist during drift and for a short duration after starting
        float timeSinceDriftStart = Time.time - driftStartTime;
        if (isDrifting && timeSinceDriftStart < driftAssistDuration)
        {
            Vector3 targetDirection = CalculateTargetDirection();
            Vector3 currentForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

            // how much to rotate toward target
            float alignmentStrength = 1f - (timeSinceDriftStart / driftAssistDuration);
            alignmentStrength = Mathf.Clamp01(alignmentStrength);

            // rotate toward target direction
            Vector3 newForward = Vector3.Slerp(currentForward, targetDirection, driftAssistStrength * alignmentStrength * Time.fixedDeltaTime);

            // apply the rotation
            if (newForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(newForward, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, driftAssistStrength * alignmentStrength * Time.fixedDeltaTime));
            }
        }
    }


}
