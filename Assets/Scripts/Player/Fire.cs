using UnityEngine;
using System.Collections;

public class Fire : MonoBehaviour
{
    [Header("References")]
    public Transform car;
    public Transform fireSpawn;
    public GameObject smokeSpherePrefab;
    public GameObject[] fireSpherePrefabs;

    [Header("Spawn Settings")]
    public float spawnInterval = 0.02f;

    [Header("Smoke Lifetime")]
    public float smokeMinLifetime = 0.75f;
    public float smokeMaxLifetime = 1.25f;

    [Header("Fire Lifetime")]
    public float fireMinLifetime = 0.2f;
    public float fireMaxLifetime = 0.5f;

    [Header("Motion Settings – Smoke")]
    public float smokeUpwardMin = 1f;
    public float smokeUpwardMax = 4f;

    [Header("Motion Settings – Fire")]
    public float fireUpwardMin = 0.3f;
    public float fireUpwardMax = 1.2f;

    [Header("Shared Noise")]
    public float sidewaysNoise = 1f;
    public float upwardNoise = 1f;

    private bool active = false;
    private Coroutine fireRoutine;

    public void SetFireActive(bool on)
    {
        if (on == active) return;
        active = on;

        if (active)
            fireRoutine = StartCoroutine(EmitFire());
        else if (fireRoutine != null)
            StopCoroutine(fireRoutine);
    }

    IEnumerator EmitFire()
    {
        while (active)
        {
            Vector3 spawnPos =
                fireSpawn.position +
                Vector3.up * Random.Range(0f, upwardNoise) +
                car.right * Random.Range(-sidewaysNoise, sidewaysNoise) +
                car.forward * Random.Range(-sidewaysNoise, sidewaysNoise);

            GameObject smoke = Instantiate(smokeSpherePrefab, spawnPos, Random.rotation, car);
            smoke.SetActive(true);

            if (smoke.TryGetComponent<Rigidbody>(out var smokeRb))
            {
                float up = Random.Range(smokeUpwardMin, smokeUpwardMax);
                Vector3 vel = Vector3.up * up + car.right * Random.Range(-1f, 1f);

                smokeRb.linearVelocity = vel;
                smokeRb.angularVelocity = Random.insideUnitSphere * 3f;
            }

            float smokeLifetime = Random.Range(smokeMinLifetime, smokeMaxLifetime);
            StartCoroutine(GrowAndFade(smoke, smokeLifetime));

            GameObject fire = Instantiate(fireSpherePrefabs[Random.Range(0, fireSpherePrefabs.Length)], spawnPos, Random.rotation, car);
            fire.SetActive(true);

            if (fire.TryGetComponent<Rigidbody>(out var fireRb))
            {
                float up = Random.Range(fireUpwardMin, fireUpwardMax);
                Vector3 vel = Vector3.up * up + Random.insideUnitSphere * 0.5f;

                fireRb.linearVelocity = vel;
                fireRb.angularVelocity = Random.insideUnitSphere * 5f;
            }

            float fireLifetime = Random.Range(fireMinLifetime, fireMaxLifetime);
            StartCoroutine(ShrinkAndFade(fire, fireLifetime));

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator GrowAndFade(GameObject obj, float lifetime)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        float elapsed = 0f;

        Vector3 startScale = obj.transform.localScale;
        Vector3 endScale = startScale * Random.Range(1.25f, 2f);

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

    IEnumerator ShrinkAndFade(GameObject obj, float lifetime)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        float elapsed = 0f;

        Vector3 startScale = obj.transform.localScale;
        Vector3 endScale = startScale * Random.Range(0.25f, 0.5f);

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
