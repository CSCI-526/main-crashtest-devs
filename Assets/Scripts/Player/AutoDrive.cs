using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Component that provides autodrive functionality for the player.
/// When activated, the car will drive itself along the track similar to a bot.
/// </summary>
public class AutoDrive : MonoBehaviour
{
    [Header("References")]
    public Racetrack racetrack;
    public LayerMask roadLayer;

    [Header("AutoDrive Settings")]
    public float groundCheckDistance = 0.75f;
    public int maxAutoDriveUses = 3; // Number of times player can use autodrive per race
    public float autoDriveDuration = 5f; // Duration in seconds

    [Header("Track Following - Advanced")]
    public float targetSwitchDistance = 5f; // Switch targets sooner for tighter turning
    public float lookaheadDistance = 25f; // How far ahead to look for path planning
    public int waypointsPerCurve = 5; // More waypoints for smoother path
    public float cornerSpeedMultiplier = 0.7f; // Speed reduction in corners

    [Header("Steering Tuning")]
    public float steerSmoothness = 8.0f; // How smooth the steering is (higher = faster response)
    public float maxSteerAngle = 25f; // Maximum steering angle before full input (lower = more aggressive)
    public float steerMultiplier = 1.5f; // Extra steering power
    
    [Header("Speed Control")]
    public float targetSpeed = 95f; // Target cruising speed
    public float cornerDetectionAngle = 20f; // Angle to start slowing for corners
    public float sharpCornerAngle = 45f; // Angle considered a sharp corner
    public float minCornerSpeed = 35f; // Minimum speed through corners

    // Internal state
    private Rigidbody rb;
    private List<Vector3> targets = new List<Vector3>();
    private int currentTargetIndex = 0;
    private RoadType currentRoadType = RoadType.Normal;
    private float currentSteer = 0f; // Smoothed steering value
    
    // Autodrive state
    private bool isAutoDriving = false;
    private float autoDriveTimer = 0f;
    private int autoDriveUsesRemaining;

    // Physics parameters (copied from player)
    private float motorPower;
    private float steerTorque;
    private float maxSpeed;
    private float brakeDrag;
    private float normalDrag;
    private float normalGrip;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        autoDriveUsesRemaining = maxAutoDriveUses;
        
        // Get physics parameters from BotPlayer static class
        motorPower = BotPlayer.motorPower;
        steerTorque = BotPlayer.steerTorque;
        maxSpeed = BotPlayer.maxSpeed;
        brakeDrag = BotPlayer.brakeDrag;
        normalDrag = BotPlayer.normalDrag;
        normalGrip = BotPlayer.normalGrip;
    }

    void Update()
    {
        // Check for spacebar input to activate autodrive (only in single player)
        if (Input.GetKeyDown(KeyCode.Space) && !isAutoDriving && autoDriveUsesRemaining > 0)
        {
            SimpleCarController playerController = GetComponent<SimpleCarController>();
            if (playerController != null && !playerController.hasCrashed && racetrack.lightsOutAndAwayWeGOOOOO)
            {
                ActivateAutoDrive();
            }
        }

        // Update autodrive timer
        if (isAutoDriving)
        {
            autoDriveTimer -= Time.deltaTime;
            if (autoDriveTimer <= 0f)
            {
                DeactivateAutoDrive();
            }
        }
    }

    void FixedUpdate()
    {
        if (!isAutoDriving) return;

        (int wheelsInContact, RoadMesh roadMesh) = BotPlayer.IsGrounded(gameObject, groundCheckDistance, roadLayer);

        if (wheelsInContact == 0)
        {
            // Apply downforce when airborne
            rb.AddForce(BotPlayer.downforce * rb.linearVelocity.magnitude * Vector3.down, ForceMode.Force);
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
        float currentSpeed = rb.linearVelocity.magnitude * 2.237f; // Speed in mph

        // --- ADVANCED TARGET SELECTION ---
        UpdateCurrentTarget();
        if (currentTargetIndex >= targets.Count) return;

        Vector3 targetPos = targets[currentTargetIndex];

        // Get BOTH current and lookahead targets
        Vector3 immediateTarget = targets[currentTargetIndex];
        Vector3 lookaheadTarget = GetLookaheadTarget();
        
        // Blend between immediate and lookahead target based on speed
        float blendFactor = Mathf.Clamp01(currentSpeed / 60f); // At low speeds focus on immediate, high speeds look ahead
        Vector3 blendedTarget = Vector3.Lerp(immediateTarget, lookaheadTarget, blendFactor);

        // --- IMPROVED STEERING ---
        Vector3 toTarget = blendedTarget - transform.position;
        toTarget.y = 0f;
        toTarget.Normalize();

        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z).normalized;
        float angleToTarget = Vector3.SignedAngle(flatForward, toTarget, Vector3.up);
        
        // Calculate desired steer with more aggressive response
        float desiredSteer = Mathf.Clamp(angleToTarget / maxSteerAngle, -1f, 1f);
        
        // Apply faster steering smoothness
        currentSteer = Mathf.Lerp(currentSteer, desiredSteer, Time.fixedDeltaTime * steerSmoothness);

        // Apply road type effects
        float accelMultiplier = 1.0f;
        float steerRoadMultiplier = 1.0f;
        float lateralGripMultiplier = 1.0f;
        float roadDragMultiplier = BotPlayer.normalDrag;

        switch (currentRoadType)
        {
            case RoadType.Wet:
                accelMultiplier = BotPlayer.wetAccelMultiplier;
                steerRoadMultiplier = BotPlayer.wetSteerMultiplier;
                lateralGripMultiplier = BotPlayer.wetLateralGrip;
                roadDragMultiplier = BotPlayer.wetDrag;
                break;
            case RoadType.Dirt:
                accelMultiplier = BotPlayer.dirtAccelMultiplier;
                steerRoadMultiplier = BotPlayer.dirtSteerMultiplier;
                lateralGripMultiplier = BotPlayer.dirtLateralGrip;
                roadDragMultiplier = BotPlayer.dirtDrag;
                break;
        }

        // --- INTELLIGENT SPEED CONTROL ---
        float desiredSpeed = CalculateDesiredSpeed(angleToTarget, currentSpeed);
        
        float accel = 0f;
        bool braking = false;

        // Accelerate if below desired speed
        if (currentSpeed < desiredSpeed - 5f)
        {
            accel = 1f;
        }
        // Brake if significantly over desired speed
        else if (currentSpeed > desiredSpeed + 10f || (Mathf.Abs(angleToTarget) > sharpCornerAngle && currentSpeed > minCornerSpeed + 10f))
        {
            braking = true;
        }
        // Gentle acceleration to maintain speed
        else if (currentSpeed < targetSpeed)
        {
            accel = 0.5f;
        }

        // Update rear lights
        Transform rearLights = transform.Find("lights/rear");
        if (rearLights != null)
        {
            if (braking)
            {
                for (int i = 0; i < 2; i++)
                {
                    Light light = rearLights.GetChild(i).GetComponent<Light>();
                    if (light != null) light.intensity = 25;
                }
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    Light light = rearLights.GetChild(i).GetComponent<Light>();
                    if (light != null) light.intensity = 1;
                }
            }
        }

        // Apply forward force with road-specific acceleration multiplier
        if (accel > 0f && forwardVel < BotPlayer.maxSpeed)
        {
            rb.AddForce(forward * accel * BotPlayer.motorPower * accelMultiplier * wheelsInContact / 4f * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        // Apply steering with road-specific multiplier and extra power
        float totalSteerPower = currentSteer * BotPlayer.steerTorque * 9.5f * steerMultiplier * steerRoadMultiplier * wheelsInContact / 4f;
        rb.AddRelativeTorque(Vector3.up * totalSteerPower * Time.fixedDeltaTime, ForceMode.Acceleration);

        // Lateral grip for cornering
        Vector3 right = transform.right;
        float lateralVel = Vector3.Dot(rb.linearVelocity, right);
        Vector3 lateralImpulse = -right * lateralVel * BotPlayer.normalGrip * lateralGripMultiplier * wheelsInContact / 4f;
        rb.AddForce(lateralImpulse, ForceMode.VelocityChange);

        // Apply drag
        float activeDrag = braking ? BotPlayer.brakeDrag : roadDragMultiplier;
        rb.linearDamping = activeDrag;

        // Debug visualization (optional)
        // Debug.DrawLine(transform.position, targetPos, Color.green);
        // Debug.DrawLine(transform.position, lookaheadTarget, Color.cyan);
    }

    /// <summary>
    /// Updates the current target index based on distance
    /// </summary>
    void UpdateCurrentTarget()
    {
        if (targets.Count == 0) return;

        float distToTarget = Vector3.Distance(transform.position, targets[currentTargetIndex]);
        
        if (distToTarget < targetSwitchDistance)
        {
            currentTargetIndex++;
            
            // Loop back or refresh targets when reaching the end
            if (currentTargetIndex >= targets.Count)
            {
                UpdateTargetPoints();
                currentTargetIndex = 0;
            }
        }
    }

    /// <summary>
    /// Gets a lookahead target point for better path prediction
    /// </summary>
    Vector3 GetLookaheadTarget()
    {
        if (targets.Count == 0) return transform.position + transform.forward * 10f;

        // Look 2-3 waypoints ahead (reduced from 5 for better turning)
        float speed = rb.linearVelocity.magnitude;
        int lookaheadCount = Mathf.Clamp(Mathf.RoundToInt(speed / 15f), 2, 3);
        
        int lookaheadIndex = Mathf.Min(currentTargetIndex + lookaheadCount, targets.Count - 1);
        
        return targets[lookaheadIndex];
    }

    /// <summary>
    /// Calculates the desired speed based on upcoming corners
    /// </summary>
    float CalculateDesiredSpeed(float angleToTarget, float currentSpeed)
    {
        float absAngle = Mathf.Abs(angleToTarget);
        
        // Straight sections - maintain target speed
        if (absAngle < cornerDetectionAngle)
        {
            return targetSpeed;
        }
        // Sharp corners - slow down significantly
        else if (absAngle > sharpCornerAngle)
        {
            return Mathf.Lerp(minCornerSpeed, targetSpeed * cornerSpeedMultiplier, 
                (90f - absAngle) / (90f - sharpCornerAngle));
        }
        // Medium corners
        else
        {
            return Mathf.Lerp(targetSpeed * cornerSpeedMultiplier, targetSpeed, 
                (sharpCornerAngle - absAngle) / (sharpCornerAngle - cornerDetectionAngle));
        }
    }

    void UpdateTargetPoints()
    {
        if (racetrack == null) return;
        
        targets.Clear();
        
        // Generate more waypoints per curve for smoother path following
        for (int i = 1; i < racetrack.GetCurveCount(); i++)
        {
            BezierCurve curve = racetrack.GetCurve(i);
            
            // Create multiple waypoints along each curve
            for (int j = 0; j < waypointsPerCurve; j++)
            {
                float t = (j + 1f) / (float)waypointsPerCurve;
                Vector3 waypoint = curve.GetPoint(t);
                targets.Add(waypoint);
            }
        }
    }

    public void ActivateAutoDrive()
    {
        if (autoDriveUsesRemaining <= 0 || isAutoDriving) return;

        isAutoDriving = true;
        autoDriveTimer = autoDriveDuration;
        autoDriveUsesRemaining--;
        currentSteer = 0f; // Reset steering smoothing

        // Update target points based on current position
        UpdateTargetPoints();
        
        // Find the best starting target - prefer targets ahead of us
        if (targets.Count > 0)
        {
            float closestDist = float.MaxValue;
            int closestIndex = 0;
            
            Vector3 forward = transform.forward;
            Vector3 position = transform.position;
            
            for (int i = 0; i < targets.Count; i++)
            {
                Vector3 toTarget = targets[i] - position;
                float dist = toTarget.magnitude;
                
                // Check if target is ahead of us
                float dot = Vector3.Dot(toTarget.normalized, forward);
                
                // Prefer targets ahead and closer
                float score = dist - (dot * 20f); // Bias towards forward targets
                
                if (score < closestDist)
                {
                    closestDist = score;
                    closestIndex = i;
                }
            }
            
            currentTargetIndex = closestIndex;
        }

        Debug.Log($"AutoDrive activated! Uses remaining: {autoDriveUsesRemaining}");
    }

    public void DeactivateAutoDrive()
    {
        if (!isAutoDriving) return;

        isAutoDriving = false;
        autoDriveTimer = 0f;

        Debug.Log("AutoDrive deactivated!");
    }

    // Public getters for UI
    public bool IsAutoDriving()
    {
        return isAutoDriving;
    }

    public int GetAutoDriveUsesRemaining()
    {
        return autoDriveUsesRemaining;
    }

    public float GetAutoDriveTimeRemaining()
    {
        return autoDriveTimer;
    }

    public int GetMaxAutoDriveUses()
    {
        return maxAutoDriveUses;
    }
}


