using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsTransition : MonoBehaviour
{
    public Transform tutorialView; // assign TutorialCameraPoint here
    public float transitionDuration = 2f;

    public GameObject InstructionsUI; // assign panel for instructions
    public GameObject menuUI;         // assign original menu UI parent here
    public LoadingScreenManager loadingScreenManager; // assign LoadingScreenManager here

    public GameObject P2ColorTxt;
    public GameObject P2Colors;

    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 menuPos;
    private Quaternion menuRot;
    private float elapsed;
    private bool transitioning;
    private bool reverseTransition;
    
    public static bool gameMode;
    public static InstructionsTransition instance;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (transitioning)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            if (reverseTransition)
            {
                // Transition back from instructions view to menu
                transform.position = Vector3.Lerp(startPos, menuPos, t);
                transform.rotation = Quaternion.Slerp(startRot, menuRot, t);

                if (t >= 1f)
                {
                    transitioning = false;
                    reverseTransition = false;
                    if (InstructionsUI != null)
                    {
                        InstructionsUI.SetActive(false);
                    }
                    if (menuUI != null)
                    {
                        menuUI.SetActive(true);
                    }
                }
            }
            else
            {
                // Forward transition from menu to instructions view
                InstructionsUI.SetActive(true);

                if (P2ColorTxt != null && P2Colors != null)
                {
                    if (gameMode)
                    {
                        P2ColorTxt.SetActive(false);
                        P2Colors.SetActive(false);
                    }
                    else
                    {
                        P2ColorTxt.SetActive(true);
                        P2Colors.SetActive(true);
                    }
                }

                transform.position = Vector3.Lerp(startPos, tutorialView.position, t);
                transform.rotation = Quaternion.Slerp(startRot, tutorialView.rotation, t);

                if (t >= 1f)
                {
                    transitioning = false;
                }
            }
        }
    }
    public void StartTutorialTransitionMulti()
    {
        gameMode = false;
        StartTutorialTransition();
    }
      public void StartTutorialTransitionSingle()
    {
        gameMode = true;
        StartTutorialTransition();
    }
    public void StartTutorialTransition()
    {
        // Store the menu position before transitioning
        menuPos = transform.position;
        menuRot = transform.rotation;

        if (menuUI != null)
        {
            menuUI.SetActive(false);      // Hide original menu UI
        }
        //InstructionsUI.SetActive(false);   // Hide instructions UI at start of transition
        startPos = transform.position;
        startRot = transform.rotation;
        elapsed = 0f;
        transitioning = true;
        reverseTransition = false;
    }

    public void StartReverseTransition()
    {
        // Transition back to main menu
        startPos = transform.position;
        startRot = transform.rotation;
        elapsed = 0f;
        transitioning = true;
        reverseTransition = true;
    }

    public void StartTutoral()
    {
        SceneManager.LoadScene("Assets/Scenes/Tutorial.unity");
    }
}
