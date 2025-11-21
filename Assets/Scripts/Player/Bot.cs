using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public class Bot : MonoBehaviour
{
    public Racetrack racetrack;
    public LayerMask roadLayer;
    public bool hasCrashed = false;
    private readonly BotPlayer.WheelState ws = new();

    [Header("Track Following")]
    public float targetSwitchDistance = 10f;
    public float turnSlowdownAngle = 30f;

    private Rigidbody rb;
    private readonly List<Vector3> targets = new();
    private int currentTargetIndex = 1;
    private RoadType currentRoadType = RoadType.Normal;
    private float previousSpeed = 0f;
    private Vector3 previousVel = Vector3.zero;
    private Vector3 previousAng = Vector3.zero;
    private Vector3 botOffset;
    private float maxSpeedMultiplier;
    private float brakingAngle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 100f;
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
        rb.linearDamping = BotPlayer.normalDrag;
        rb.angularDamping = 2f;

        botOffset = new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
        maxSpeedMultiplier = Random.Range(.95f, 1.05f);
        brakingAngle = Random.Range(15f, 35f);
    }

    void FixedUpdate()
    {
        // administrative 

        if (previousSpeed - rb.linearVelocity.magnitude * 2.237f >= BotPlayer.botDeltaSpeed && !hasCrashed)
        {
            BotPlayer.TriggerCrash(transform, previousVel, previousAng, this);
            hasCrashed = true;
            previousSpeed = 0;
        }
        else
        {
            if (hasCrashed) previousSpeed = 0;
            else previousSpeed = rb.linearVelocity.magnitude * 2.237f;
            previousAng = rb.angularVelocity;
            previousVel = rb.linearVelocity;
        }

        if (!racetrack.lightsOutAndAwayWeGOOOOO || hasCrashed) return;

        BotPlayer.RotateWheels(rb, transform.Find("Intact"), ws);

        // ground check
        (int wheelsInContact, RoadMesh roadMesh) = BotPlayer.IsGrounded(transform.gameObject, BotPlayer.groundCheckDistance, roadLayer);

        if (wheelsInContact == 0)
        { // Apply downforce when player is on air
            float constantDownforce = 500f;
            float velocityDownforce = BotPlayer.downforce * rb.linearVelocity.magnitude;
            rb.AddForce((constantDownforce + velocityDownforce) * Vector3.down, ForceMode.Force);
            return;
        }
        else
        {
            //if (wheelsInContact == -1) wheelsInContact = 4;

            if (roadMesh != null) currentRoadType = roadMesh.roadType;
            else currentRoadType = RoadType.Normal;
        }

        // driving

        float accelMultiplier = 1.0f;
        float steerRoadMultiplier = 3.0f;
        float lateralGripMultiplier = .9f;
        float roadDragMultiplier = BotPlayer.normalDrag;

        switch (currentRoadType)
        {
            case RoadType.Wet:
                accelMultiplier = BotPlayer.wetAccelMultiplier;
                steerRoadMultiplier = BotPlayer.wetSteerMultiplier;
                lateralGripMultiplier = BotPlayer.wetLateralGrip;
                roadDragMultiplier = BotPlayer.wetDrag;
                transform.GetComponent<CloudTrail>().SetTrailActive(true, true);
                break;
            case RoadType.Dirt:
                accelMultiplier = BotPlayer.dirtAccelMultiplier;
                steerRoadMultiplier = BotPlayer.dirtSteerMultiplier;
                lateralGripMultiplier = BotPlayer.dirtLateralGrip;
                roadDragMultiplier = BotPlayer.dirtDrag;
                transform.GetComponent<CloudTrail>().SetTrailActive(true, false);
                break;
            default:
                transform.GetComponent<CloudTrail>().SetTrailActive(false, false);
                break;
        }

        UpdateTargetPoints();

        float totalWeight = 0f;
        Vector3 avgDir = Vector3.zero;

        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 toTarget = targets[i] + botOffset - transform.position;
            float dist = toTarget.magnitude;
            Vector3 dir = toTarget.normalized;

            float weight = 1f / (1f + dist);
            avgDir += dir * weight;
            totalWeight += weight;

            //Debug.DrawLine(transform.position, targets[i], Color.blue);
        }

        if (totalWeight > 0f)
            avgDir /= totalWeight;

        avgDir.y = 0f;
        avgDir.Normalize();

        //Debug.DrawLine(transform.position, transform.position + avgDir * 10f, Color.red);

        // --- Steering ---
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        float angleToTarget = Vector3.SignedAngle(forward, avgDir, Vector3.up);

        float maxSteerAngle = 30f;
        float steerAmount = Mathf.Clamp(angleToTarget / maxSteerAngle, -1f, 1f);
        BotPlayer.TurnWheels(angleToTarget, ws);

        float rotationSpeed = steerAmount * steerRoadMultiplier;

        Quaternion turnRotation = Quaternion.Euler(0f, rotationSpeed, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        // --- Lateral grip (anti-drift correction) ---
        Vector3 right = transform.right;
        Vector3 lateralVel = Vector3.Dot(rb.linearVelocity, right) * right;
        Vector3 correctedVel = rb.linearVelocity - lateralVel * lateralGripMultiplier;
        rb.linearVelocity = correctedVel;


        // --- Forward acceleration (engine) ---
        float forwardVel = Vector3.Dot(rb.linearVelocity, transform.forward);
        float absAngle = Mathf.Abs(angleToTarget);

        bool braking = absAngle > brakingAngle && forwardVel > 30f;
        float activeDrag = braking ? BotPlayer.brakeDrag : roadDragMultiplier;
        rb.linearDamping = activeDrag;

        if (!braking && forwardVel < BotPlayer.maxSpeed * maxSpeedMultiplier)
            rb.AddForce(accelMultiplier * BotPlayer.botPowerMulti * BotPlayer.motorPower * transform.forward, ForceMode.Acceleration);


        Transform rearLights = transform.Find("lights/rear");
        if (braking) for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 25;
        else for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 1;


        //Debug.DrawLine(transform.position, targetPos, Color.green);
    }

    void UpdateTargetPoints()
    {
        targets.Clear();
        if (currentTargetIndex >= racetrack.GetCurveCount()) { BotPlayer.TriggerCrash(transform, previousVel, previousAng, this); hasCrashed = true; previousSpeed = 0; return; }
        BezierCurve currentCurve = racetrack.GetCurve(currentTargetIndex);
        float currentT = currentCurve.GetClosestTOnCurve(transform.position);

        float baseLookahead = 0.25f;
        float speedFactor = Mathf.Clamp(rb.linearVelocity.magnitude / 75f, 0.5f, 2f);
        float lookaheadStep = baseLookahead * speedFactor;

        int curveIndex = currentTargetIndex;
        float tAhead = currentT;

        for (int i = 1; i <= 5; i++)
        {
            tAhead += lookaheadStep;
            while (tAhead > 1f)
            {
                tAhead -= 1f;
                curveIndex++;
            }
            if (curveIndex >= racetrack.GetCurveCount()) break;
            BezierCurve curve = racetrack.GetCurve(curveIndex);
            targets.Add(curve.GetPoint(tAhead));
        }
    }

    public void ChangeTarget(int newTarget)
    {
        currentTargetIndex = newTarget + 1;
    }

    public void ChangeMotorPower(float newTarget)
    {
        //BotPlayer.motorPower = newTarget;
    }
}
