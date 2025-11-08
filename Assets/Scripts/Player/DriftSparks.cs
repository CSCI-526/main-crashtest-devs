using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DriftSparks : MonoBehaviour
{
    [Header("Spark Settings (3D)")]
    public GameObject player;
    public Transform sparkParent;
    public Transform leftWheelPos;
    public Transform rightWheelPos;
    public float spawnInterval = 0.001f;
    public float minLifetime = 0.05f;
    public float maxLifetime = 0.2f;
    public float upwardForce = 3f;
    public float sidewaysForce = 2f;
    public float backwardForce = 0.5f;

    [Header("Spark Settings (2D UI)")]
    public Transform uiSparkParent;
    public RectTransform leftSparkPos;
    public RectTransform rightSparkPos;
    public float uiUpwardForce = 60f;
    public float uiSidewaysForce = 30f;
    public float uiLifetimeMin = 0.15f;
    public float uiLifetimeMax = 0.35f;

    private bool drifting = false;
    private Coroutine sparkRoutine;

    public void UpdateAnim(bool drift)
    {
        if (drift == drifting) return;

        drifting = drift;
        if (drifting)
            sparkRoutine = StartCoroutine(EmitSparks());
        else if (sparkRoutine != null)
            StopCoroutine(sparkRoutine);
    }

    IEnumerator EmitSparks()
    {
        while (drifting)
        {
            // 3d
            bool left = Random.value < 0.5f;
            Transform wheelPos = left ? leftWheelPos : rightWheelPos;

            Vector3 sparkPos = wheelPos.position;
            float deltaX = left ? Random.Range(-0.05f, 0) : Random.Range(0, 0.05f);
            sparkPos += new Vector3(deltaX, Random.Range(0, 0.05f), Random.Range(-0.05f, 0.025f));

            int index3D = Random.Range(0, sparkParent.childCount);
            Transform chosenSpark3D = sparkParent.GetChild(index3D);
            GameObject spark3D = Instantiate(chosenSpark3D.gameObject, sparkPos, Random.rotation);
            spark3D.SetActive(true);

            if (spark3D.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 dir = (Random.Range(0.8f, 1.2f) * upwardForce * wheelPos.up)
                            + (Random.Range(-1f, 1f) * sidewaysForce * wheelPos.right)
                            - (backwardForce * Random.Range(0.5f, 1f) * wheelPos.forward);
                rb.linearVelocity = player.GetComponent<Rigidbody>().linearVelocity + dir;
                rb.angularVelocity = Random.insideUnitSphere * 5f;
            }

            float lifetime3D = Random.Range(minLifetime, maxLifetime);
            Destroy(spark3D, lifetime3D);

            // 2d
            int index2D = Random.Range(0, uiSparkParent.childCount);
            Transform chosenSpark2D = uiSparkParent.GetChild(index2D);

            sparkPos = left ? leftSparkPos.position : rightSparkPos.position;
            GameObject spark2D = Instantiate(chosenSpark2D.gameObject, sparkPos, Quaternion.identity, uiSparkParent.parent);
            spark2D.SetActive(true);

            if (spark2D.TryGetComponent<Rigidbody2D>(out var rb2d))
            {
                Vector2 dir2D = new(Random.Range(-uiSidewaysForce, uiSidewaysForce),
                                            Random.Range(uiUpwardForce * 0.8f, uiUpwardForce * 1.2f));
                rb2d.linearVelocity = dir2D;
                rb2d.angularVelocity = Random.Range(-180f, 180f);
            }

            float lifetime2D = Random.Range(uiLifetimeMin, uiLifetimeMax);
            Destroy(spark2D, lifetime2D);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
