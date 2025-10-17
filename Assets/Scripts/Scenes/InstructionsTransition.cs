using UnityEngine;

public class TransitionScript : MonoBehaviour
{
    public Transform tutorialView; // assign TutorialCameraPoint here
    public float transitionDuration = 2f;

    public GameObject InstructionsUI; // assign panel for instructions
    public GameObject menuUI;         // assign original menu UI parent here

    private Vector3 startPos;
    private Quaternion startRot;
    private float elapsed;
    private bool transitioning;
    
    public static bool gameMode;
    void Update()
    {
        if (transitioning)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            transform.position = Vector3.Lerp(startPos, tutorialView.position, t);
            transform.rotation = Quaternion.Slerp(startRot, tutorialView.rotation, t);

            if (t >= 1f)
            {
                transitioning = false;
                // InstructionsUI.SetActive(true);
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
        if (menuUI != null)
        {
            menuUI.SetActive(false);      // Hide original menu UI
        }
        // InstructionsUI.SetActive(false);   // Hide instructions UI at start of transition
        startPos = transform.position;
        startRot = transform.rotation;
        elapsed = 0f;
        transitioning = true;
    }
}
