
using UnityEngine;

public class BotPlayer : MonoBehaviour
{
    public static float groundCheckDistance = 0.45f;
    public static float DynamicObstacles = .25f; // anything above is cutoff
    public static float intensity = 100f;
    public static float range = 50f;

    [Header("Car Physics")]
    public static float motorPower = 40;
    public static float botPowerMulti = 1f;
    public static float steerTorque = 200f;
    public static float maxSpeed = 98f;
    public static float brakeDrag = 1f;
    public static float normalDrag = 0.1f;
    public static float downforce =  75;

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
}
