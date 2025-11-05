
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InstructionsUIManager : MonoBehaviour
{
    public GameObject instructionsPanel; // Assign in Inspector
    public GameObject diffButtons;
    private int currentDiff = 1;

    void Start()
    {
        // Show the instructions panel when scene loads
        instructionsPanel.SetActive(true);
    }

    public void OnContinueButton()
    {
        if (TransitionScript.gameMode)
        {
            SceneManager.LoadScene("Assets/Scenes/SinglePlayer.unity");
        }
        else
        {
            SceneManager.LoadScene("Assets/Scenes/MultiPlayer.unity");
        }
    }
    public void OnTutorialButton()
    {
        SceneManager.LoadScene("Assets/Scenes/Tutorial.unity");
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

                BotPlayer.motorPower = 30;
                BotPlayer.botPowerMulti = .7f;
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
}
