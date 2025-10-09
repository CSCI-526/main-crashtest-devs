// SimpleCarController.cs
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
    public GameObject startLights;
    public GameObject speed;
    public float motorPower = 2000;    // forward/backward force
    public float steerTorque = 200f;   // turning torque
    public float maxSpeed = 98;       // m/s
    public float brakeDrag = 1f;       // extra drag when braking
    public float normalDrag = 0.1f;

    // drift settings
    public float normalGrip = 0.8f;        // normal friction
    public float driftGrip = 0.25f;        // reduced grip while drifting
    public float driftSteerBoost = 1.3f;   // steering multiplier during drift
    public float driftDrag = 0.6f;         // extra drag while drifting (slows you down)
    public float minDriftSpeed = 30f;      // minimum speed to drift

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // lowers center for stability
        rb.linearDamping = normalDrag;
    }

    void FixedUpdate()
    {
        //if (startLights.activeSelf) return;
        
        float accel = 0f;
        if (Input.GetKey(KeyCode.W)) accel = 1f;
        else if (Input.GetKey(KeyCode.S)) accel = -1f;

        float steer = 0f;
        if (Input.GetKey(KeyCode.D)) steer = 1f;
        else if (Input.GetKey(KeyCode.A)) steer = -1f;

        bool braking = Input.GetKey(KeyCode.Space);

        Vector3 forward = transform.forward;

        // limit forward speed
        float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

        // drift only when turning + holding shift + going fast enough
        bool attemptDrift = Input.GetKey(KeyCode.LeftShift);
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

        speed.GetComponent<TMP_Text>().text = $"{Mathf.RoundToInt(forwardVel*2.237f)}";

    }
}
