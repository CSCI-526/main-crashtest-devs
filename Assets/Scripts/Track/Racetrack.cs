using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Racetrack : MonoBehaviour
{
    public GameObject startLights;
    public GameObject progressBar;
    public bool lightsOutAndAwayWeGOOOOO = false;
    private float startTimer = 1.5f;
    //public float resetFreezeDuration = 1.5f;
    private int lightCount = 0;
    private readonly List<CheckPointCheck> players = new();
    private List<BezierCurve> curves = new();

    private class CheckPointCheck
    {
        public int playerID;
        public GameObject player;
        public float playerTimer;
        public int currentSection;
        public GameObject checkpoint;
        //public bool isDuringReset;
        //public float resetLockTimer;

        public CheckPointCheck(int playerID, GameObject player, GameObject checkpoint)
        {
            this.playerTimer = 15f;
            this.currentSection = 0;
            this.player = player;
            this.playerID = playerID;
            this.checkpoint = checkpoint;
            //this.isDuringReset = false;
            //this.resetLockTimer = 0f;
        }
    }

    private void Start()
    {
        players.Add(new CheckPointCheck(0, GameObject.Find("Player 0"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoint")));
        players.Add(new CheckPointCheck(0, GameObject.Find("Bot 0"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoint")));
        players.Add(new CheckPointCheck(0, GameObject.Find("Bot 1"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoint")));
        players.Add(new CheckPointCheck(0, GameObject.Find("Bot 2"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoint")));
        players.Add(new CheckPointCheck(0, GameObject.Find("Bot 3"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoint")));
        players.Add(new CheckPointCheck(0, GameObject.Find("Bot 4"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoint")));
        players.Add(new CheckPointCheck(0, GameObject.Find("Bot 5"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoint")));
        players.Add(new CheckPointCheck(0, GameObject.Find("Bot 6"), GameObject.Find("RaceTrack/Start Straight 0/Checkpoint")));
        if (SceneManager.GetActiveScene().name == "MultiPlayer") players.Add(new CheckPointCheck(1, GameObject.Find("Player 1"), GameObject.Find("Track/Start Straight 0/Checkpoint")));
    }

    public BezierCurve GetCurve(int index) { return curves[index]; }

    public int GetCurveCount() { return curves.Count; }

    public void AddTrackCurves()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++) curves.Add(gameObject.transform.GetChild(i).GetComponent<RoadMesh>().curve);
    }

    void FixedUpdate()
    {
        if (lightCount < 6)
        {
            startTimer -= Time.deltaTime;

            if (startTimer <= 0f)
            {
                TurnOnLight();

                lightCount++;

                startTimer = 1.5f;

                if (lightCount == 5) startTimer += Random.Range(-.25f, .5f);
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            players[i].playerTimer -= Time.deltaTime;

            /*
            if (players[i].isDuringReset)
            {
                players[i].resetLockTimer -= Time.deltaTime;
                // Unlock when timer expires
                if (players[i].resetLockTimer <= 0f) players[i].isDuringReset = false;
            }*/

            if (players[i].playerTimer <= 0f)
            {
                //Debug.Log($"Player {players[i].playerID} is out of time. Current Checkpoint {players[i].currentSection}"); // need to move the player to the reset spot
                players[i].playerTimer = 5f;

                RectTransform rt = players[i].player.GetComponent<RectTransform>();
                Vector3 pos = players[i].checkpoint.transform.position;
                pos.y -= 12.5f;
                if (players.Count > 1)
                {
                    if (i == 0) pos.x -= 1;
                    else pos.x += 1;
                    rt.position = pos;
                }
                else rt.position = pos;

                Quaternion baseRot = players[i].checkpoint.transform.parent.rotation;
                Vector3 euler = baseRot.eulerAngles;
                int roty = 0;
                switch (players[i].checkpoint.transform.parent.name[..2])
                {
                    case "90":
                        roty = 90;
                        break;
                    case "60":
                        roty = 60;
                        break;
                    case "45":
                        roty = 45;
                        break;
                    case "30":
                        roty = 30;
                        break;
                }
                if (players[i].checkpoint.transform.parent.name[2] == 'L') roty *= -1;
                euler.y += roty;
                rt.rotation = Quaternion.Euler(euler);

                Rigidbody rb = players[i].player.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                //players[i].isDuringReset = true;
                //players[i].resetLockTimer = resetFreezeDuration;
            }
        }
    }

    private void TurnOnLight()
    {
        if (lightCount == 5) { startLights.SetActive(false); lightsOutAndAwayWeGOOOOO = true; }
        else
        {
            GameObject light = startLights.transform.Find($"l{lightCount + 1}/light").gameObject;
            light.SetActive(true);
        }
    }

    private void OnEnable()
    {
        CheckPointTrigger.OnAnyPlaneTrigger += HandlePlaneTrigger;
    }

    private void OnDisable()
    {
        CheckPointTrigger.OnAnyPlaneTrigger -= HandlePlaneTrigger;
    }

    private void HandlePlaneTrigger(Transform section, string obj)
    {
        string[] parts1 = obj.Split();
        int playerID = int.Parse(parts1[1]);

        string[] parts2 = section.name.Split();
        int sectionID = int.Parse(parts2[2]);

        UpdateSection(playerID, sectionID, section.Find("Checkpoint").gameObject);
    }

    private void UpdateSection(int playerID, int sectionID, GameObject checkpoint)
    {
        if (players[playerID].currentSection + 1 == sectionID)
        {
            players[playerID].currentSection++;
            players[playerID].playerTimer = 5f;
            players[playerID].checkpoint = checkpoint;

            if (sectionID == 91) SceneManager.LoadScene("Assets/Scenes/StartScene.unity");

            if (players.Count > 1) UpdateHeadLights();

            //UpdateProgressBar();
        }
    }

    private void UpdateHeadLights()
    {
        int diff = players[0].currentSection - players[1].currentSection;

        if (diff > 5 || diff * -1 > 5) return;

        if (diff == 0) for (int i = 0; i < 2; i++) SetPlayerLights(players[i].player, 100, 50);
        else
        {
            int leaderIndex = diff > 0 ? 0 : 1;
            int followerIndex = diff > 0 ? 1 : 0;
            int absDiff = Mathf.Abs(diff);

            SetPlayerLights(players[leaderIndex].player, 100 / absDiff, 50 / absDiff);
            SetPlayerLights(players[followerIndex].player, 100 * absDiff, 50 * absDiff);
        }
    }

    private void SetPlayerLights(GameObject player, float intensity, float range)
    {
        for (int j = 0; j < 2; j++)
        {
            Light light = player.transform.Find($"light {j}").GetComponent<Light>();
            light.intensity = intensity;
            light.range = range;
        }
    }

    private void UpdateProgressBar()
    {
        for (int i = 0; i < players.Count; i++)
        {
            GameObject marker = progressBar.transform.Find($"p{i}m").gameObject;

            marker.GetComponent<RectTransform>().pivot = new Vector2(players[i].currentSection / 91f, 0);
            marker.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }
    }

    // Public method to check if a player is during reset
    /*public bool IsPlayerDuringReset(int playerID)
    {
        if (playerID < 0 || playerID >= players.Count) return false;
        return players[playerID].isDuringReset;
    }*/
}

