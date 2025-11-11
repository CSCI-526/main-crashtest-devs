using UnityEngine;
using System.Collections;

public class CloudTrail : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public Transform spawnPoint;
    public GameObject dust;
    public GameObject mist;

    [Header("Spawn Settings")]
    public float spawnInterval = 0.02f;
    public float minLifetime = 0.4f;
    public float maxLifetime = 0.8f;

    [Header("Motion Settings")]
    public float forwardVelocityFraction = 0.25f;
    public float upwardRandomness = 0.5f;
    public float sidewaysRandomness = 0.75f;
    private bool isWet = true;

    private Coroutine trailRoutine;
    private bool isActive = false;

    public void SetTrailActive(bool active, bool wet)
    {
        if (active == isActive && wet == isWet) return;
        isActive = active;
        isWet = wet;

        if (isActive)
            trailRoutine = StartCoroutine(EmitTrail());
        else if (trailRoutine != null)
            StopCoroutine(trailRoutine);
    }

    IEnumerator EmitTrail()
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();

        while (isActive)
        {
            Vector3 spawnPos = spawnPoint.position + player.transform.right * Random.Range(-sidewaysRandomness, sidewaysRandomness) + Vector3.up * Random.Range(0f, upwardRandomness);

            GameObject sphere = Instantiate(isWet ? mist : dust, spawnPos, Random.rotation);
            sphere.SetActive(true);

            float upValue = isWet ? Random.Range(1f, 3f) : Random.Range(0.5f, 2f);
            if (sphere.TryGetComponent<Rigidbody>(out var srb))
            {
                Vector3 carVel = rb.linearVelocity;
                Vector3 sphereVel = carVel * forwardVelocityFraction + Vector3.up * upValue + player.transform.right * Random.Range(-2f, 2f);
                srb.linearVelocity = sphereVel;
                srb.angularVelocity = Random.insideUnitSphere * 2f;
            }

            StartCoroutine(GrowAndFade(sphere, Random.Range(minLifetime, 0.001f + minLifetime + player.GetComponent<Rigidbody>().linearVelocity.magnitude / BotPlayer.maxSpeed / 2f)));

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator GrowAndFade(GameObject obj, float lifetime)
    {
        float elapsed = 0f;
        Vector3 startScale = obj.transform.localScale;
        Vector3 endScale = isWet ? Vector3.one * (2.5f * player.GetComponent<Rigidbody>().linearVelocity.magnitude / BotPlayer.maxSpeed) : Vector3.one * (1.5f * player.GetComponent<Rigidbody>().linearVelocity.magnitude / BotPlayer.maxSpeed);

        Renderer rend = obj.GetComponent<Renderer>();
        Color baseColor = rend != null ? rend.material.color : Color.white;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            if (rend != null)
            {
                Color c = baseColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                rend.material.color = c;
            }

            yield return null;
        }

        Destroy(obj);
    }
}
