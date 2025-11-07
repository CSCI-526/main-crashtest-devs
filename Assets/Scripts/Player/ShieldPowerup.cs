using UnityEngine;

public class ShieldPowerup : MonoBehaviour
{
    [SerializeField] private float shieldDuration = 15f;
    [SerializeField] public GameObject shieldVisual;

    public bool isActive = false;

    private float timer = 0f;
    private Racetrack racetrack;

    void Start()
    {
        racetrack = GetComponent<SimpleCarController>()?.racetrack;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        else
        {
            Debug.LogError("Shield Visual is not assigned!");
        }
    }

    void Update()
    {
        // Activate shield on E key press
        if (Input.GetKeyDown(KeyCode.E) && !isActive)
        {
            ActivateShield();
        }
    }

    void FixedUpdate()
    {
        if (isActive)
        {
            timer -= Time.fixedDeltaTime;

            // Flash shield visual in last 4 seconds
            if (timer <= 4f && shieldVisual != null)
            {
                float pulseSpeed = 5f;
                bool isVisible = Mathf.Sin(Time.time * pulseSpeed) > 0f;
                shieldVisual.SetActive(isVisible);
            }

            if (timer <= 0f)
            {
                DeactivateShield();
            }
        }
    }

    private void ActivateShield()
    {
        isActive = true;
        timer = shieldDuration;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
    }

    public void DeactivateShield()
    {
        if (!isActive) return;

        isActive = false;
        timer = 0f;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }

    public bool IsShieldActive()
    {
        return isActive;
    }

    public float GetShieldTimeRemaining()
    {
        return timer;
    }

    public float GetShieldDuration()
    {
        return shieldDuration;
    }
}
