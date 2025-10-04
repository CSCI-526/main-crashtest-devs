using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player2Controller : MonoBehaviour
{
    public GameObject startLights;
    public GameObject speed;
    public float motorPower = 800f;
    public float steerTorque = 200f;
    public float maxSpeed = 40f;
    public float brakeDrag = 2f;
    public float normalDrag = 0.1f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
        rb.linearDamping = normalDrag;
    }

    void FixedUpdate()
    {
        if (startLights.activeSelf) return;

        float accel = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) accel = 1f;
        else if (Input.GetKey(KeyCode.DownArrow)) accel = -1f;

        float steer = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) steer = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) steer = -1f;

        bool braking = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);

        Vector3 forward = transform.forward;
        float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

        if (!(accel > 0f && forwardVel > maxSpeed))
        {
            rb.AddForce(forward * accel * motorPower * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        float steerFactor = 10f;
        rb.AddRelativeTorque(Vector3.up * steer * steerTorque * steerFactor * Time.fixedDeltaTime, ForceMode.Acceleration);

        Vector3 right = transform.right;
        float lateralVel = Vector3.Dot(rb.linearVelocity, right);
        Vector3 lateralImpulse = -right * lateralVel * 0.8f;
        rb.AddForce(lateralImpulse, ForceMode.VelocityChange);

        rb.linearDamping = braking ? brakeDrag : normalDrag;

        speed.GetComponent<TMP_Text>().text = $"{Mathf.RoundToInt(forwardVel * 2.237f)}";
    }
}
