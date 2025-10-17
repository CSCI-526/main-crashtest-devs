using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Bot : MonoBehaviour
{
    public Racetrack racetrack;
    public LayerMask roadLayer;
    public GameObject startLights;
    public float groundCheckDistance = 0.75f;

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
        if (!racetrack.lightsOutAndAwayWeGOOOOO) return;

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, roadLayer);

        // Detect road type when grounded
        if (isGrounded)
        {
            RoadMesh roadMesh = hit.collider.GetComponent<RoadMesh>();
            if (roadMesh != null)
            {
                currentRoadType = roadMesh.roadType;
            }
        }

        // Apply downforce only when in the air
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * downforce * rb.linearVelocity.magnitude, ForceMode.Force);
            return;
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
}
