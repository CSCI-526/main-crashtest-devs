using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreboardUIManager : MonoBehaviour
{
    public Canvas scoreboardCanvas;    // ScoreboardCanvas
    public TMP_Text playerScores;
    public GameObject playAgainButton; // Buttons/PlayAgainButton
    public GameObject mainMenuButton;  // Buttons/MainMenuButton

    public GameObject P1FinishScreen;
    public GameObject P2FinishScreen;

    private bool shown;

    void Awake()
    {
        if (scoreboardCanvas != null) scoreboardCanvas.gameObject.SetActive(false);
    }

    public void Show(string scores)
    {
        if (shown) return;
        shown = true;

        playerScores.text = scores;

        scoreboardCanvas.gameObject.SetActive(true);
    }

    public void HideAndGoTo(string scenePath)
    {
        // Unpause before leaving scene
        SceneManager.LoadScene(scenePath);
    }

    public void ShowPlayerFinishScreen(int playerID)
    {
        Debug.Log($"Show scoreboard");
        if (P1FinishScreen != null && playerID == 0)
            P1FinishScreen.gameObject.SetActive(true);
        else if (P2FinishScreen != null)
            P2FinishScreen.gameObject.SetActive(true);
    }

    public void OnPlayAgainButton()
    {
        if (SceneManager.GetActiveScene().name == "MultiPlayer")
        {
            //Debug.Log("play multiplayer again");
            HideAndGoTo("Assets/Scenes/MultiPlayer.unity");
        }
        else if(SceneManager.GetActiveScene().name == "SinglePlayer")
        {
            //Debug.Log("play single again");
            HideAndGoTo("Assets/Scenes/SinglePlayer.unity");
        }
        else
        {
            HideAndGoTo("Assets/Scenes/Tutorial.unity");
        }
    }
    public void OnMainMenuButton() => HideAndGoTo("Assets/Scenes/StartScene.unity");
}