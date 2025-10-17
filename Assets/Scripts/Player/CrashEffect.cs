using UnityEngine;

public class CrashEffect : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionForce = 500f;      // how hard to throw the cubes
    public float explosionRadius = 5f;       // how far the cubes can fly
    public float upwardModifier = 1f;        // adds upward lift to the explosion
    public float randomTorque = 300f;        // how much each cube spins
    public float lifetime = 5f;              // how long cubes stay active before disappearing

    private bool hasExploded = false;

    public void TriggerCrash(Vector3 explosionPoint)
    {
        Debug.Log("Crash");
        if (hasExploded) return;
        hasExploded = true;

        // Hide the main model (the car)
        var carRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in carRenderers)
            r.enabled = false;

        // Enable the “fragment” object (the hidden cube group)
        Transform fragments = transform.Find("CrashFragments");
        if (fragments == null)
        {
            Debug.LogWarning("No 'CrashFragments' child found!");
            return;
        }

        fragments.gameObject.SetActive(true);

        // Apply random physics to each cube
        foreach (Transform child in fragments)
        {
            if (!child.TryGetComponent<Rigidbody>(out Rigidbody rb))
                rb = child.gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;

            // Random direction & spin
            Vector3 randomDir = Random.onUnitSphere;
            rb.AddForce(randomDir * explosionForce + Vector3.up * upwardModifier, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);

            // Auto-destroy fragment after lifetime
            Destroy(child.gameObject, lifetime + Random.Range(0f, 1f));
        }

        // Optionally destroy the main object later
        Destroy(gameObject, lifetime + 1f);
    }
}
