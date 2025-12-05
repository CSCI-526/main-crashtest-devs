
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
    public GameObject p1ModelButtons; // For car/roadrunner selection
    private int currentDiff = 1;
    private static int currP1Color = 0; // static so it persists between scenes
    private static int currP2Color = 0; // static so it persists between scenes
    private static bool p1UseRoadrunner = false; // static so it persists between scenes
    private static bool p2UseRoadrunner = false; // static so it persists between scenes

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // Show the instructions panel when scene loads
        instructionsPanel.SetActive(false);
    }
    
    void OnEnable()
    {
        // Add controller menu navigation when this UI becomes active
        if (FindObjectOfType<ControllerMenuNavigation>() == null)
        {
            gameObject.AddComponent<ControllerMenuNavigation>();
        }
    }

    void Update()
    {
        // Check for cancel input from both keyboard and controller
        bool cancelPressed = false;
        bool pausePressed = false;
        
        if (InputManager.Instance != null)
        {
            cancelPressed = InputManager.Instance.GetUICancelPressed();
            pausePressed = InputManager.Instance.GetPausePressed();
            
            if (pausePressed)
            {
                InputManager.Instance.ResetPauseInput(); // Prevent multiple triggers
            }
        }
        else
        {
            // Fallback to old input
            cancelPressed = Input.GetKeyDown(KeyCode.Escape);
        }
        
        // Back button (B/Circle) or Start/Options button goes back to main menu
        if (cancelPressed || pausePressed)
        {
            OnBackToMainMenu();
        }
    }

    public void OnBackToMainMenu()
    {
        // Trigger reverse camera transition back to main menu
        if (InstructionsTransition.instance != null)
        {
            InstructionsTransition.instance.StartReverseTransition();
        }
    }

    public void OnContinueButton()
    {
        // Load the sketch loading screen
        if (InstructionsTransition.gameMode)
        {
            TrackGen.Scene2Load = "SinglePlayer";
        }
        else
        {
            TrackGen.Scene2Load = "MultiPlayer";
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

    private Image FindButtonImage(GameObject parent, string buttonName)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == buttonName)
            {
                return child.GetComponent<Image>();
            }
        }
        return null;
    }

    public void ChangeP1Color(int newP1Color)
    {
        if (newP1Color == currP1Color) return;
        if (p1ColorButtons == null) return;

        // Get all button images
        Image redBtn = FindButtonImage(p1ColorButtons, "red");
        Image greenBtn = FindButtonImage(p1ColorButtons, "green");
        Image blueBtn = FindButtonImage(p1ColorButtons, "blue");
        Image yellowBtn = FindButtonImage(p1ColorButtons, "yellow");
        Image pinkBtn = FindButtonImage(p1ColorButtons, "pink");
        Image birdBtn = FindButtonImage(p1ColorButtons, "bird");

        // Dim all color buttons first
        if (redBtn != null) redBtn.color = new Color(0.603f, 0.002f, 0.003f);
        if (greenBtn != null) greenBtn.color = new Color(0, 0.490f, 0.0284f);
        if (blueBtn != null) blueBtn.color = new Color(0, 0.463f, 0.707f);
        if (yellowBtn != null) yellowBtn.color = new Color(0.67f, 0.541f, 0);
        if (pinkBtn != null) pinkBtn.color = new Color(0.613f, 0.304f, 0.598f);
        if (birdBtn != null) birdBtn.color = new Color(0.4f, 0.4f, 0.4f);

        // Highlight the selected option and set roadrunner flag
        p1UseRoadrunner = (newP1Color == 5);

        switch (newP1Color)
        {
            case 0: // red
                if (redBtn != null) redBtn.color = new Color(1f, 0, 0);
                break;
            case 1: // green
                if (greenBtn != null) greenBtn.color = new Color(0f, 0.811f, 0.0482f);
                break;
            case 2: // blue
                if (blueBtn != null) blueBtn.color = new Color(0, 0.653f, 1);
                break;
            case 3: // yellow
                if (yellowBtn != null) yellowBtn.color = new Color(0.981f, 0.793f, 0);
                break;
            case 4: // pink
                if (pinkBtn != null) pinkBtn.color = new Color(1, 0.514f, 0.975f);
                break;
            case 5: // bird (roadrunner)
                if (birdBtn != null) birdBtn.color = Color.white;
                break;
        }

        currP1Color = newP1Color;
    }

    public void ChangeP2Color(int newP2Color)
    {
        if (newP2Color == currP2Color) return;
        if (p2ColorButtons == null) return;

        // Get all button images
        Image orangeBtn = FindButtonImage(p2ColorButtons, "orange");
        Image purpleBtn = FindButtonImage(p2ColorButtons, "purple");
        Image cyanBtn = FindButtonImage(p2ColorButtons, "cyan");
        Image whiteBtn = FindButtonImage(p2ColorButtons, "white");
        Image idkBtn = FindButtonImage(p2ColorButtons, "idk");
        Image birdBtn = FindButtonImage(p2ColorButtons, "bird");

        // Dim all buttons first
        if (orangeBtn != null) orangeBtn.color = new Color(0.651f, 0.406f, 0.187f);
        if (purpleBtn != null) purpleBtn.color = new Color(0.400f, 0.209f, 0.509f);
        if (cyanBtn != null) cyanBtn.color = new Color(0.197f, 0.623f, 0.594f);
        if (whiteBtn != null) whiteBtn.color = new Color(0.6f, 0.6f, 0.6f);
        if (idkBtn != null) idkBtn.color = new Color(0.462f, 0.155f, 0.213f);
        if (birdBtn != null) birdBtn.color = new Color(0.4f, 0.4f, 0.4f);

        // Set roadrunner flag
        p2UseRoadrunner = (newP2Color == 5);

        switch (newP2Color)
        {
            case 0: // orange
                if (orangeBtn != null) orangeBtn.color = new Color(1f, 0.586f, 0.212f);
                break;
            case 1: // purple
                if (purpleBtn != null) purpleBtn.color = new Color(0.746f, 0.297f, 1f);
                break;
            case 2: // cyan
                if (cyanBtn != null) cyanBtn.color = new Color(0.193f, 1, 0.944f);
                break;
            case 3: // white
                if (whiteBtn != null) whiteBtn.color = new Color(1, 1, 1);
                break;
            case 4: // idk (pink/red)
                if (idkBtn != null) idkBtn.color = new Color(0.765f, 0.243f, 0.340f);
                break;
            case 5: // bird (roadrunner)
                if (birdBtn != null) birdBtn.color = Color.white;
                break;
        }

        currP2Color = newP2Color;
    }

    public static int GetP1Color() { return currP1Color; } // static so it works across scenes
    public static int GetP2Color() { return currP2Color; } // static so it works across scenes
    public static bool GetP1UseRoadrunner() { return p1UseRoadrunner; } // static so it works across scenes
    public static bool GetP2UseRoadrunner() { return p2UseRoadrunner; } // static so it works across scenes

    // Call this from UI buttons: 0 = car, 1 = roadrunner
    public void SetP1Model(int model)
    {
        p1UseRoadrunner = (model == 1);

        // Update button visuals if they exist
        if (p1ModelButtons != null)
        {
            Transform carBtn = p1ModelButtons.transform.Find("car");
            Transform birdBtn = p1ModelButtons.transform.Find("bird");

            if (carBtn != null)
                carBtn.GetComponent<Image>().color = p1UseRoadrunner ? new Color(0.4f, 0.4f, 0.4f) : Color.white;
            if (birdBtn != null)
                birdBtn.GetComponent<Image>().color = p1UseRoadrunner ? Color.white : new Color(0.4f, 0.4f, 0.4f);
        }
    }
}
