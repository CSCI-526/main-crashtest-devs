using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsUIManager : MonoBehaviour
{
    public GameObject instructionsPanel; // Assign in Inspector

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
}
