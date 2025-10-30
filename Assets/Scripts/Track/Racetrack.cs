using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Racetrack : MonoBehaviour
{
    public TMP_Text countdownText;  // Drag your CountdownText UI element here
    public GameObject canvas;
    public bool lightsOutAndAwayWeGOOOOO = false;
    private float startTimer = 1.0f;  // Time between each countdown
    private int countdownStage = 0;  // 0=Ready, 1=3, 2=2, 3=1, 4=GO
    private readonly List<CheckPointCheck> players = new();
    private List<BezierCurve> curves = new();
    private float raceStartTime = -1f;
    private int finishedPlayers = 0;
    private int realFinishedPlayers = 0;
    private bool isSinglePlayer = true;
    private string playerTimeDisplay = "";
    private ScoreboardUIManager scoreboard;

    private class CheckPointCheck
    {
        public int playerID;
        public GameObject player;
        public float playerTimer;
        public int currentSection = 0;
        public int currentSubSection = 0;
        public GameObject checkpoint;
        public bool bot;
        public float finishTime = -1f;
        public bool finished = false;

        public CheckPointCheck(int playerID, GameObject player, GameObject checkpoint, bool bot = true)
        {
            this.playerTimer = 15f;
            this.currentSection = 0;
            this.player = player;
            this.playerID = playerID;
            this.checkpoint = checkpoint;
            this.bot = bot;
        }
    }

    private void Start()
    {
        scoreboard = FindFirstObjectByType<ScoreboardUIManager>();
        int start = 1;

        players.Add(new CheckPointCheck(0, GameObject.Find("Player 0"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoints/cp 3"), false));
        if (SceneManager.GetActiveScene().name == "MultiPlayer")
        {
            players.Add(new CheckPointCheck(1, GameObject.Find("Player 1"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoints/cp 3"), false));
            start = 2;
            isSinglePlayer = false;
        }

        for (int i = start; ; i++)
        {
            GameObject bot = GameObject.Find($"Bot {i}");
            if (bot == null) break;

            GameObject checkpoint = GameObject.Find("RaceTrack/Start Straight 0/Checkpoints/cp 3");
            players.Add(new CheckPointCheck(0, bot, checkpoint));

            GameObject newMarker = Instantiate(canvas.transform.Find("progressBar").transform.Find("bm").gameObject, canvas.transform.Find("progressBar").transform);
            newMarker.SetActive(true);
            newMarker.name = $"bm{i}";
        }
        canvas.transform.Find("progressBar").transform.GetChild(0).SetAsLastSibling();
        if (start == 2) canvas.transform.Find("progressBar").transform.GetChild(0).SetAsLastSibling();
    }

    void FixedUpdate()
    {
        if (countdownStage < 5)
        {
            startTimer -= Time.deltaTime;

            if (startTimer <= 0f)
            {
                ShowCountdown();
                countdownStage++;
                startTimer = 1.0f;

                // Add slight random delay before "GO!" for excitement
                if (countdownStage == 4) startTimer += UnityEngine.Random.Range(-0.1f, 0.3f);
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            players[i].playerTimer -= Time.deltaTime;
            if (players[i].playerTimer <= 0f) RespawnPlayer(i);
        }

        UpdateUI();
    }

    private void ShowCountdown()
    {
        // Reset scale for new text
        if (countdownText != null)
            countdownText.transform.localScale = Vector3.one;

        switch (countdownStage)
        {
            case 0: // READY
                countdownText.text = "READY";
                countdownText.color = Color.white;
                countdownText.fontSize = 120;
                StartCoroutine(PulseText(0.8f, 1.2f, 0.5f)); // pulse between 0.8 and 1.2 scale
                break;

            case 1: // 3
                countdownText.text = "3";
                countdownText.color = Color.red;
                countdownText.fontSize = 180;
                StartCoroutine(PulseText(0.5f, 1.3f, 0.4f)); // bigger pulse
                break;

            case 2: // 2
                countdownText.text = "2";
                countdownText.color = Color.yellow;
                countdownText.fontSize = 180;
                StartCoroutine(PulseText(0.5f, 1.3f, 0.4f));
                break;

            case 3: // 1
                countdownText.text = "1";
                countdownText.color = Color.yellow;
                countdownText.fontSize = 180;
                StartCoroutine(PulseText(0.5f, 1.3f, 0.4f));
                break;

            case 4: // GO!
                countdownText.text = "GO!";
                countdownText.color = Color.green;
                countdownText.fontSize = 200;
                StartCoroutine(PulseText(0.8f, 1.5f, 0.3f)); // explosive pulse for GO!

                // Start race AFTER the GO! text disappears (1.2 seconds)
                Invoke(nameof(StartRace), 1.2f);

                // Hide countdown text after 1.2 seconds
                Invoke(nameof(HideCountdown), 1.2f);
                break;
        }
    }

    private System.Collections.IEnumerator PulseText(float minScale, float maxScale, float speed)
    {
        float elapsed = 0f;

        while (countdownText != null && countdownText.gameObject.activeInHierarchy)
        {
            elapsed += Time.deltaTime * speed;

            // Ping-pong between min and max scale
            float scale = Mathf.Lerp(minScale, maxScale, Mathf.PingPong(elapsed, 1f));
            countdownText.transform.localScale = Vector3.one * scale;

            yield return null;
        }
    }

    private void StartRace()
    {
        lightsOutAndAwayWeGOOOOO = true;
        finishedPlayers = 0;

        if (raceStartTime < 0f)
            raceStartTime = Time.time;
    }

    private void HideCountdown()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        CheckPointTrigger.OnAnyPlaneTrigger += HandlePlaneTrigger;
    }

    private void OnDisable()
    {
        CheckPointTrigger.OnAnyPlaneTrigger -= HandlePlaneTrigger;
    }

    private void HandlePlaneTrigger(Transform section, string obj, int cpNum)
    {
        string[] parts1 = obj.Split();
        if (parts1.Length < 2 || obj[0] == 'T' || obj[0] == 'C') return;
        int playerID = int.Parse(parts1[1]);

        string[] parts2 = section.name.Split();
        int sectionID = int.Parse(parts2[2]);

        if (cpNum == 3) UpdateSection(playerID, sectionID, section.Find("Checkpoints/cp 3").gameObject);
        UpdateSubCheckpoints(playerID, sectionID, cpNum);
    }

    private void UpdateSubCheckpoints(int playerID, int sectionID, int cpNum)
    {
        // sub checkpoints
        if (players[playerID].currentSection == sectionID)
        {
            if (players[playerID].currentSubSection + 1 == cpNum)
            {
                players[playerID].currentSubSection += 1;
                players[playerID].playerTimer += 3;
            }
        }
        else if (players[playerID].currentSection + 1 == sectionID && cpNum == 3)
        {
            players[playerID].currentSubSection = 0;
            players[playerID].playerTimer += 3;
        }
    }

    private void UpdateUI()
    {
        string compassGO = "playerStats/leftSide/compass";
        int numPlayers = 1;
        if (!isSinglePlayer)
        {
            numPlayers = 2;
            compassGO = "compass";
        }

        for (int j = 0; j < numPlayers; j++)
        {
            int playerSection = players[j].currentSection;

            // ranking
            if (playerSection == 0) continue;
            int rank = 1;
            Vector3 playerPos = players[j].player.transform.position;

            // Approximate where the player is along the curve
            if (playerSection + 1 >= curves.Count) continue;
            float playerT = curves[playerSection + 1].GetClosestTOnCurve(playerPos);

            for (int i = numPlayers; i < players.Count; i++)
            {
                int botSection = players[i].currentSection;

                if (botSection > playerSection) rank++;
                else if (botSection == playerSection)
                {
                    Vector3 botPos = players[i].player.transform.position;
                    float botT = curves[botSection + 1].GetClosestTOnCurve(botPos);
                    if (botT > playerT) rank++;
                }
            }

            if (numPlayers == 2)
            {
                int extra = 1;
                if (j == 1) extra = 0;

                int botSection = players[extra].currentSection;

                if (botSection > playerSection) rank++;
                else if (botSection == playerSection)
                {
                    Vector3 botPos = players[extra].player.transform.position;
                    float botT = curves[botSection + 1].GetClosestTOnCurve(botPos);
                    if (botT > playerT) rank++;
                }
            }

            string rankString = $"{rank}";
            Color rankColor = new(1, 1, 1);

            switch (rank)
            {
                case 1:
                    rankString += "st";
                    rankColor = new(1, .75f, 0);
                    break;
                case 2:
                    rankString += "nd";
                    rankColor = new(.69f, .69f, .69f);
                    break;
                case 3:
                    rankString += "rd";
                    rankColor = new(.55f, .32f, .14f);
                    break;
                default:
                    rankString += "th";
                    break;
            }

            canvas.transform.Find($"ranking{j+1}").GetComponent<TMP_Text>().text = rankString;
            canvas.transform.Find($"ranking{j+1}").GetComponent<TMP_Text>().color = rankColor;

            // compass
            List<Vector3> nextCheckpoints = new();
            playerSection += 2;
            int playerSubSection = players[j].currentSubSection;

            for (int i = 0; i < 3; i++)
            {
                playerSubSection++;
                if (playerSubSection > 3)
                {
                    playerSubSection = 1;
                    playerSection++;
                }
                if (playerSection >= curves.Count) continue;
                nextCheckpoints.Add(curves[playerSection].GetPoint(playerSubSection / 3f));
            }

            Vector3 playerForward = new Vector3(players[j].player.transform.forward.x, 0f, players[j].player.transform.forward.z).normalized;
            Vector3 playerPos2D = new(players[j].player.transform.position.x, 0f, players[j].player.transform.position.z);

            float totalAngle = 0f;

            foreach (Vector3 checkpoint in nextCheckpoints)
            {
                Vector3 targetPos = new(checkpoint.x, 0f, checkpoint.z);
                Vector3 toCheckpoint = (targetPos - playerPos2D).normalized;

                float angle = Vector3.SignedAngle(playerForward, toCheckpoint, Vector3.up);

                totalAngle += angle;
            }
            float averageAngle = totalAngle / nextCheckpoints.Count;

            RectTransform compass = canvas.transform.Find($"{compassGO}{j + 1}/Image").GetComponent<RectTransform>();
            compass.localEulerAngles = new Vector3(0f, 0f, -averageAngle);
        }
    }

    private void UpdateSection(int playerID, int sectionID, GameObject checkpoint)
    {
        if (players[playerID].currentSection + 1 == sectionID)
        {
            players[playerID].currentSection++;
            players[playerID].playerTimer = 5f;
            players[playerID].checkpoint = checkpoint;

            if (sectionID >= curves.Count - 1)
            {
                if (!players[playerID].finished)
                {
                    ++finishedPlayers;
                    players[playerID].finished = true;
                    players[playerID].finishTime = Time.time - raceStartTime;

                    if (!players[playerID].bot)
                    {
                        playerTimeDisplay += $"{finishedPlayers}. Player {playerID} : {FormatTime(players[playerID].finishTime)}\n";
                        Debug.Log("Player finished");
                        ++realFinishedPlayers;

                        scoreboard.ShowPlayerFinishScreen(players[playerID].playerID);
                    }
                    else
                    {
                        playerTimeDisplay += $"{finishedPlayers}. Bot {playerID} : {FormatTime(players[playerID].finishTime)}\n";
                    }
                }


                //Debug.Log($"{finishedPlayers} Player finished");
                if ((isSinglePlayer && realFinishedPlayers == 1) || (!isSinglePlayer && realFinishedPlayers == 2))
                {
                    scoreboard.Show(playerTimeDisplay);
                }
            }

            if (players.Count > 1) UpdateHeadLights();

            UpdateProgressBar(playerID);

            if (isSinglePlayer) UpdateBots(); // how should I do this for multiplayer

            DynamicObstacles(sectionID);
        }
    }

    private void UpdateBots()
    {
        float enginePowerMultiplyer;
        for (int i = 1; i < players.Count; i++)
        {
            int playerDiff = players[0].currentSection - players[i].currentSection;

            if (playerDiff < -20 || playerDiff > 20) continue;
            enginePowerMultiplyer = 1 + playerDiff / 20f;

            players[i].player.GetComponent<Bot>().ChangeMotorPower(2000 * enginePowerMultiplyer);
        }
    }

    private static string FormatTime(float seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
    }

    private void UpdateHeadLights()
    {
        int leader = -1;
        foreach (CheckPointCheck player in players) if (player.currentSection > leader) leader = player.currentSection;

        for (int i = 0; i < players.Count; i++)
        {
            int playerDiff = leader - players[i].currentSection;
            SetPlayerLights(players[i].player, BotPlayer.intensity * (1 + 0.1f * playerDiff), BotPlayer.range * (1 + 0.1f * playerDiff));
        }
    }

    private void SetPlayerLights(GameObject player, float intensity, float range)
    {
        for (int j = 0; j < 2; j++)
        {
            Light light = player.transform.Find($"lights/front/light {j}").GetComponent<Light>();
            light.intensity = intensity;
            light.range = range;
        }
    }

    private void UpdateProgressBar(int playerID)
    {
        GameObject marker;
        if (players[playerID].bot) marker = canvas.transform.Find("progressBar").transform.Find($"bm{playerID}").gameObject;
        else marker = canvas.transform.Find("progressBar").transform.Find($"pm{playerID}").gameObject;

        marker.GetComponent<RectTransform>().pivot = new Vector2(players[playerID].currentSection / (curves.Count * 1.0f), 0);
        marker.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }

    private void RespawnPlayer(int playerID)
    {
        players[playerID].playerTimer = 5f;
        RectTransform rt = players[playerID].player.GetComponent<RectTransform>();

        // position
        int respawnIndex = 0;
        while (true)
        {
            if (!curves[players[playerID].currentSection].respawnSpots[respawnIndex])
            {
                curves[players[playerID].currentSection].respawnSpots[respawnIndex] = true;
                curves[players[playerID].currentSection].respawnTimers[respawnIndex] = 2f;
                break;
            }
            else respawnIndex += 1;

            if (respawnIndex >= 12) { Debug.Log("you broke respawning"); break; }
        }

        Vector3 pos = curves[players[playerID].currentSection].GetOffsetPoint(1 - respawnIndex / 10f, respawnIndex % 2 == 0);
        pos.y += 10f;
        rt.position = pos;

        // rotation
        Vector3 direction = curves[players[playerID].currentSection].GetTangent(1 - respawnIndex / 10f).normalized;
        rt.rotation = Quaternion.LookRotation(direction, Vector3.up);

        // rest
        Rigidbody rb = players[playerID].player.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        bool hasCrashed;

        if (players[playerID].bot)
        {
            Bot botScript = players[playerID].player.GetComponent<Bot>();
            string[] parts = players[playerID].checkpoint.transform.parent.parent.name.Split();
            botScript.ChangeTarget(int.Parse(parts[2]) * 2);
            hasCrashed = botScript.hasCrashed;
            botScript.hasCrashed = false;
        }
        else
        {
            SimpleCarController playerScript = players[playerID].player.GetComponent<SimpleCarController>();
            hasCrashed = playerScript.hasCrashed;
            playerScript.hasCrashed = false;
        }

        if (hasCrashed)
        {
            players[playerID].player.GetComponent<MeshRenderer>().enabled = true;
            for (int i = 0; i < 2; i++) players[playerID].player.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public BezierCurve GetCurve(int index) { return curves[index]; }

    public int GetCurveCount() { return curves.Count; }

    public void AddTrackCurves()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++) curves.Add(gameObject.transform.GetChild(i).GetComponent<RoadMesh>().curve);
    }

    public void DynamicObstacles(int sectionID)
    {
        if (UnityEngine.Random.value > BotPlayer.DynamicObstacles) return;

        int section = UnityEngine.Random.Range(sectionID + 3, sectionID + 6);
        if (section >= curves.Count) return;

        Transform sectionTrees = transform.GetChild(section).Find("Obstacles/Trees");
        if (sectionTrees == null || sectionTrees.childCount == 0) return;

        Transform tree = sectionTrees.GetChild(UnityEngine.Random.Range(0, sectionTrees.childCount));
        if (!tree.TryGetComponent<Rigidbody>(out var rb))
        {
            rb = tree.gameObject.AddComponent<Rigidbody>();
            rb.mass = 250f;
        }

        Vector3 curveMid = curves[section].GetPoint(0.5f);
        Vector3 dirToRoad = (curveMid - tree.position).normalized;

        Quaternion tilt = Quaternion.FromToRotation(Vector3.up, Vector3.up + dirToRoad * 0.1f);

        tree.rotation = tilt * tree.rotation;
        tree.Rotate(Vector3.up, UnityEngine.Random.Range(-10f, 10f), Space.Self);
    }

}

