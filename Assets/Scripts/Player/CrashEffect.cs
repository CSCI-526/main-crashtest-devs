using UnityEngine;
using System.Collections;

public class CrashEffect : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionForce = 100;      // how hard to throw the cubes
    public float explosionRadius = 20f;       // how far the cubes can fly
    public float upwardModifier = 5f;        // adds upward lift to the explosion
    public float randomTorque = 20;        // how much each cube spins
    public float lifetime = 3f;              // how long cubes stay active before disappearing

    public void TriggerCrash()
    {
        // Hide the main model (the car)
        GetComponent<MeshRenderer>().enabled = false;

        // Enable the “fragment” object (the hidden cube group)
        Transform fragments = transform.Find("CrashFragments");
        GameObject fragmentClone = Instantiate(fragments.gameObject, fragments.position, fragments.rotation);
        fragmentClone.SetActive(true);

        Transform speed = transform.Find("speed");
        if (speed != null) speed.gameObject.SetActive(false);

        transform.Find("lights").gameObject.SetActive(false);

        // Apply random physics to each cube
        foreach (Transform child in fragmentClone.transform)
        {
            if (!child.TryGetComponent<Rigidbody>(out Rigidbody rb))
                rb = child.gameObject.GetComponent<Rigidbody>();

            // Random direction & spin
            Vector3 randomDir = Random.onUnitSphere;
            rb.AddForce(randomDir * explosionForce + Vector3.up * upwardModifier, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);

            // Auto-destroy fragment after lifetime
            Destroy(child.gameObject, lifetime + Random.Range(0f, 1f));
        }
        Destroy(fragmentClone, lifetime + 1);
    }
}
