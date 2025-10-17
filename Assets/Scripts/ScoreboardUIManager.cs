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

    public void OnPlayAgainButton()
    {
        if (SceneManager.GetActiveScene().name == "MultiPlayer")
        {
            //Debug.Log("play multiplayer again");
            HideAndGoTo("Assets/Scenes/MultiPlayer.unity");
        }
        else
        {
            //Debug.Log("play single again");
            HideAndGoTo("Assets/Scenes/SinglePlayer.unity");
        }
    }
    public void OnMainMenuButton() => HideAndGoTo("Assets/Scenes/StartScene.unity");
}