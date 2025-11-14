using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public GameObject loadingPanel;           // The panel containing the loading UI
    public TMP_Text loadingText;              // Text component for "Generating Track..." message
    public Canvas loadingCanvas;              // The canvas containing the loading screen
    public float displayDuration = 2f;        // How long to show the message
    
    private static LoadingScreenManager instance;

    void Awake()
    {
        // Singleton pattern to persist across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup canvas to always be on top and independent
        if (loadingCanvas != null)
        {
            loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            loadingCanvas.sortingOrder = 1000; // Very high to be on top
            
            // Make canvas persist across scenes and independent
            DontDestroyOnLoad(loadingCanvas.gameObject);
            
            // Ensure canvas is at root level (not parented to anything)
            loadingCanvas.transform.SetParent(null);
        }

        // Hide loading panel initially
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        // Subscribe to scene loaded event to ensure panel is hidden
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Always hide the loading panel when a new scene loads
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    public static void ShowLoadingScreen(string sceneName)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.ShowLoadingCoroutine(sceneName));
        }
    }

    private IEnumerator ShowLoadingCoroutine(string sceneName)
    {
        // Show the loading panel
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Loading messages sequence
        string[] loadingMessages = new string[]
        {
            "Generating Track",
            "Building Curves",
            "Adding Obstacles",
            "Finalizing Track"
        };

        float timePerMessage = (displayDuration - 1f) / loadingMessages.Length;
        
        // Show each message with animated dots
        for (int i = 0; i < loadingMessages.Length; i++)
        {
            string baseMessage = loadingMessages[i];
            Debug.Log(baseMessage + "...");
            
            // Calculate progress percentage
            int progress = (int)((i / (float)loadingMessages.Length) * 100);
            
            // Animate dots for this message
            float messageStartTime = Time.time;
            while (Time.time - messageStartTime < timePerMessage)
            {
                int dotCount = ((int)((Time.time - messageStartTime) * 2)) % 4; // Cycle through 0-3 dots
                string dots = new string('.', dotCount);
                
                if (loadingText != null)
                {
                    loadingText.text = $"{baseMessage}{dots}\n{progress}%";
                }
                
                yield return new WaitForSeconds(0.3f);
            }
        }

        // Show success message
        if (loadingText != null)
        {
            loadingText.text = "Track Generated!\n100%";
        }

        Debug.Log("Track Generated!");

        // Wait a bit to show the success message
        yield return new WaitForSeconds(1f);

        // Load the target scene
        SceneManager.LoadScene(sceneName);

        // Hide the loading panel (will happen after scene loads)
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    // Clean up when destroyed
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

