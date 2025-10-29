using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public class Bot : MonoBehaviour
{
    public Racetrack racetrack;
    public LayerMask roadLayer;
    public GameObject startLights;
    public float groundCheckDistance = 0.75f;
    public bool hasCrashed = false;

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
    public float driftDrag = 0.6f;
    public float minDriftSpeed = 30f;
    public float driftSteerBoost = 1.3f;

    [Header("Road Type Multipliers")]
    public float wetAccelMultiplier = 0.6f;
    public float wetSteerMultiplier = 0.95f;
    public float wetLateralGrip = 0.15f;
    public float wetDrag = 0f;

    public float dirtAccelMultiplier = 0.8f;
    public float dirtSteerMultiplier = 0.95f;
    public float dirtLateralGrip = 0.6f;
    public float dirtDrag = 0.18f;


    [Header("Track Following")]
    public float targetSwitchDistance = 10f;
    public float turnSlowdownAngle = 30f;

    private Rigidbody rb;
    private readonly List<Vector3> targets = new();
    private int currentTargetIndex = 0;
    private RoadType currentRoadType = RoadType.Normal;
    private float previousSpeed = 0f;
    private float t = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 100f;
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
        rb.linearDamping = normalDrag;
        rb.angularDamping = 2f;

        UpdateTargetPoints();
    }

    void FixedUpdate()
    {
        if (previousSpeed - rb.linearVelocity.magnitude * 2.237f >= 50f) { GetComponent<CrashEffect>().TriggerCrash(); hasCrashed = true; }
        previousSpeed = rb.linearVelocity.magnitude * 2.237f;

         if (hasCrashed)
        {
            GameObject flashLight = transform.Find("crashLight").gameObject;
        
            if (t == 0) flashLight.GetComponent<LensFlareComponentSRP>().enabled = true;
            if (t < 1f)
            {
                t += Time.deltaTime;
                flashLight.GetComponent<Light>().intensity = 200 * (1 - t);
            } else if (t > .5f) flashLight.GetComponent<LensFlareComponentSRP>().enabled = false;
            else flashLight.GetComponent<Light>().intensity = 0;
        }

        if (!racetrack.lightsOutAndAwayWeGOOOOO || hasCrashed) return;
        else t = 0;

        (int wheelsInContact, RoadMesh roadMesh) = BotPlayer.IsGrounded(transform.gameObject, groundCheckDistance, roadLayer);

        if (wheelsInContact == 0)
        {
            rb.AddForce(downforce * rb.linearVelocity.magnitude * Vector3.down, ForceMode.Force);
            return;
        }
        else
        {
            if (wheelsInContact == -1) wheelsInContact = 4;

            if (roadMesh != null) currentRoadType = roadMesh.roadType;
            else currentRoadType = RoadType.Wet;
        }

        if (targets.Count == 0) UpdateTargetPoints();
        if (targets.Count == 0 || currentTargetIndex >= targets.Count) return;

        Vector3 forward = transform.forward;
        float forwardVel = Vector3.Dot(rb.linearVelocity, forward);
        Vector3 targetPos = targets[currentTargetIndex];

        // --- SWITCH TARGET ---
        float distToTarget = Vector3.Distance(transform.position, targetPos);
        if (distToTarget < targetSwitchDistance)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= targets.Count) return;
            targetPos = targets[currentTargetIndex];
        }

        // --- STEERING ---
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;
        toTarget.Normalize();

        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z).normalized;
        float angle = Vector3.SignedAngle(flatForward, toTarget, Vector3.up);
        float steer = Mathf.Clamp(angle / 45f, -1f, 1f);

        // Apply road type effects
        float accelMultiplier = 1.0f;
        float steerRoadMultiplier = 1.0f;
        float lateralGripMultiplier = 1.0f;
        float roadDragMultiplier = normalDrag;

        switch (currentRoadType)
        {
            case RoadType.Wet:
                accelMultiplier = wetAccelMultiplier;
                steerRoadMultiplier = wetSteerMultiplier;
                lateralGripMultiplier = wetLateralGrip;
                roadDragMultiplier = wetDrag;
                break;
            case RoadType.Dirt:
                accelMultiplier = dirtAccelMultiplier;
                steerRoadMultiplier = dirtSteerMultiplier;
                lateralGripMultiplier = dirtLateralGrip;
                roadDragMultiplier = dirtDrag;
                break;
        }

        // --- ACCELERATION / BRAKING LOGIC ---
        float accel = 0f;
        bool braking = false;

        // If turning sharply, slow down
        if (Mathf.Abs(angle) > turnSlowdownAngle && forwardVel > 40f) braking = true;
        else if (forwardVel < maxSpeed) accel = 1f;

        Transform rearLights = transform.Find("lights/rear");
        if (braking) for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 100;
        else for (int i = 0; i < 2; i++) rearLights.GetChild(i).GetComponent<Light>().intensity = 50;

        // apply forward or braking force with road-specific acceleration multiplier
        if (accel > 0f && forwardVel < maxSpeed)
        {
            rb.AddForce(forward * accel * motorPower * accelMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        // Steering with road-specific multiplier
        rb.AddRelativeTorque(Vector3.up * steer * steerTorque * 9.5f * steerRoadMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);

        // gripping / drifting
        Vector3 right = transform.right;
        float lateralVel = Vector3.Dot(rb.linearVelocity, right);
        Vector3 lateralImpulse = -right * lateralVel * normalGrip * lateralGripMultiplier;
        rb.AddForce(lateralImpulse, ForceMode.VelocityChange);

        // apply drag
        float activeDrag = braking ? brakeDrag : roadDragMultiplier;
        rb.linearDamping = activeDrag;

        //Debug.DrawLine(transform.position, targetPos, Color.green);
    }

    void UpdateTargetPoints()
    {
        targets.Clear();
        for (int i = 1; i < racetrack.GetCurveCount(); i++)
        {
            BezierCurve c = racetrack.GetCurve(i);
            Vector3 mid = c.GetPoint(0.5f);
            Vector3 end = c.GetPoint(1f);
            targets.Add(mid);
            targets.Add(end);
        }
    }

    public void ChangeTarget(int newTarget)
    {
        currentTargetIndex = newTarget;
    }

    public void ChangeMotorPower(float newTarget)
    {
        motorPower = newTarget;
    }
}
