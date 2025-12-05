using UnityEngine;

public class BirdAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody carRigidbody;

    [Header("Settings")]
    [SerializeField] private float minSpeed = 0.1f;
    [SerializeField] private float maxSpeedForAnimation = 100f;
    [SerializeField] private float minAnimSpeed = 1f;
    [SerializeField] private float maxAnimSpeed = 10f;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (carRigidbody == null)
            carRigidbody = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        float carSpeed = carRigidbody != null ? carRigidbody.linearVelocity.magnitude : 0f;

        if (carSpeed > minSpeed)
        {
            float speedPercent = Mathf.Clamp01(carSpeed / maxSpeedForAnimation);
            float animSpeed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, speedPercent);
            animator.speed = animSpeed;
        }
        else
        {
            animator.speed = 0f;
        }
    }
}
