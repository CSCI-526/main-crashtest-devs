
using System.Runtime.InteropServices;
using UnityEngine;

public class BotPlayer : MonoBehaviour
{
    public static float groundCheckDistance = 0.45f;
    public static float DynamicObstacles = .25f; // anything above is cutoff
    public static float intensity = 100f;
    public static float range = 50f;
    public static float wheelRadius = 0.9f;

    [Header("Car Physics")]
    public static float motorPower = 40;
    public static float botPowerMulti = 1f;
    public static float steerTorque = 200f;
    public static float maxSpeed = 98f;
    public static float brakeDrag = 1f;
    public static float normalDrag = 0.1f;
    public static float downforce = 75;

    [Header("Grip / Handling")]
    public static float normalGrip = 0.8f;
    public static float driftGrip = 0.25f;
    public static float driftDrag = 0.6f;
    public static float minDriftSpeed = 25f;
    public static float driftSteerBoost = 1.3f;

    [Header("Road Type Multipliers")]
    public static float wetAccelMultiplier = 0.6f;
    public static float wetSteerMultiplier = 1f;
    public static float wetLateralGrip = 0.15f;
    public static float wetDrag = 0f;
    public static float dirtAccelMultiplier = 0.8f;
    public static float dirtSteerMultiplier = 2f;
    public static float dirtLateralGrip = 0.6f;
    public static float dirtDrag = 0.18f;

    [Header("Explosion Settings")]
    public static float explosionForce = 100;
    public static float explosionRadius = 20f;
    public static float upwardModifier = 5f;
    public static float randomTorque = 20;
    public static float lifetime = 3f;
    public static float playerDeltaSpeed = 75f;
    public static float botDeltaSpeed = 50f;

    public static (int wheelsInContact, RoadMesh roadMesh) IsGrounded(GameObject player, float groundCheckDistance, LayerMask roadLayer)
    {
        int wheelsInContact = 0;
        RoadMesh roadMesh = null;

        Transform corners = player.transform.Find("corners");

        for (int i = 0; i < corners.childCount; i++)
        {
            Transform wheel = corners.GetChild(i);

            if (Physics.CheckSphere(wheel.position, groundCheckDistance, roadLayer))
            {
                if (Physics.Raycast(wheel.position, Vector3.down, out RaycastHit hit, groundCheckDistance + 1f, roadLayer))
                {
                    if (hit.collider.TryGetComponent<RoadMesh>(out var rm))
                        roadMesh = rm;
                }
                wheelsInContact++;
            }
        }

        if (wheelsInContact == 0)
        {
            if (Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit, 1.5f))
                if (!hit.collider.TryGetComponent<RoadMesh>(out var _)) wheelsInContact = -1;
        }

        return (wheelsInContact, roadMesh);
    }

    public static void TriggerCrash(Transform car)
    {
        // Hide the main model (the car)
        //GetComponent<MeshRenderer>().enabled = false;

        // Enable the “fragment” object (the hidden cube group)
        Transform fragments = car.Find("CrashFragments");
        GameObject fragmentClone = Instantiate(fragments.gameObject, fragments.position, fragments.rotation);
        fragmentClone.SetActive(true);

        //Transform speed = transform.Find("speed");
        //if (speed != null) speed.gameObject.SetActive(false);

        car.Find("lights").gameObject.SetActive(false);

        // Apply random physics to each cube
        foreach (Transform child in fragmentClone.transform)
        {
            if (!child.TryGetComponent(out Rigidbody rb))
                rb = child.gameObject.GetComponent<Rigidbody>();

            // Random direction & spin
            Vector3 randomDir = Random.onUnitSphere;
            rb.AddForce(randomDir * explosionForce + Vector3.up * upwardModifier, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);

            // Auto-destroy fragment after lifetime
            Destroy(child.gameObject, lifetime + Random.Range(0f, 1f));
        }
        Destroy(fragmentClone, lifetime + 1);
    }


    private static readonly Quaternion FRCorrection = Quaternion.Euler(0f, 180f, 0f);

    public static void RotateWheels(Rigidbody rb, Transform car, WheelState ws)
    {
        Transform wheelFL = car.Find("FL");
        Transform wheelFR = car.Find("FR");
        Transform wheelBL = car.Find("BL");
        Transform wheelBR = car.Find("BR");

        Vector3 velocity = rb.linearVelocity;
        float direction = Vector3.Dot(car.forward, velocity.normalized);
        float speed = velocity.magnitude;

        float circumference = 2f * Mathf.PI * wheelRadius;
        float rotationsPerSec = speed / circumference;
        float degreesPerSec = rotationsPerSec * 360f;

        float delta = degreesPerSec * Time.deltaTime;

        if (direction < 0)
        {
            delta = -delta;
            ws.reverse = true;
        }
        else
        {
            ws.reverse = false;
        }

        // Update per-wheel roll angles
        ws.wheelRollFL += delta;
        ws.wheelRollFR += delta;
        ws.wheelRollBL += delta;
        ws.wheelRollBR += delta;

        // Apply rolling + steering
        wheelFL.localRotation = Quaternion.Euler(ws.wheelRollFL, ws.currentSteerAngle, 0f);
        wheelFR.localRotation = Quaternion.Euler(ws.wheelRollFR, ws.currentSteerAngle, 0f) * FRCorrection;
        wheelBL.localRotation = Quaternion.Euler(ws.wheelRollBL, 0f, 0f);
        wheelBR.localRotation = Quaternion.Euler(ws.wheelRollBR, 0f, 0f) * FRCorrection;
    }



    public static void TurnWheels(float targetSteer, WheelState ws)
    {
        float steerSmooth = 0.1f;

        if (ws.reverse)
            targetSteer *= -1;

        ws.currentSteerAngle = Mathf.SmoothDamp(
            ws.currentSteerAngle,
            targetSteer,
            ref ws.steerVelocity,
            steerSmooth
        );
    }


    public class WheelState
    {
        public float currentSteerAngle = 0f;
        public float steerVelocity = 0f;

        public float wheelRollFL = 0f;
        public float wheelRollFR = 0f;
        public float wheelRollBL = 0f;
        public float wheelRollBR = 0f;

        public bool reverse = false;
    }
}
