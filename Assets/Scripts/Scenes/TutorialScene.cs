using System.Collections;
using UnityEngine;

public class TutorialScene : MonoBehaviour
{

    public GameObject WASDTutorial;
    public GameObject DriftTutorial;
    public GameObject PowerUpTutorial;
    public GameObject PowerUpTutorial2;
    public GameObject RespawnTutorial;
    public GameObject EndTutorial;
    public Racetrack racetrack;

    private bool finishedWASDTutoral = false;
    private bool finishedDriftTutoral = false;
    private bool finishedPowerUpTutorial = false;
    private bool finishedRespawnTutorial = false;

    // Track individual WASD key presses
    private bool pressedW = false;
    private bool pressedA = false;
    private bool pressedS = false;
    private bool pressedD = false;
    private float wasdTimer = 0f;
    private const float wasdTimeLimit = 3f;

    // Track successful drifts
    private int successfulDrifts = 0;
    private int powerUpTutorialDrifts = 0;
    private bool wasDriftingLastFrame = false;
    private bool wasDriftingLastFramePowerUp = false;

    void Start()
    {
        // Skip countdown and enable player movement immediately in tutorial
        if (racetrack != null)
        {
            racetrack.lightsOutAndAwayWeGOOOOO = true;
            racetrack.countdownStage = 5; // Skip all countdown stages

            // Hide countdown text
            if (racetrack.countdownText != null)
            {
                racetrack.countdownText.gameObject.SetActive(false);
            }
        }
    }

    void FixedUpdate()
    {

        if (!finishedWASDTutoral)
        {
            if (Input.GetKey(KeyCode.W)) pressedW = true;
            if (Input.GetKey(KeyCode.A)) pressedA = true;
            if (Input.GetKey(KeyCode.S)) pressedS = true;
            if (Input.GetKey(KeyCode.D)) pressedD = true;

            wasdTimer += Time.fixedDeltaTime;

            // Check if W, A, D keys pressed OR 3 seconds have passed
            if ((pressedW && pressedA && pressedD) || wasdTimer >= wasdTimeLimit)
            {
                finishedWASDTutoral = true;
                StartCoroutine(HideWASDTutorial());
            }
        }

        if(finishedWASDTutoral && DriftTutorial.activeInHierarchy && !finishedDriftTutoral)
        {
            // Check if player is actually drifting
            GameObject player = GameObject.Find("Player 0");
            if (player != null)
            {
                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null)
                {
                    Rigidbody rb = player.GetComponent<Rigidbody>();
                    float steer = 0f;
                    if (Input.GetKey(KeyCode.D)) steer = 1f;
                    else if (Input.GetKey(KeyCode.A)) steer = -1f;

                    bool attemptDrift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    Vector3 forward = player.transform.forward;
                    float forwardVel = Vector3.Dot(rb.linearVelocity, forward);
                    bool isSteering = Mathf.Abs(steer) > 0.1f;
                    bool hasSpeed = Mathf.Abs(forwardVel) > BotPlayer.minDriftSpeed;
                    bool isDrifting = attemptDrift && isSteering && hasSpeed;

                    // Count a successful drift when player starts drifting
                    if (isDrifting && !wasDriftingLastFrame)
                    {
                        successfulDrifts++;
                    }

                    wasDriftingLastFrame = isDrifting;

                    // Progress after 2 successful drifts
                    if (successfulDrifts >= 2)
                    {
                        finishedDriftTutoral = true;
                        StartCoroutine(HideDriftTutorial());
                    }
                }
            }
        }

        // Power-up tutorial - requires drifting twice to see the bar fill
        if(finishedDriftTutoral && PowerUpTutorial.activeInHierarchy && !finishedPowerUpTutorial)
        {
            // Check if player is actually drifting
            GameObject player = GameObject.Find("Player 0");
            if (player != null)
            {
                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null)
                {
                    Rigidbody rb = player.GetComponent<Rigidbody>();
                    float steer = 0f;
                    if (Input.GetKey(KeyCode.D)) steer = 1f;
                    else if (Input.GetKey(KeyCode.A)) steer = -1f;

                    bool attemptDrift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    Vector3 forward = player.transform.forward;
                    float forwardVel = Vector3.Dot(rb.linearVelocity, forward);
                    bool isSteering = Mathf.Abs(steer) > 0.1f;
                    bool hasSpeed = Mathf.Abs(forwardVel) > BotPlayer.minDriftSpeed;
                    bool isDrifting = attemptDrift && isSteering && hasSpeed;

                    // Count a successful drift when player starts drifting
                    if (isDrifting && !wasDriftingLastFramePowerUp)
                    {
                        powerUpTutorialDrifts++;
                    }

                    wasDriftingLastFramePowerUp = isDrifting;

                    // Progress after 2 successful drifts
                    if (powerUpTutorialDrifts >= 2)
                    {
                        finishedPowerUpTutorial = true;
                        StartCoroutine(HidePowerUpTutorial());
                    }
                }
            }
        }

        if(finishedPowerUpTutorial && PowerUpTutorial2.activeInHierarchy && (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3)))
        {
            StartCoroutine(HidePowerUpTutorial2());
        }

        if(finishedPowerUpTutorial && RespawnTutorial.activeInHierarchy && !finishedRespawnTutorial && Input.GetKey(KeyCode.R))
        {
            finishedRespawnTutorial = true;

            // Re-enable crashing and respawning
            GameObject player = GameObject.Find("Player 0");
            if (player != null)
            {
                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.isTutorial = false;
                }
            }

            StartCoroutine(HideRespawnTutorial());
        }
    }

    IEnumerator HideWASDTutorial()
    {
        WASDTutorial.SetActive(false);        // hide the tutorial immediately
        yield return new WaitForSeconds(2f); // brief pause before next tutorial

        StartCoroutine(ShowDriftTutorial());
    }

    IEnumerator ShowDriftTutorial()
    {
        DriftTutorial.SetActive(true);         // show drift tutorial immediately
        yield return null;
    }

    IEnumerator HideDriftTutorial()
    {
        yield return new WaitForSeconds(2f);
        DriftTutorial.SetActive(false);

        StartCoroutine(ShowPowerUpTutorial());
    }

    IEnumerator ShowPowerUpTutorial()
    {
        yield return new WaitForSeconds(2f);
        PowerUpTutorial.SetActive(true);
        // waits for player to drift twice
    }

    IEnumerator HidePowerUpTutorial()
    {
        yield return new WaitForSeconds(2f);
        PowerUpTutorial.SetActive(false);
        PowerUpTutorial2.SetActive(true);
    }

    IEnumerator HidePowerUpTutorial2()
    {
        PowerUpTutorial2.SetActive(false);
        yield return new WaitForSeconds(2f);

        RespawnTutorial.SetActive(true);
    }

    IEnumerator HideRespawnTutorial()
    {
        RespawnTutorial.SetActive(false);
        yield return new WaitForSeconds(2f);

        EndTutorial.SetActive(true);
    }
}
