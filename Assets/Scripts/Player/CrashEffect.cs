using UnityEngine;
using System.Collections;

public class CrashEffect : MonoBehaviour
{
    

    public void TriggerCrash()
    {
        // Hide the main model (the car)
        //GetComponent<MeshRenderer>().enabled = false;

        // Enable the “fragment” object (the hidden cube group)
        Transform fragments = transform.Find("CrashFragments");
        GameObject fragmentClone = Instantiate(fragments.gameObject, fragments.position, fragments.rotation);
        fragmentClone.SetActive(true);

        //Transform speed = transform.Find("speed");
        //if (speed != null) speed.gameObject.SetActive(false);

        transform.Find("lights").gameObject.SetActive(false);

        // Apply random physics to each cube
        foreach (Transform child in fragmentClone.transform)
        {
            if (!child.TryGetComponent<Rigidbody>(out Rigidbody rb))
                rb = child.gameObject.GetComponent<Rigidbody>();

            // Random direction & spin
            Vector3 randomDir = Random.onUnitSphere;
            rb.AddForce(randomDir * BotPlayer.explosionForce + Vector3.up * BotPlayer.upwardModifier, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * BotPlayer.randomTorque, ForceMode.Impulse);

            // Auto-destroy fragment after lifetime
            Destroy(child.gameObject, BotPlayer.lifetime + Random.Range(0f, 1f));
        }
        Destroy(fragmentClone, BotPlayer.lifetime + 1);
    }
}
