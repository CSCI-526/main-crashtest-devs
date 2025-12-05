using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerKeyUIManager : MonoBehaviour
{
    public Button w;
    public Button a;
    public Button s;
    public Button d;
    public Button WASDShift;

    public Button up;
    public Button down;
    public Button left;
    public Button right;
    public Button ArrowShift;

    // Optional: Text components to show controller button names
    public TMP_Text player1ControllerPromptText;
    public TMP_Text player2ControllerPromptText;

    private Color WASDColor = new Color(0.9215686f, 0.2078431f, 0.2039216f);
    private Color WASDpressedColor = new Color(0.5754717f, 0.2272161f, 0.2253026f);

    private Color ArrowColor = new Color(0.9607843f, 0.8705882f, 0.007843138f);
    private Color ArrowpressedColor = new Color(0.5849056f, 0.548085f, 0.1958882f);

    private bool lastP1ControllerState = false;
    private bool lastP2ControllerState = false;

    void Start()
    {
        UpdateControllerPrompts();
    }

    void Update()
    {
        // Check if controller connection state changed
        if (InputManager.Instance != null)
        {
            bool currentP1State = InputManager.Instance.IsPlayer1ControllerConnected;
            bool currentP2State = InputManager.Instance.IsPlayer2ControllerConnected;
            
            if (currentP1State != lastP1ControllerState || currentP2State != lastP2ControllerState)
            {
                lastP1ControllerState = currentP1State;
                lastP2ControllerState = currentP2State;
                UpdateControllerPrompts();
            }
        }

        // Handle keyboard/controller input visualization for Player 1
        bool wPressed = false;
        bool aPressed = false;
        bool sPressed = false;
        bool dPressed = false;
        bool shiftPressed = false;

        if (InputManager.Instance != null)
        {
            // Check controller/keyboard input via InputManager
            wPressed = InputManager.Instance.P1Accelerate > 0.1f;
            sPressed = InputManager.Instance.P1Brake > 0.1f;
            aPressed = InputManager.Instance.P1Steer < -0.1f;
            dPressed = InputManager.Instance.P1Steer > 0.1f;
            shiftPressed = InputManager.Instance.P1Drift;
        }
        
        // Also check keyboard directly
        if (Input.GetKey(KeyCode.W)) wPressed = true;
        if (Input.GetKey(KeyCode.A)) aPressed = true;
        if (Input.GetKey(KeyCode.S)) sPressed = true;
        if (Input.GetKey(KeyCode.D)) dPressed = true;
        if (Input.GetKey(KeyCode.LeftShift)) shiftPressed = true;

        // Update WASD button colors
        if (wPressed)
        {
            if (w != null) w.GetComponent<Image>().color = WASDpressedColor;
        }
        else
        {
            if (w != null) w.GetComponent<Image>().color = WASDColor;
        }

        if (aPressed)
        {
            if (a != null) a.GetComponent<Image>().color = WASDpressedColor;
        }
        else
        {
            if (a != null) a.GetComponent<Image>().color = WASDColor;
        }

        if (sPressed)
        {
            if (s != null) s.GetComponent<Image>().color = WASDpressedColor;
        }
        else
        {
            if (s != null) s.GetComponent<Image>().color = WASDColor;
        }

        if (dPressed)
        {
            if (d != null) d.GetComponent<Image>().color = WASDpressedColor;
        }
        else
        {
            if (d != null) d.GetComponent<Image>().color = WASDColor;
        }

        if (shiftPressed)
        {
            if (WASDShift != null) WASDShift.GetComponent<Image>().color = WASDpressedColor;
        }
        else
        {
            if (WASDShift != null) WASDShift.GetComponent<Image>().color = WASDColor;
        }
        
        // Handle Player 2 arrow key visualization
        bool upPressed = false;
        bool downPressed = false;
        bool leftPressed = false;
        bool rightPressed = false;
        bool rightShiftPressed = false;

        if (InputManager.Instance != null)
        {
            // Check controller/keyboard input via InputManager for Player 2
            upPressed = InputManager.Instance.P2Accelerate > 0.1f;
            downPressed = InputManager.Instance.P2Brake > 0.1f;
            leftPressed = InputManager.Instance.P2Steer < -0.1f;
            rightPressed = InputManager.Instance.P2Steer > 0.1f;
            rightShiftPressed = InputManager.Instance.P2Drift;
        }
        
        // Also check keyboard directly
        if (Input.GetKey(KeyCode.UpArrow)) upPressed = true;
        if (Input.GetKey(KeyCode.DownArrow)) downPressed = true;
        if (Input.GetKey(KeyCode.LeftArrow)) leftPressed = true;
        if (Input.GetKey(KeyCode.RightArrow)) rightPressed = true;
        if (Input.GetKey(KeyCode.RightShift)) rightShiftPressed = true;



        if (upPressed)
        {
            if (up != null) up.GetComponent<Image>().color = ArrowpressedColor;
        }
        else
        {
            if (up != null) up.GetComponent<Image>().color = ArrowColor;
        }

        if (downPressed)
        {
            if (down != null) down.GetComponent<Image>().color = ArrowpressedColor;
        }
        else
        {
            if (down != null) down.GetComponent<Image>().color = ArrowColor;
        }

        if (leftPressed)
        {
            if (left != null) left.GetComponent<Image>().color = ArrowpressedColor;
        }
        else
        {
            if (left != null) left.GetComponent<Image>().color = ArrowColor;
        }

        if (rightPressed)
        {
            if (right != null) right.GetComponent<Image>().color = ArrowpressedColor;
        }
        else
        {
            if (right != null) right.GetComponent<Image>().color = ArrowColor;
        }

        if (rightShiftPressed)
        {
            if (ArrowShift != null) ArrowShift.GetComponent<Image>().color = ArrowpressedColor;
        }
        else
        {
            if (ArrowShift != null) ArrowShift.GetComponent<Image>().color = ArrowColor;
        }
    }

    private void UpdateControllerPrompts()
    {
        if (InputManager.Instance != null)
        {
            // Update Player 1 controller prompt
            if (player1ControllerPromptText != null)
            {
                if (InputManager.Instance.IsPlayer1ControllerConnected)
                {
                    player1ControllerPromptText.text = $"P1 Controller: {InputManager.Instance.Player1ControllerName}\n" +
                        $"Accelerate: RT | Steer: Left Stick | Drift: X/Square";
                }
                else
                {
                    player1ControllerPromptText.text = "P1: Keyboard (WASD)";
                }
            }

            // Update Player 2 controller prompt
            if (player2ControllerPromptText != null)
            {
                if (InputManager.Instance.IsPlayer2ControllerConnected)
                {
                    player2ControllerPromptText.text = $"P2 Controller: {InputManager.Instance.Player2ControllerName}\n" +
                        $"Accelerate: RT | Steer: Left Stick | Drift: X/Square";
                }
                else
                {
                    player2ControllerPromptText.text = "P2: Keyboard (Arrows)";
                }
            }
        }
    }
}
