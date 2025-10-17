
using System.Data.Common;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
    public bool player0 = true;
    public Racetrack racetrack;
    public GameObject startLights;
    public GameObject speed;
    [SerializeField] private float groundCheckDistance = 0.75f;
    [SerializeField] private LayerMask roadLayer;

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
    public float wetAccelMultiplier = 1.5f;      // accelerates faster
    public float wetSteerMultiplier = 1.5f;      // turns easier
    public float wetLateralGrip = 0.4f;          // slides more

    public float dirtAccelMultiplier = 0.7f;
    public float dirtSteerMultiplier = 1.0f;
    public float dirtLateralGrip = 0.8f;
    public float dirtDrag = 0.12f;

    private Rigidbody rb;
    private RoadType currentRoadType = RoadType.Normal;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 100f;
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // lowers center for stability
        rb.linearDamping = normalDrag;
        rb.angularDamping = 2f;
    }

    void FixedUpdate()
    {
        Vector3 forward = transform.forward;
        float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

        speed.GetComponent<TMP_Text>().text = $"{Mathf.Abs(Mathf.RoundToInt(rb.linearVelocity.magnitude * 2.237f))}";

        if (!racetrack.lightsOutAndAwayWeGOOOOO) return;

        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, roadLayer);

        // Detect road type when grounded
        if (isGrounded)
        {
            RoadMesh roadMesh = hit.collider.GetComponent<RoadMesh>();
            if (roadMesh != null)
            {
                currentRoadType = roadMesh.roadType;
                // Debug: Show current road type
                Debug.Log($"Current Road Type: {currentRoadType}");
            }
        }

        // Apply downforce only when in the air
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * downforce * rb.linearVelocity.magnitude, ForceMode.Force);
            return;
        }

        // Don't allow movement during reset
        //if (racetrack != null && racetrack.IsPlayerDuringReset(0)) return;

        float accel = 0f;
        float steer = 0f;
        bool braking;
        bool attemptDrift;
        switch (player0)
        {
            case true:
                if (Input.GetKey(KeyCode.W)) accel = 1f;
                else if (Input.GetKey(KeyCode.S)) accel = -.5f;

                if (Input.GetKey(KeyCode.D)) steer = 1f;
                else if (Input.GetKey(KeyCode.A)) steer = -1f;

                braking = Input.GetKey(KeyCode.LeftCommand);
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

        // limit forward speed
        //float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

        // Apply road type effects
        float accelMultiplier = 1.0f;
        float steerRoadMultiplier = 1.0f;
        float lateralGripMultiplier = 1.0f;
        float roadDragMultiplier = normalDrag;

        switch (currentRoadType)
        {
            case RoadType.Wet:
                accelMultiplier = wetAccelMultiplier;      // faster acceleration
                steerRoadMultiplier = wetSteerMultiplier;  // more responsive steering
                lateralGripMultiplier = wetLateralGrip;    // slides more
                break;
            case RoadType.Dirt:
                accelMultiplier = dirtAccelMultiplier;     // makes you slower
                break;
        }

        // drift only when turning + holding shift + going fast enough
        bool isSteering = Mathf.Abs(steer) > 0.1f;
        bool hasSpeed = Mathf.Abs(forwardVel) > minDriftSpeed;
        bool drifting = attemptDrift && isSteering && hasSpeed;
        if (accel > 0f && forwardVel > maxSpeed)
        {
            // don't add more forward force if at speed cap
        }
        else
        {
            // apply road-specific acceleration multiplier
            rb.AddForce(forward * accel * motorPower * accelMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        // steering: apply torque, boosted during drift for tighter turns
        float steerMultiplier = drifting ? driftSteerBoost : 1f;
        float steerFactor = 9.5f;
        rb.AddRelativeTorque(Vector3.up * steer * steerTorque * steerFactor * steerMultiplier * steerRoadMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);

        // friction: reduced grip while drifting allows sliding
        Vector3 right = transform.right;
        float lateralVel = Vector3.Dot(rb.linearVelocity, right);
        float gripStrength = (drifting ? driftGrip : normalGrip) * lateralGripMultiplier;
        Vector3 lateralImpulse = -right * lateralVel * gripStrength;
        rb.AddForce(lateralImpulse, ForceMode.VelocityChange);

        // braking: space for hard brake, drift also slows you down, or dirt slows you down
        float activeDrag = braking ? brakeDrag : (drifting ? driftDrag : roadDragMultiplier);
        rb.linearDamping = activeDrag;

        speed.GetComponent<TMP_Text>().text = $"{Mathf.Abs(Mathf.RoundToInt(rb.linearVelocity.magnitude * 2.237f))}";
    }

}
