
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
    public bool player0 = true;
    public GameObject startLights;
    public GameObject speed;
    public float motorPower = 2000;    // forward/backward force
    public float steerTorque = 200f;   // turning torque
    public float maxSpeed = 98;       // m/s
    public float brakeDrag = 1f;       // extra drag when braking
    public float normalDrag = 0.1f;
    [SerializeField] private float groundCheckDistance = .75f;
    [SerializeField] private LayerMask roadLayer;

    // drift settings
    public float normalGrip = 0.8f;        // normal friction
    public float driftGrip = 0.25f;        // reduced grip while drifting
    public float driftSteerBoost = 1.3f;   // steering multiplier during drift
    public float driftDrag = 0.6f;         // extra drag while drifting (slows you down)
    public float minDriftSpeed = 30f;      // minimum speed to drift

    Rigidbody rb;
    //Racetrack racetrack;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // lowers center for stability
        rb.linearDamping = normalDrag;
        //racetrack = FindFirstObjectByType<Racetrack>();
    }

    void FixedUpdate()
    {
        Vector3 forward = transform.forward;
        float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

        speed.GetComponent<TMP_Text>().text = $"{Mathf.Abs(Mathf.RoundToInt(rb.linearVelocity.magnitude * 2.237f))}";

        //if (startLights.activeSelf) return;
        if (!Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, roadLayer)) return;

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
            rb.AddForce(forward * accel * motorPower * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        // steering: apply torque, boosted during drift for tighter turns
        float steerMultiplier = drifting ? driftSteerBoost : 1f;
        float steerFactor = 9.5f; //Mathf.Clamp01(Mathf.Abs(forwardVel) / 1f);
        rb.AddRelativeTorque(Vector3.up * steer * steerTorque * steerFactor * steerMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);

        // friction: reduced grip while drifting allows sliding
        Vector3 right = transform.right;
        float lateralVel = Vector3.Dot(rb.linearVelocity, right);
        float gripStrength = drifting ? driftGrip : normalGrip;
        Vector3 lateralImpulse = -right * lateralVel * gripStrength;
        rb.AddForce(lateralImpulse, ForceMode.VelocityChange);

        // braking: space for hard brake, drift also slows you down
        rb.linearDamping = braking ? brakeDrag : (drifting ? driftDrag : normalDrag);

        speed.GetComponent<TMP_Text>().text = $"{Mathf.Abs(Mathf.RoundToInt(rb.linearVelocity.magnitude * 2.237f))}";
    }

}
