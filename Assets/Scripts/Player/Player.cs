
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    public static Player Instance;

    public bool player0 = true;
    public Racetrack racetrack;
    public GameObject canvas;
    private readonly BotPlayer.WheelState ws = new();
    [SerializeField] private LayerMask roadLayer;
    public bool hasCrashed = false;
    public bool hasFinished = false;
    [Header("Analytics")]
    public SendToGoogle analytics;
    public bool isTutorial = false;

    private Rigidbody rb;
    private RoadType currentRoadType = RoadType.Normal;
    private RoadMesh currentRoadMesh;
    private float previousSpeed = 0f;
    private Vector3 previousVel = Vector3.zero;
    private Vector3 previousAng = Vector3.zero;
    private bool analyticsAlreadySent = false;

    [Header("Drift Assist")]
    [SerializeField] private float driftAssistStrength = 5f;
    [SerializeField] private float driftAssistDuration = 3f;
    private bool wasDrifting = false;
    private float driftStartTime = 0f;

    // drift data collection
    private bool driftUsedRecently = false;
    private float lastDriftTime = 0f;
    [SerializeField] private float driftTrackingWindow = 2f;

    // turn tracking for analytics
    private bool isInTurn = false;
    private string currentTurnSegmentName = "";
    private bool driftUsedDuringTurn = false;
    private RoadMesh previousRoadMesh = null;

    // progress track
    private float raceStartTime = -1f;
    private int raceCrashCount = 0;
    private bool raceCompletionSent = false;

    [Header("Respawn (ignore)")]
    //respawn timer
    private float p0RespawnTimer = 0f;
    private float p1RespawnTimer = 0f;
    public bool p0Respawning = false;
    public bool p1Respawning = false;


    // powerups
    private int points = 0;
    private readonly bool[] powerUps = new bool[3]; // shield, disco, auto
    private readonly int[] pointsUsed = new int[] { 300, 600, 900 };

    // Shield flashing
    private float shieldFlashTimer = 0f;
    private bool shieldFlashState = true;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 100f;
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // lowers center for stability
        rb.linearDamping = BotPlayer.normalDrag;
        rb.angularDamping = 2f;
    }

    void FixedUpdate()
    {
        string speedGO = "speed1";
        if (!player0) speedGO = "speed2";
        string driftBar = player0 ? "p0bar" : "p1bar";

        p0RespawnTimer += Time.deltaTime;
        p1RespawnTimer += Time.deltaTime;

        // Track race start time (use realtimeSinceStartup to avoid scene reload issues)
        if (racetrack.lightsOutAndAwayWeGOOOOO && raceStartTime < 0f)
        {
            raceStartTime = Time.realtimeSinceStartup;
        }

        // Reset analytics flag when player is no longer crashed (after respawn)
        if (!hasCrashed) analyticsAlreadySent = false;
        else { previousSpeed = 0f; return; }

        if ((previousSpeed - rb.linearVelocity.magnitude * 2.237f >= BotPlayer.playerDeltaSpeed ||
            (player0 && Input.GetKey(KeyCode.R) && p0RespawnTimer >= 6.0f) ||
            (!player0 && Input.GetKey(KeyCode.Slash) && p1RespawnTimer >= 6.0f)) && !powerUps[2])
        {
            previousSpeed = 0;
            if (powerUps[0])
            {
                shieldFlashTimer = 0f;
                shieldFlashState = true;
                previousAng = Vector3.zero;
                previousVel = Vector3.zero;
                powerUps[0] = false;
                transform.Find("shield").gameObject.SetActive(false);
                pointsUsed[0] = 300;
                shieldFlashTimer = 0f;
                shieldFlashState = true;
                canvas.transform.Find($"{driftBar}/rightSide/shield").GetComponent<TMP_Text>().text = "Shield";
                canvas.transform.Find($"{driftBar}/rightSide/shield/text").gameObject.SetActive(true);
                return;
            }
            if (player0)
                p0Respawning = true;
            else
                p1Respawning = true;

            BotPlayer.TriggerCrash(transform, previousVel, previousAng, this, player0);
            hasCrashed = true;
            raceCrashCount++; // Track crashes for progress track
            points -= 100;
            transform.GetComponent<CloudTrail>().SetTrailActive(false, false);
            transform.Find("sparks").GetComponent<DriftSparks>().UpdateAnim(false);

            if (player0)
                p0RespawnTimer = 0f;
            else
                p1RespawnTimer = 0f;

            UpdateUI();

            // Send crash analytics (only once per crash)
            if (analytics != null && !analyticsAlreadySent && currentRoadMesh != null && !isTutorial)
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
                    if (frontLight.TryGetComponent<Light>(out var light))
                    {
                        headlightIntensity = light.intensity;
                        headlightRange = light.range;
                    }
                }

                analytics.Send(segmentType, surfaceType, eventType, playerSpeed, headlightIntensity, headlightRange, driftUsedRecently);

                // Reset turn tracking if crash happened during a turn (don't send failure analytics)
                if (isInTurn)
                {
                    isInTurn = false;
                    currentTurnSegmentName = "";
                    driftUsedDuringTurn = false;
                }

                analyticsAlreadySent = true;
            }
        }
        previousSpeed = rb.linearVelocity.magnitude * 2.237f;
        previousAng = rb.angularVelocity;
        previousVel = rb.linearVelocity;

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
            if (!raceCompletionSent && analytics != null && raceStartTime > 0f && !isTutorial)
            {
                float completionTime = Time.realtimeSinceStartup - raceStartTime;
                float progressPercentage = 100f;
                analytics.SendRaceCompletion("race_complete", completionTime, progressPercentage, raceCrashCount);
                raceCompletionSent = true;
            }

            return;
        }

        if (!racetrack.lightsOutAndAwayWeGOOOOO || hasCrashed) return;

        BotPlayer.RotateWheels(rb, transform.Find("Intact"), ws);

        (int wheelsInContact, RoadMesh roadMesh) = BotPlayer.IsGrounded(transform.gameObject, BotPlayer.groundCheckDistance, roadLayer);

        if (wheelsInContact == 0 && !powerUps[2]) //Apply downforce when player is on air
        {
            float constantDownforce = 500f;
            float velocityDownforce = BotPlayer.downforce * rb.linearVelocity.magnitude;
            rb.AddForce((constantDownforce + velocityDownforce) * Vector3.down, ForceMode.Force);
            return;
        }
        else
        {
            if (wheelsInContact == -1) wheelsInContact = 2;
            if (roadMesh != null)
            {
                currentRoadType = roadMesh.roadType;
                currentRoadMesh = roadMesh; // Track current segment for analytics

                // Turn tracking: detect entering/exiting turns
                if (currentRoadMesh != previousRoadMesh)
                {
                    // Check if current segment is a turn (ends with L/R or contains "Turn")
                    bool isCurrentSegmentTurn = currentRoadMesh.segmentName.EndsWith("L") ||
                                                currentRoadMesh.segmentName.EndsWith("R") ||
                                                currentRoadMesh.segmentName.Contains("Turn");

                    // Check if entering a turn
                    if (!isInTurn && isCurrentSegmentTurn)
                    {
                        isInTurn = true;
                        currentTurnSegmentName = currentRoadMesh.segmentName;
                        driftUsedDuringTurn = false;
                    }
                    // Check if exiting a turn (was in turn, now not)
                    else if (isInTurn && !isCurrentSegmentTurn)
                    {
                        // Turn completed successfully (no crash during turn)
                        if (analytics != null && !isTutorial)
                        {
                            analytics.SendTurnAnalytics(currentTurnSegmentName, driftUsedDuringTurn);
                        }

                        isInTurn = false;
                        currentTurnSegmentName = "";
                        driftUsedDuringTurn = false;
                    }

                    previousRoadMesh = currentRoadMesh;
                }
            }
            else currentRoadType = RoadType.Normal;
        }

        if (points >= 300) canvas.transform.Find($"{driftBar}/rightSide/shield").GetComponent<PowerUpsAnim>().UpdateAnim(true);
        else canvas.transform.Find($"{driftBar}/rightSide/shield").GetComponent<PowerUpsAnim>().UpdateAnim(false);

        if (points >= 600) canvas.transform.Find($"{driftBar}/rightSide/disco").GetComponent<PowerUpsAnim>().UpdateAnim(true);
        else canvas.transform.Find($"{driftBar}/rightSide/disco").GetComponent<PowerUpsAnim>().UpdateAnim(false);

        if (points >= 900) canvas.transform.Find($"{driftBar}/rightSide/auto").GetComponent<PowerUpsAnim>().UpdateAnim(true);
        else canvas.transform.Find($"{driftBar}/rightSide/auto").GetComponent<PowerUpsAnim>().UpdateAnim(false);

        if ((Input.GetKey(KeyCode.Alpha1) && points >= 300 && player0) || (Input.GetKey(KeyCode.Alpha0) && points >= 300 && !player0) || powerUps[0])
        {
            powerUps[0] = true;
            pointsUsed[0] -= 1;
            points -= 1;

            GameObject shieldObj = transform.Find("shield").gameObject;

            // Flash shield when less than 3 seconds remaining
            if (pointsUsed[0] < 180 || points < 180)
            {
                shieldFlashTimer += Time.deltaTime;
                if (shieldFlashTimer >= 0.2f) // flash every 0.2 seconds
                {
                    shieldFlashState = !shieldFlashState;
                    shieldObj.SetActive(shieldFlashState);
                    shieldFlashTimer = 0f;
                }
            }
            else
            {
                shieldObj.SetActive(true);
                shieldFlashState = true;
            }

            if (pointsUsed[0] < 0 || points < 0)
            {
                powerUps[0] = false;
                shieldObj.SetActive(false);
                pointsUsed[0] = 300;
                shieldFlashTimer = 0f;
                shieldFlashState = true;
                canvas.transform.Find($"{driftBar}/rightSide/shield").GetComponent<TMP_Text>().text = "Shield";
                canvas.transform.Find($"{driftBar}/rightSide/shield/text").gameObject.SetActive(true);
            }
            else
            {
                canvas.transform.Find($"{driftBar}/rightSide/shield").GetComponent<TMP_Text>().text = $"{pointsUsed[0] / 100f}";
                canvas.transform.Find($"{driftBar}/rightSide/shield/text").gameObject.SetActive(false);
            }
        }
        if ((Input.GetKey(KeyCode.Alpha2) && points >= 600 && player0) || (Input.GetKey(KeyCode.Alpha9) && points >= 600 && !player0) || powerUps[1])
        {
            powerUps[1] = true;
            points -= 2;
            pointsUsed[1] -= 2;
            racetrack.PartyTime(true);

            if (pointsUsed[1] < 0 || points < 0)
            {
                powerUps[1] = false;
                racetrack.PartyTime(false);
                pointsUsed[1] = 600;
                canvas.transform.Find($"{driftBar}/rightSide/disco").GetComponent<TMP_Text>().text = "Disco";
                canvas.transform.Find($"{driftBar}/rightSide/disco/text").gameObject.SetActive(true);
            }
            else
            {
                canvas.transform.Find($"{driftBar}/rightSide/disco").GetComponent<TMP_Text>().text = $"{pointsUsed[1] / 200f}";
                canvas.transform.Find($"{driftBar}/rightSide/disco/text").gameObject.SetActive(false);
            }
        }
        if ((Input.GetKey(KeyCode.Alpha3) && points >= 900 && player0) || (Input.GetKey(KeyCode.Alpha8) && points >= 900 && !player0) || powerUps[2])
        {
            powerUps[2] = true;
            points -= 3;
            pointsUsed[2] -= 3;

            target = Vector3.zero;
            AutoDriveTarget();

            if (target == Vector3.zero || pointsUsed[2] < 0 || points < 0)
            {
                powerUps[2] = false;
                rb.linearVelocity = transform.forward * BotPlayer.maxSpeed / 5f;
                pointsUsed[2] = 900;
                canvas.transform.Find($"{driftBar}/rightSide/auto").GetComponent<TMP_Text>().text = "Auto";
                canvas.transform.Find($"{driftBar}/rightSide/auto/text").gameObject.SetActive(true);
                return;
            }
            else
            {
                canvas.transform.Find($"{driftBar}/rightSide/auto").GetComponent<TMP_Text>().text = $"{pointsUsed[2] / 300f}";
                canvas.transform.Find($"{driftBar}/rightSide/auto/text").gameObject.SetActive(false);
            }

            Vector3 rawDir = target - transform.position;
            Vector3 moveDir = new Vector3(rawDir.x, 0f, rawDir.z).normalized;

            Vector3 idealPos = target;
            transform.position = Vector3.Lerp(transform.position, idealPos, 10f * Time.fixedDeltaTime);

            if (rawDir.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(rawDir.normalized, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 6f));
            }

            if (rb.linearVelocity.magnitude < BotPlayer.maxSpeed * .25f) rb.AddForce(moveDir, ForceMode.Acceleration);

            UpdateUI();
            return;
        }


        float accel = 0f;
        float steer = 0f;
        bool braking;
        bool attemptDrift;

        switch (player0)
        {
            case true:
                if (Input.GetKey(KeyCode.W) || (
                    SceneManager.GetActiveScene().name != "MultiPlayer" && Input.GetKey(KeyCode.UpArrow))) accel = 1f;
                if (Input.GetKey(KeyCode.S) || (
                    SceneManager.GetActiveScene().name != "MultiPlayer" && Input.GetKey(KeyCode.DownArrow))) accel = -.75f;

                if (Input.GetKey(KeyCode.D) || (
                    SceneManager.GetActiveScene().name != "MultiPlayer" && Input.GetKey(KeyCode.RightArrow))) steer = 1f;
                else if (Input.GetKey(KeyCode.A) || (
                    SceneManager.GetActiveScene().name != "MultiPlayer" && Input.GetKey(KeyCode.LeftArrow))) steer = -1f;

                braking = Input.GetKey(KeyCode.LeftCommand);
                if (Input.GetKey(KeyCode.LeftShift) ||
                    SceneManager.GetActiveScene().name != "MultiPlayer" && Input.GetKey(KeyCode.RightShift))
                {
                    attemptDrift = true;
                }
                else attemptDrift = false;
                break;
            case false:
                if (Input.GetKey(KeyCode.UpArrow)) accel = 1f;
                else if (Input.GetKey(KeyCode.DownArrow)) accel = -.75f;

                if (Input.GetKey(KeyCode.RightArrow)) steer = 1f;
                else if (Input.GetKey(KeyCode.LeftArrow)) steer = -1f;

                braking = Input.GetKey(KeyCode.RightCommand);
                if (Input.GetKey(KeyCode.RightShift))
                {
                    attemptDrift = true;
                }
                else attemptDrift = false;
                break;
        }


        Transform rearLights = transform.Find("lights/rear");
        if (braking || (Input.GetKey(KeyCode.S) && player0) || (Input.GetKey(KeyCode.DownArrow) && !player0)) for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 25;
        else for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 1;
        BotPlayer.TurnWheels((steer == 1) ? 30f : (steer == -1) ? -30f : 0f, ws);

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
                transform.GetComponent<CloudTrail>().SetTrailActive(true, true);
                break;
            case RoadType.Dirt:
                accelMultiplier = BotPlayer.dirtAccelMultiplier;
                steerRoadMultiplier = BotPlayer.dirtSteerMultiplier / 2f;
                lateralGripMultiplier = BotPlayer.dirtLateralGrip;
                roadDragMultiplier = BotPlayer.dirtDrag;
                transform.GetComponent<CloudTrail>().SetTrailActive(true, false);
                break;
            default:
                transform.GetComponent<CloudTrail>().SetTrailActive(false, false);
                break;
        }

        // drift only when turning + holding shift + going fast enough
        bool isSteering = Mathf.Abs(steer) > 0.1f;
        bool hasSpeed = Mathf.Abs(forwardVel) > BotPlayer.minDriftSpeed;
        bool drifting = attemptDrift && isSteering && hasSpeed;

        // drift visual effects only during actual drifting
        transform.Find("sparks").GetComponent<DriftSparks>().UpdateAnim(drifting);

        // points only during actual drifting
        if (drifting)
        {
            points += 3;
        }

        // Track drift usage for analytics
        if (drifting)
        {
            driftUsedRecently = true;
            lastDriftTime = Time.time;

            // Track drift usage during turn
            if (isInTurn)
            {
                driftUsedDuringTurn = true;
            }
        }
        else
        {
            // Check if drift was used within the tracking window
            driftUsedRecently = (Time.time - lastDriftTime) <= driftTrackingWindow;
        }

        // apply drift assist to help players navigate turns
        if (!isTutorial)
        {
            ApplyDriftAssist(drifting);
        }

        // apply drift visual effects (car tilt, camera tilt)
        GetComponent<DriftEffect>()?.UpdateDrift(drifting, steer);

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


        UpdateUI();
    }

    private void UpdateUI()
    {
        int max = 1000;
        if (points < 0) points = 0;
        if (points > max) points = max;

        string driftBar = player0 ? "p0bar" : "p1bar";
        RectTransform gasRect = canvas.transform.Find($"{driftBar}/leftSide/bar/tally").GetComponent<RectTransform>();
        float fill = Mathf.Clamp01(points / (1.0f * max));

        gasRect.anchorMin = new Vector2(gasRect.anchorMin.x, 0f);
        gasRect.anchorMax = new Vector2(gasRect.anchorMax.x, fill);

        gasRect.offsetMin = Vector2.zero;
        gasRect.offsetMax = Vector2.zero;
    }

    private Vector3 target = new();
    private int currentTargetIndex = 1;
    private void AutoDriveTarget()
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
            {
                if (isTutorial) { currentTargetIndex = 0; curveIndex = 0; tAhead = 0f; }
                else return;
            }
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
        if (raceStartTime > 0f && !hasFinished && !raceCompletionSent && analytics != null && !isTutorial)
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

