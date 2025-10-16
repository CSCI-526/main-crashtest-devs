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

    [Header("Grip / Handling")]
    public float normalGrip = 0.8f;
    public float driftGrip = 0.25f;
    public float driftDrag = 0.6f;
    public float minDriftSpeed = 30f;
    public float driftSteerBoost = 1.3f;

    [Header("Track Following")]
    public float targetSwitchDistance = 10f;
    public float turnSlowdownAngle = 30f;

    private Rigidbody rb;
    private readonly List<Vector3> targets = new();
    private int currentTargetIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
        rb.linearDamping = normalDrag;

        UpdateTargetPoints();
    }

    void FixedUpdate()
    {
        if (!racetrack.lightsOutAndAwayWeGOOOOO) return;
        if (!Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, roadLayer)) return;
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

        // --- ACCELERATION / BRAKING LOGIC ---
        float accel = 0f;
        bool braking = false;

        // If turning sharply, slow down
        if (Mathf.Abs(angle) > turnSlowdownAngle && forwardVel > 40f) braking = true;
        else if (forwardVel < maxSpeed) accel = 1f;

        // Apply forward or braking force
        if (accel > 0f && forwardVel < maxSpeed)
            rb.AddForce(forward * accel * motorPower * Time.fixedDeltaTime, ForceMode.Acceleration);

        rb.AddRelativeTorque(Vector3.up * steer * steerTorque * 9.5f * Time.fixedDeltaTime, ForceMode.Acceleration);

        // Grip/friction
        Vector3 right = transform.right;
        float lateralVel = Vector3.Dot(rb.linearVelocity, right);
        Vector3 lateralImpulse = -right * lateralVel * normalGrip;
        rb.AddForce(lateralImpulse, ForceMode.VelocityChange);
        rb.linearDamping = braking ? brakeDrag : normalDrag;
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
}
