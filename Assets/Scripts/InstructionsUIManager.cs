
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InstructionsUIManager : MonoBehaviour
{
    public static InstructionsUIManager Instance;
    public GameObject instructionsPanel; // Assign in Inspector
    public GameObject diffButtons;
    public GameObject p1ColorButtons;
    public GameObject p2ColorButtons;
    private int currentDiff = 1;
    private int currP1Color = 0;
    private int currP2Color = 0;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // Show the instructions panel when scene loads
        instructionsPanel.SetActive(false);
    }

    public void OnContinueButton()
    {
        // Load the sketch loading screen
        if (InstructionsTransition.gameMode)
        {
            TrackSketchLoader.targetScene = "SinglePlayer";
        }
        else
        {
            TrackSketchLoader.targetScene = "MultiPlayer";
        }
        SceneManager.LoadScene("LoadingScene");
    }
    public void OnTutorialButton()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void ChangeDiff(int newDiff)
    {
        if (newDiff == currentDiff) return;

        switch (newDiff)
        {
            case 0:
                diffButtons.transform.Find("easy").transform.GetComponent<Image>().color = new Color(0, 1, 0);
                diffButtons.transform.Find("med").transform.GetComponent<Image>().color = new Color(.4f, .4f, 0);
                diffButtons.transform.Find("hard").transform.GetComponent<Image>().color = new Color(.4f, 0, 0);

                BotPlayer.diff = 0;
                BotPlayer.motorPower = 30;
                BotPlayer.botPowerMulti = .8f;
                BotPlayer.maxSpeed = 50;
                BotPlayer.brakeDrag = 1.5f;
                BotPlayer.DynamicObstacles = .05f;
                BotPlayer.playerDeltaSpeed = 75f;
                BotPlayer.botDeltaSpeed = 75f;
                BotPlayer.intensity = 175f;
                BotPlayer.range = 100f;
                break;
            case 1:
                diffButtons.transform.Find("easy").transform.GetComponent<Image>().color = new Color(0, .4f, 0);
                diffButtons.transform.Find("med").transform.GetComponent<Image>().color = new Color(1, 1, 0);
                diffButtons.transform.Find("hard").transform.GetComponent<Image>().color = new Color(.4f, 0, 0);

                BotPlayer.diff = 1;
                BotPlayer.motorPower = 40;
                BotPlayer.botPowerMulti = 1f;
                BotPlayer.maxSpeed = 98;
                BotPlayer.brakeDrag = 1f;
                BotPlayer.DynamicObstacles = .15f;
                BotPlayer.playerDeltaSpeed = 65f;
                BotPlayer.botDeltaSpeed = 50f;
                BotPlayer.intensity = 100;
                BotPlayer.range = 50;
                break;
            case 2:
                diffButtons.transform.Find("easy").transform.GetComponent<Image>().color = new Color(0, .4f, 0);
                diffButtons.transform.Find("med").transform.GetComponent<Image>().color = new Color(.4f, .4f, 0);
                diffButtons.transform.Find("hard").transform.GetComponent<Image>().color = new Color(1, 0, 0);

                BotPlayer.diff = 2;
                BotPlayer.motorPower = 60;
                BotPlayer.botPowerMulti = 1.2f;
                BotPlayer.maxSpeed = 150;
                BotPlayer.brakeDrag = .8f;
                BotPlayer.DynamicObstacles = .25f;
                BotPlayer.playerDeltaSpeed = 50f;
                BotPlayer.botDeltaSpeed = 40f;
                BotPlayer.intensity = 75;
                BotPlayer.range = 40;
                break;
        }

        currentDiff = newDiff;
    }

    public void ChangeP1Color(int newP1Color)
    {
        if (newP1Color == currP1Color) return;

        switch (newP1Color)
        {
            case 0: // red
                p1ColorButtons.transform.Find("red").transform.GetComponent<Image>().color = new Color(1f, 0, 0);
                p1ColorButtons.transform.Find("green").transform.GetComponent<Image>().color = new Color(0, 0.490f, 0.0284f);
                p1ColorButtons.transform.Find("blue").transform.GetComponent<Image>().color = new Color(0, 0.463f, 0.707f);
                p1ColorButtons.transform.Find("yellow").transform.GetComponent<Image>().color = new Color(0.67f, 0.541f, 0);
                p1ColorButtons.transform.Find("pink").transform.GetComponent<Image>().color = new Color(0.613f, 0.304f, 0.598f);

                break;
            case 1: // green
                p1ColorButtons.transform.Find("red").transform.GetComponent<Image>().color = new Color(0.603f, 0.002f, 0.003f);
                p1ColorButtons.transform.Find("green").transform.GetComponent<Image>().color = new Color(0f, 0.811f, 0.0482f);
                p1ColorButtons.transform.Find("blue").transform.GetComponent<Image>().color = new Color(0, 0.463f, 0.707f);
                p1ColorButtons.transform.Find("yellow").transform.GetComponent<Image>().color = new Color(0.67f, 0.541f, 0);
                p1ColorButtons.transform.Find("pink").transform.GetComponent<Image>().color = new Color(0.613f, 0.304f, 0.598f);

                break;
            case 2: // blue
                p1ColorButtons.transform.Find("red").transform.GetComponent<Image>().color = new Color(0.603f, 0.002f, 0.003f);
                p1ColorButtons.transform.Find("green").transform.GetComponent<Image>().color = new Color(0, 0.490f, 0.0284f);
                p1ColorButtons.transform.Find("blue").transform.GetComponent<Image>().color = new Color(0, 0.653f, 1);
                p1ColorButtons.transform.Find("yellow").transform.GetComponent<Image>().color = new Color(0.67f, 0.541f, 0);
                p1ColorButtons.transform.Find("pink").transform.GetComponent<Image>().color = new Color(0.613f, 0.304f, 0.598f);

                break;
            case 3: // yellow
                p1ColorButtons.transform.Find("red").transform.GetComponent<Image>().color = new Color(0.603f, 0.002f, 0.003f);
                p1ColorButtons.transform.Find("green").transform.GetComponent<Image>().color = new Color(0, 0.490f, 0.0284f);
                p1ColorButtons.transform.Find("blue").transform.GetComponent<Image>().color = new Color(0, 0.463f, 0.707f);
                p1ColorButtons.transform.Find("yellow").transform.GetComponent<Image>().color = new Color(0.981f, 0.793f, 0);
                p1ColorButtons.transform.Find("pink").transform.GetComponent<Image>().color = new Color(0.613f, 0.304f, 0.598f);

                break;
            case 4: // pink
                p1ColorButtons.transform.Find("red").transform.GetComponent<Image>().color = new Color(0.603f, 0.002f, 0.003f);
                p1ColorButtons.transform.Find("green").transform.GetComponent<Image>().color = new Color(0, 0.490f, 0.0284f);
                p1ColorButtons.transform.Find("blue").transform.GetComponent<Image>().color = new Color(0, 0.463f, 0.707f);
                p1ColorButtons.transform.Find("yellow").transform.GetComponent<Image>().color = new Color(0.67f, 0.541f, 0);
                p1ColorButtons.transform.Find("pink").transform.GetComponent<Image>().color = new Color(1, 0.514f, 0.975f);

                break;
        }

        currP1Color = newP1Color;
    }

    public void ChangeP2Color(int newP2Color)
    {
        if (newP2Color == currP2Color) return;

        switch (newP2Color)
        {
            case 0: // red
                p2ColorButtons.transform.Find("orange").transform.GetComponent<Image>().color = new Color(1f, 0.586f, 0.212f);
                p2ColorButtons.transform.Find("purple").transform.GetComponent<Image>().color = new Color(0.400f, 0.209f, 0.509f);
                p2ColorButtons.transform.Find("cyan").transform.GetComponent<Image>().color = new Color(0.197f, 0.623f, 0.594f);
                p2ColorButtons.transform.Find("white").transform.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);
                p2ColorButtons.transform.Find("idk").transform.GetComponent<Image>().color = new Color(0.462f, 0.155f, 0.213f);

                break;
            case 1: // green
                p2ColorButtons.transform.Find("orange").transform.GetComponent<Image>().color = new Color(0.651f, 0.406f, 0.187f);
                p2ColorButtons.transform.Find("purple").transform.GetComponent<Image>().color = new Color(0.746f, 0.297f, 1f);
                p2ColorButtons.transform.Find("cyan").transform.GetComponent<Image>().color = new Color(0.197f, 0.623f, 0.594f);
                p2ColorButtons.transform.Find("white").transform.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);
                p2ColorButtons.transform.Find("idk").transform.GetComponent<Image>().color = new Color(0.462f, 0.155f, 0.213f);

                break;
            case 2: // blue
                p2ColorButtons.transform.Find("orange").transform.GetComponent<Image>().color = new Color(0.651f, 0.406f, 0.187f);
                p2ColorButtons.transform.Find("purple").transform.GetComponent<Image>().color = new Color(0.400f, 0.209f, 0.509f);
                p2ColorButtons.transform.Find("cyan").transform.GetComponent<Image>().color = new Color(0.193f, 1, 0.944f);
                p2ColorButtons.transform.Find("white").transform.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);
                p2ColorButtons.transform.Find("idk").transform.GetComponent<Image>().color = new Color(0.462f, 0.155f, 0.213f);

                break;
            case 3: // yellow
                p2ColorButtons.transform.Find("orange").transform.GetComponent<Image>().color = new Color(0.651f, 0.406f, 0.187f);
                p2ColorButtons.transform.Find("purple").transform.GetComponent<Image>().color = new Color(0.400f, 0.209f, 0.509f);
                p2ColorButtons.transform.Find("cyan").transform.GetComponent<Image>().color = new Color(0.197f, 0.623f, 0.594f);
                p2ColorButtons.transform.Find("white").transform.GetComponent<Image>().color = new Color(1, 1, 1);
                p2ColorButtons.transform.Find("idk").transform.GetComponent<Image>().color = new Color(0.462f, 0.155f, 0.213f);

                break;
            case 4: // pink
                p2ColorButtons.transform.Find("orange").transform.GetComponent<Image>().color = new Color(0.651f, 0.406f, 0.187f);
                p2ColorButtons.transform.Find("purple").transform.GetComponent<Image>().color = new Color(0.400f, 0.209f, 0.509f);
                p2ColorButtons.transform.Find("cyan").transform.GetComponent<Image>().color = new Color(0.197f, 0.623f, 0.594f);
                p2ColorButtons.transform.Find("white").transform.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);
                p2ColorButtons.transform.Find("idk").transform.GetComponent<Image>().color = new Color(0.765f, 0.243f, 0.340f);

                break;
        }

        currP2Color = newP2Color;
    }

    public int getP1Color() { return currP1Color;  }
    public int getP2Color() { return currP2Color; }
}
