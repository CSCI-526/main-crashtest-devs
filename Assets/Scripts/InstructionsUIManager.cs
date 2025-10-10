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
        SceneManager.LoadScene("Assets/Scenes/SinglePlayer.unity");
    }
}
