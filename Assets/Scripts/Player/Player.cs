
using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
    public bool player0 = true;
    public Racetrack racetrack;
    public GameObject canvas;
    //[SerializeField] private float groundCheckDistance = 0.75f;
    [SerializeField] private LayerMask roadLayer;
    public bool hasCrashed = false;
    public bool hasFinished = false;
    [Header("Analytics")]
    public SendToGoogle analytics;

/*
    [Header("Car Physics")]
    public float motorPower = 2000f;
    public float steerTorque = 200f;
    public float maxSpeed = 98f;
    public float brakeDrag = 1f;
    public float normalDrag = 0.1f;
    public float downforce = 100f;

    

    [Header("Grip / Handling")]
    public float normalGrip = 0.8f;
    public float driftGrip = 0.25f;
    public float driftSteerBoost = 1.3f;
    public float driftDrag = 0.6f;
    public float minDriftSpeed = 30f;

    [Header("Road Type Multipliers")]
    public float wetAccelMultiplier = 0.6f;
    public float wetSteerMultiplier = 0.95f;
    public float wetLateralGrip = 0.15f;
    public float wetDrag = 0f;

    public float dirtAccelMultiplier = 0.8f;
    public float dirtSteerMultiplier = 0.95f;
    public float dirtLateralGrip = 0.6f;
    public float dirtDrag = 0.18f;*/

    private Rigidbody rb;
    private RoadType currentRoadType = RoadType.Normal;
    private RoadMesh currentRoadMesh;
    private float previousSpeed = 0f;
    private float t = 0f;
    private bool analyticsAlreadySent = false;
    private readonly float[] points = new float[] { 0, 0 };

    [Header("Drift Assist")]
    [SerializeField] private float driftAssistStrength = 5f;
    [SerializeField] private float driftAssistDuration = 3f;
    private bool wasDrifting = false;
    private float driftStartTime = 0f;

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
        // Reset analytics flag when player is no longer crashed (after respawn)
        if (!hasCrashed)
        {
            analyticsAlreadySent = false;
        }

        if (previousSpeed - rb.linearVelocity.magnitude * 2.237f >= BotPlayer.playerDeltaSpeed)
        {
            GetComponent<CrashEffect>().TriggerCrash();
            hasCrashed = true;
            points[0] = 0;
            points[1] = 0;
            UpdateUI();

            // Send crash analytics (only once per crash)
            if (analytics != null && !analyticsAlreadySent && currentRoadMesh != null)
            {
                string segmentType = currentRoadMesh.segmentName;
                string surfaceType = currentRoadMesh.roadType.ToString();
                string eventType = "crash";
                float playerSpeed = previousSpeed;

                analytics.Send(segmentType, surfaceType, eventType, playerSpeed);
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

        float accel = 0f;
        float steer = 0f;
        bool braking;
        bool attemptDrift;
        switch (player0)
        {
            case true:
                if (Input.GetKey(KeyCode.W))
                {
                    accel = 1f;
                    points[0]++;
                    points[1] -= 5;
                }
                else points[0]--;
                if (Input.GetKey(KeyCode.S))
                {
                    accel = -.5f;
                    points[0] -= 5;
                    points[1] += 3;
                }
                else points[1]--;

                if (Input.GetKey(KeyCode.D)) steer = 1f;
                else if (Input.GetKey(KeyCode.A)) steer = -1f;

                braking = Input.GetKey(KeyCode.LeftCommand);
                if (braking)
                {
                    points[0] -= 5;
                    points[1] += 3;
                }
                else points[1]--;
                attemptDrift = Input.GetKey(KeyCode.LeftShift);
                break;
            case false:
                if (Input.GetKey(KeyCode.UpArrow)) accel = 1f;
                else if (Input.GetKey(KeyCode.DownArrow)) accel = -1f;

                if (Input.GetKey(KeyCode.RightArrow)) steer = 1f;
                else if (Input.GetKey(KeyCode.LeftArrow)) steer = -1f;

                braking = Input.GetKey(KeyCode.RightCommand);
                attemptDrift = Input.GetKey(KeyCode.RightShift);
                break;
        }
        Transform rearLights = transform.Find("lights/rear");
        if (braking || (Input.GetKey(KeyCode.S) && player0) || (Input.GetKey(KeyCode.DownArrow) && !player0)) for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 25;
        else for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 1;

        // Apply road type effects
        float accelMultiplier = 1.0f;
        float steerRoadMultiplier = 1.0f;
        float lateralGripMultiplier = 1.0f;
        float roadDragMultiplier = BotPlayer.normalDrag;

        switch (currentRoadType)
        {
            case RoadType.Wet:
                accelMultiplier = BotPlayer.wetAccelMultiplier;
                steerRoadMultiplier = BotPlayer.wetSteerMultiplier;
                lateralGripMultiplier = BotPlayer.wetLateralGrip;
                roadDragMultiplier = BotPlayer.wetDrag;
                break;
            case RoadType.Dirt:
                accelMultiplier = BotPlayer.dirtAccelMultiplier;
                steerRoadMultiplier = BotPlayer.dirtSteerMultiplier;
                lateralGripMultiplier = BotPlayer.dirtLateralGrip;
                roadDragMultiplier = BotPlayer.dirtDrag;
                break;
        }

        // drift only when turning + holding shift + going fast enough
        bool isSteering = Mathf.Abs(steer) > 0.1f;
        bool hasSpeed = Mathf.Abs(forwardVel) > BotPlayer.minDriftSpeed;
        bool drifting = attemptDrift && isSteering && hasSpeed;

        // apply drift assist to help players navigate turns
        ApplyDriftAssist(drifting);

        // apply drift visual effects (car tilt, camera tilt)
        GetComponent<DriftEffect>()?.UpdateDrift(drifting, steer);

        if (accel > 0f && forwardVel > BotPlayer.maxSpeed)
        {
            // don't add more forward force if at speed cap
        }
        else
        {
            // apply road-specific acceleration multiplier
            rb.AddForce(accel * accelMultiplier * BotPlayer.motorPower * wheelsInContact * forward / 4f * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        // steering: apply torque, boosted during drift for tighter turns
        float steerMultiplier = drifting ? BotPlayer.driftSteerBoost : 1f;
        float steerFactor = 9.5f;
        rb.AddRelativeTorque(Vector3.up * steer * BotPlayer.steerTorque * steerFactor * steerMultiplier * steerRoadMultiplier * wheelsInContact / 4f * Time.fixedDeltaTime, ForceMode.Acceleration);

        // friction: reduced grip while drifting allows sliding
        Vector3 right = transform.right;
        float lateralVel = Vector3.Dot(rb.linearVelocity, right);
        float gripStrength = (drifting ? BotPlayer.driftGrip : BotPlayer.normalGrip) * lateralGripMultiplier;
        Vector3 lateralImpulse = -right * lateralVel * gripStrength * wheelsInContact / 4f;
        rb.AddForce(lateralImpulse, ForceMode.VelocityChange);

        // braking: space for hard brake, drift also slows you down, or dirt slows you down
        float activeDrag = braking ? BotPlayer.brakeDrag : (drifting ? BotPlayer.driftDrag : roadDragMultiplier);
        rb.linearDamping = activeDrag;

        canvas.transform.Find(speedGO).GetComponent<TMP_Text>().text = $"{Mathf.Abs(Mathf.RoundToInt(rb.linearVelocity.magnitude * 2.237f))} mph";

        UpdateUI();

    }

    private void UpdateUI()
    {
        if (SceneManager.GetActiveScene().name == "MultiPlayer") return;

        float max = 40;
        for (int i = 0; i < 2; i++)
        {
            if (points[i] < 0) points[i] = 0;
            if (points[i] > max) points[i] = max;
        }

        RectTransform gasRect = canvas.transform.Find("playerStats/leftSide/gas/grey/Image").GetComponent<RectTransform>();
        float fill = Mathf.Clamp01(points[0] / max);

        gasRect.anchorMin = new Vector2(gasRect.anchorMin.x, 0f);
        gasRect.anchorMax = new Vector2(gasRect.anchorMax.x, fill);

        gasRect.offsetMin = Vector2.zero;
        gasRect.offsetMax = Vector2.zero;

        gasRect = canvas.transform.Find("playerStats/leftSide/brake/grey/Image").GetComponent<RectTransform>();
        fill = Mathf.Clamp01(points[1] / max);

        gasRect.anchorMin = new Vector2(gasRect.anchorMin.x, 0f);
        gasRect.anchorMax = new Vector2(gasRect.anchorMax.x, fill);

        gasRect.offsetMin = Vector2.zero;
        gasRect.offsetMax = Vector2.zero;

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
