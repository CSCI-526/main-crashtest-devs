using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct RaceResult
{
    public string playerName;
    public float timeSeconds; // use -1 for DNF
    public RaceResult(string name, float t) { playerName = name; timeSeconds = t; }
}

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
        Time.timeScale = 1f;
        SceneManager.LoadScene(scenePath);
    }

    public void OnPlayAgainButton() => HideAndGoTo("Assets/Scenes/SinglePlayer.unity");
    public void OnMainMenuButton() => HideAndGoTo("Assets/Scenes/StartScene.unity");
}