using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralized input manager that handles both keyboard and controller inputs.
/// Supports PS4, PS5, Switch Pro, Xbox, and other generic gamepads.
/// Now supports 2 controllers for local multiplayer!
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private RacingInputActions inputActions;
    private Gamepad player1Gamepad;  // Controller for Player 1
    private Gamepad player2Gamepad;  // Controller for Player 2
    
    // Input state for Player 1 (WASD / Gamepad)
    public float P1Accelerate { get; private set; }
    public float P1Brake { get; private set; }
    public float P1Steer { get; private set; }
    public bool P1Drift { get; private set; }
    public bool P1HardBrake { get; private set; }
    public bool P1Respawn { get; private set; }
    public bool P1PowerUp1 { get; private set; }
    public bool P1PowerUp2 { get; private set; }
    public bool P1PowerUp3 { get; private set; }
    public bool P1Pause { get; private set; }

    // Input state for Player 2 (Arrow Keys)
    public float P2Accelerate { get; private set; }
    public float P2Brake { get; private set; }
    public float P2Steer { get; private set; }
    public bool P2Drift { get; private set; }
    public bool P2HardBrake { get; private set; }
    public bool P2Respawn { get; private set; }
    public bool P2PowerUp1 { get; private set; }
    public bool P2PowerUp2 { get; private set; }
    public bool P2PowerUp3 { get; private set; }

    // Controller detection
    public bool IsPlayer1ControllerConnected => player1Gamepad != null && player1Gamepad.enabled;
    public bool IsPlayer2ControllerConnected => player2Gamepad != null && player2Gamepad.enabled;
    public bool IsControllerConnected => IsPlayer1ControllerConnected; // Backward compatibility
    public string Player1ControllerName => player1Gamepad?.displayName ?? "None";
    public string Player2ControllerName => player2Gamepad?.displayName ?? "None";
    public string ControllerName => Player1ControllerName; // Backward compatibility

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInputSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInputSystem()
    {
        inputActions = new RacingInputActions();
        
        // Subscribe to gameplay actions for Player 1 ONLY (keyboard + first controller)
        // We'll handle controller inputs directly in Update() to separate P1 and P2
        inputActions.Gameplay.Accelerate.performed += ctx => UpdateAccelerate(ctx.ReadValue<float>());
        inputActions.Gameplay.Accelerate.canceled += ctx => UpdateAccelerate(0f);
        
        inputActions.Gameplay.Brake.performed += ctx => UpdateBrake(ctx.ReadValue<float>());
        inputActions.Gameplay.Brake.canceled += ctx => UpdateBrake(0f);
        
        inputActions.Gameplay.Steer.performed += ctx => UpdateSteer(ctx.ReadValue<float>());
        inputActions.Gameplay.Steer.canceled += ctx => UpdateSteer(0f);
        
        inputActions.Gameplay.Drift.performed += ctx => P1Drift = true;
        inputActions.Gameplay.Drift.canceled += ctx => P1Drift = false;
        
        inputActions.Gameplay.HardBrake.performed += ctx => P1HardBrake = true;
        inputActions.Gameplay.HardBrake.canceled += ctx => P1HardBrake = false;
        
        inputActions.Gameplay.Respawn.performed += ctx => P1Respawn = true;
        inputActions.Gameplay.Respawn.canceled += ctx => P1Respawn = false;
        
        inputActions.Gameplay.PowerUp1.performed += ctx => P1PowerUp1 = true;
        inputActions.Gameplay.PowerUp1.canceled += ctx => P1PowerUp1 = false;
        
        inputActions.Gameplay.PowerUp2.performed += ctx => P1PowerUp2 = true;
        inputActions.Gameplay.PowerUp2.canceled += ctx => P1PowerUp2 = false;
        
        inputActions.Gameplay.PowerUp3.performed += ctx => P1PowerUp3 = true;
        inputActions.Gameplay.PowerUp3.canceled += ctx => P1PowerUp3 = false;
        
        inputActions.Gameplay.Pause.performed += ctx => P1Pause = true;
        inputActions.Gameplay.Pause.canceled += ctx => P1Pause = false;

        // Enable the action maps
        inputActions.Gameplay.Enable();
        inputActions.UI.Enable();

        // Listen for controller connection/disconnection
        InputSystem.onDeviceChange += OnDeviceChange;
        
        // Check for existing controllers
        DetectControllers();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    Debug.Log($"Controller connected: {device.displayName}");
                    DetectControllers();
                    break;
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    Debug.Log($"Controller disconnected: {device.displayName}");
                    DetectControllers();
                    break;
            }
        }
    }

    private void DetectControllers()
    {
        // Get all connected gamepads
        var gamepads = Gamepad.all;
        
        if (gamepads.Count > 0)
        {
            player1Gamepad = gamepads[0];
            Debug.Log($"Player 1 controller: {player1Gamepad.displayName} (Device ID: {player1Gamepad.deviceId})");
            
            // Bind InputActions to ONLY the first controller
            if (inputActions != null)
            {
                inputActions.devices = new InputDevice[] { player1Gamepad, Keyboard.current };
            }
        }
        else
        {
            player1Gamepad = null;
            
            // If no controller, bind to keyboard only
            if (inputActions != null && Keyboard.current != null)
            {
                inputActions.devices = new InputDevice[] { Keyboard.current };
            }
        }
        
        if (gamepads.Count > 1)
        {
            player2Gamepad = gamepads[1];
            Debug.Log($"Player 2 controller: {player2Gamepad.displayName} (Device ID: {player2Gamepad.deviceId})");
        }
        else
        {
            player2Gamepad = null;
        }
    }

    private void UpdateAccelerate(float value)
    {
        P1Accelerate = value;
    }

    private void UpdateBrake(float value)
    {
        P1Brake = value;
    }

    private void UpdateSteer(float value)
    {
        P1Steer = value;
    }

    private void Update()
    {
        bool isMultiplayer = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MultiPlayer";
        
        // Reset P1 inputs first
        bool p1UsingController = false;
        
        // Read Player 1 controller directly (first controller only)
        if (player1Gamepad != null && player1Gamepad.enabled)
        {
            float p1RightTrigger = player1Gamepad.rightTrigger.ReadValue();
            float p1LeftTrigger = player1Gamepad.leftTrigger.ReadValue();
            float p1StickX = player1Gamepad.leftStick.x.ReadValue();
            
            // Check if any controller input is active
            if (p1RightTrigger > 0.1f || p1LeftTrigger > 0.1f || Mathf.Abs(p1StickX) > 0.1f ||
                player1Gamepad.buttonWest.isPressed || player1Gamepad.buttonEast.isPressed ||
                player1Gamepad.buttonNorth.isPressed || player1Gamepad.dpad.left.isPressed ||
                player1Gamepad.dpad.down.isPressed || player1Gamepad.dpad.right.isPressed ||
                player1Gamepad.startButton.isPressed)
            {
                // Use controller input for Player 1
                P1Accelerate = p1RightTrigger;
                P1Brake = p1LeftTrigger;
                P1Steer = p1StickX;
                P1Drift = player1Gamepad.buttonWest.isPressed;
                P1HardBrake = player1Gamepad.buttonEast.isPressed;
                P1Respawn = player1Gamepad.buttonNorth.isPressed;
                P1PowerUp1 = player1Gamepad.dpad.left.isPressed;
                P1PowerUp2 = player1Gamepad.dpad.down.isPressed;
                P1PowerUp3 = player1Gamepad.dpad.right.isPressed;
                if (player1Gamepad.startButton.wasPressedThisFrame) P1Pause = true;
                
                p1UsingController = true;
            }
        }
        
        // If Player 1 not using controller, InputActions handle keyboard
        // In multiplayer, we DON'T want to block P1's WASD input
        // The InputActions correctly read WASD for P1, we just need to ensure it's not zeroed out
        
        // Also check for keyboard fallback on Player 1 inputs
        // This ensures number keys (1,2,3) and R still work even if InputActions don't capture them
        if (Input.GetKey(KeyCode.Alpha1)) P1PowerUp1 = true;
        if (Input.GetKey(KeyCode.Alpha2)) P1PowerUp2 = true;
        if (Input.GetKey(KeyCode.Alpha3)) P1PowerUp3 = true;
        if (Input.GetKey(KeyCode.R)) P1Respawn = true;
        
        // Handle Player 2 inputs - can use second controller OR keyboard
        if (player2Gamepad != null && player2Gamepad.enabled)
        {
            // Player 2 using controller (second controller only)
            float p2RightTrigger = player2Gamepad.rightTrigger.ReadValue();
            float p2LeftTrigger = player2Gamepad.leftTrigger.ReadValue();
            float p2StickX = player2Gamepad.leftStick.x.ReadValue();
            
            P2Accelerate = p2RightTrigger > 0.1f ? p2RightTrigger : 0f;
            P2Brake = p2LeftTrigger;
            P2Steer = Mathf.Abs(p2StickX) > 0.1f ? p2StickX : 0f;
            P2Drift = player2Gamepad.buttonWest.isPressed;
            P2HardBrake = player2Gamepad.buttonEast.isPressed;
            P2Respawn = player2Gamepad.buttonNorth.isPressed;
            P2PowerUp1 = player2Gamepad.dpad.left.isPressed;
            P2PowerUp2 = player2Gamepad.dpad.down.isPressed;
            P2PowerUp3 = player2Gamepad.dpad.right.isPressed;
        }
        else
        {
            // Player 2 using keyboard (Arrow keys) - ONLY in multiplayer
            if (isMultiplayer)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                    P2Accelerate = 1f;
                else if (Input.GetKey(KeyCode.DownArrow))
                    P2Accelerate = -0.75f;
                else
                    P2Accelerate = 0f;

                if (Input.GetKey(KeyCode.DownArrow))
                    P2Brake = 1f;
                else
                    P2Brake = 0f;

                if (Input.GetKey(KeyCode.RightArrow))
                    P2Steer = 1f;
                else if (Input.GetKey(KeyCode.LeftArrow))
                    P2Steer = -1f;
                else
                    P2Steer = 0f;

                P2Drift = Input.GetKey(KeyCode.RightShift);
                P2HardBrake = Input.GetKey(KeyCode.RightCommand);
                P2Respawn = Input.GetKey(KeyCode.Slash);
                P2PowerUp1 = Input.GetKey(KeyCode.Alpha0);
                P2PowerUp2 = Input.GetKey(KeyCode.Alpha9);
                P2PowerUp3 = Input.GetKey(KeyCode.Alpha8);
            }
            else
            {
                // In single player, zero out P2 inputs
                P2Accelerate = 0f;
                P2Brake = 0f;
                P2Steer = 0f;
                P2Drift = false;
                P2HardBrake = false;
                P2Respawn = false;
                P2PowerUp1 = false;
                P2PowerUp2 = false;
                P2PowerUp3 = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Gameplay.Disable();
            inputActions.UI.Disable();
            inputActions.Dispose();
        }
        
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // Helper methods for easy access
    public bool GetPausePressed()
    {
        return P1Pause;
    }

    public void ResetPauseInput()
    {
        P1Pause = false;
    }

    // UI Navigation helpers
    public Vector2 GetUINavigationInput()
    {
        if (inputActions != null)
        {
            return inputActions.UI.Navigate.ReadValue<Vector2>();
        }
        return Vector2.zero;
    }

    public bool GetUISubmitPressed()
    {
        if (inputActions != null)
        {
            return inputActions.UI.Submit.triggered;
        }
        return false;
    }

    public bool GetUICancelPressed()
    {
        if (inputActions != null)
        {
            return inputActions.UI.Cancel.triggered;
        }
        return false;
    }

    // Get button prompts based on current input device
    public string GetAcceleratePrompt()
    {
        return IsControllerConnected ? "RT" : "W/↑";
    }

    public string GetBrakePrompt()
    {
        return IsControllerConnected ? "LT" : "S/↓";
    }

    public string GetSteerPrompt()
    {
        return IsControllerConnected ? "Left Stick" : "A/D or ←/→";
    }

    public string GetDriftPrompt()
    {
        return IsControllerConnected ? "X (PS) / Y (Xbox)" : "Shift";
    }

    public string GetRespawnPrompt()
    {
        return IsControllerConnected ? "Y (PS) / Triangle (Xbox)" : "R";
    }

    public string GetPowerUp1Prompt()
    {
        return IsControllerConnected ? "D-Pad Left" : "1";
    }

    public string GetPowerUp2Prompt()
    {
        return IsControllerConnected ? "D-Pad Down" : "2";
    }

    public string GetPowerUp3Prompt()
    {
        return IsControllerConnected ? "D-Pad Right" : "3";
    }
}

