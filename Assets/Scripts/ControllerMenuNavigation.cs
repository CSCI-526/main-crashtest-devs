using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Simple controller menu navigation support.
/// Add this to any scene with UI buttons to enable controller navigation.
/// Works alongside existing mouse/keyboard controls without regression.
/// </summary>
public class ControllerMenuNavigation : MonoBehaviour
{
    [Header("Optional: Set first selected button")]
    public Button defaultSelectedButton;
    
    [Header("Visual Feedback")]
    [Tooltip("Scale multiplier for selected button (e.g., 1.1 = 10% larger)")]
    public float selectedScale = 1.15f;
    
    private EventSystem eventSystem;
    private bool wasControllerUsedLastFrame = false;
    private GameObject lastSelectedObject = null;
    private Vector3 originalScale = Vector3.one;

    void Start()
    {
        InitializeNavigation();
    }
    
    void OnEnable()
    {
        // Re-initialize when component is enabled (for panels that get activated)
        InitializeNavigation();
    }
    
    private void InitializeNavigation()
    {
        eventSystem = EventSystem.current;
        
        // If no EventSystem exists, create one
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
        
        // Set default selection if controller is connected
        if (InputManager.Instance != null && InputManager.Instance.IsPlayer1ControllerConnected)
        {
            // Delay selection slightly to ensure buttons are ready
            Invoke(nameof(SelectDefaultButton), 0.1f);
        }
    }

    void Update()
    {
        if (InputManager.Instance == null || eventSystem == null) return;

        // Check if controller is being used
        bool controllerActive = InputManager.Instance.IsPlayer1ControllerConnected;
        Vector2 navInput = InputManager.Instance.GetUINavigationInput();
        bool submitPressed = InputManager.Instance.GetUISubmitPressed();
        
        // If controller just became active and nothing is selected, select default button
        if (controllerActive && !wasControllerUsedLastFrame && eventSystem.currentSelectedGameObject == null)
        {
            SelectDefaultButton();
        }
        
        // If controller input detected and nothing selected, select default
        if (controllerActive && (Mathf.Abs(navInput.x) > 0.1f || Mathf.Abs(navInput.y) > 0.1f || submitPressed))
        {
            if (eventSystem.currentSelectedGameObject == null)
            {
                SelectDefaultButton();
            }
        }
        
        // If selected object became inactive or null, find a new one
        if (controllerActive && eventSystem.currentSelectedGameObject != null)
        {
            if (!eventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                SelectDefaultButton();
            }
        }
        
        // Update visual feedback for selected button
        UpdateVisualFeedback();
        
        // If submit pressed on a button, invoke it
        if (submitPressed && eventSystem.currentSelectedGameObject != null)
        {
            Button selectedButton = eventSystem.currentSelectedGameObject.GetComponent<Button>();
            if (selectedButton != null && selectedButton.interactable)
            {
                selectedButton.onClick.Invoke();
            }
        }
        
        wasControllerUsedLastFrame = controllerActive;
    }

    private void UpdateVisualFeedback()
    {
        GameObject currentSelected = eventSystem.currentSelectedGameObject;
        
        // If selection changed, restore previous and apply to new
        if (currentSelected != lastSelectedObject)
        {
            // Restore previous button to normal
            if (lastSelectedObject != null)
            {
                RestoreButtonVisuals(lastSelectedObject);
            }
            
            // Apply highlight to newly selected button
            if (currentSelected != null)
            {
                ApplyButtonHighlight(currentSelected);
            }
            
            lastSelectedObject = currentSelected;
        }
    }

    private void ApplyButtonHighlight(GameObject buttonObject)
    {
        if (buttonObject == null) return;
        
        // Store original scale
        originalScale = buttonObject.transform.localScale;
        
        // Apply scale effect only (clean and simple)
        buttonObject.transform.localScale = originalScale * selectedScale;
    }

    private void RestoreButtonVisuals(GameObject buttonObject)
    {
        if (buttonObject == null) return;
        
        // Restore scale to original
        buttonObject.transform.localScale = originalScale;
    }

    private void OnDisable()
    {
        // Restore visuals when disabled
        if (lastSelectedObject != null)
        {
            RestoreButtonVisuals(lastSelectedObject);
        }
    }

    private void SelectDefaultButton()
    {
        if (defaultSelectedButton != null && defaultSelectedButton.interactable)
        {
            eventSystem.SetSelectedGameObject(defaultSelectedButton.gameObject);
        }
        else
        {
            // Find first active button in scene
            Button firstButton = FindFirstActiveButton();
            if (firstButton != null)
            {
                eventSystem.SetSelectedGameObject(firstButton.gameObject);
            }
        }
    }

    private Button FindFirstActiveButton()
    {
        // Find all buttons, prioritizing active ones
        Button[] allButtons = FindObjectsOfType<Button>(false); // Only find active buttons
        
        // Sort by hierarchy order (top to bottom, left to right)
        System.Array.Sort(allButtons, (a, b) => 
        {
            // Compare by Y position (higher Y = earlier in list)
            float yDiff = b.transform.position.y - a.transform.position.y;
            if (Mathf.Abs(yDiff) > 0.1f)
                return yDiff > 0 ? 1 : -1;
            
            // If same Y, compare by X position (lower X = earlier)
            return a.transform.position.x.CompareTo(b.transform.position.x);
        });
        
        foreach (Button btn in allButtons)
        {
            if (btn.gameObject.activeInHierarchy && btn.interactable)
            {
                return btn;
            }
        }
        return null;
    }
    
    // Public method to force re-selection (useful when UI changes)
    public void RefreshSelection()
    {
        if (InputManager.Instance != null && InputManager.Instance.IsPlayer1ControllerConnected)
        {
            SelectDefaultButton();
        }
    }
}
