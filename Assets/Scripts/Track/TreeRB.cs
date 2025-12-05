using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TreeRB : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 startPos;
    private bool initialized = false;

    public float velocityThreshold = 50f;
    public float distanceThreshold = 0.5f;
    private float timer = 0f;
    private bool exploded = false;

    [Header("Burn Settings")]
    public float finalScaleMultiplier = 0.1f;
    public Color burnColor = Color.black;
    public Renderer[] renderers;
    private Vector3 initialScale;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialScale = transform.localScale;

        // Delay capturing start position to allow scene to settle
        Invoke(nameof(CaptureStartPosition), 0.5f);
    }

    private void CaptureStartPosition()
    {
        startPos = transform.position;
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized) return;

        if (rb.useGravity && !transform.name.Contains("ctree"))
        {
            timer += Time.fixedDeltaTime;

            if ((rb.linearVelocity.magnitude > velocityThreshold || rb.angularVelocity.magnitude > velocityThreshold|| timer >= 10f) && !exploded) TriggerExplosion();
        }
        else if (Vector3.Distance(transform.position, startPos) > distanceThreshold) rb.useGravity = true;
    }

    private void TriggerExplosion()
    {
        exploded = true;

        Transform fragments = transform.Find("Fragments");
        GameObject fragmentClone = Instantiate(fragments.gameObject, fragments.position, fragments.rotation);
        fragmentClone.SetActive(true);

        foreach (Transform child in fragmentClone.transform)
        {
            if (!child.TryGetComponent(out Rigidbody rb))
                rb = child.gameObject.GetComponent<Rigidbody>();

            Vector3 randomDir = UnityEngine.Random.onUnitSphere;
            rb.AddForce(randomDir * BotPlayer.explosionForce / 2f + Vector3.up * BotPlayer.upwardModifier / 2f, ForceMode.Impulse);
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * BotPlayer.randomTorque / 2f, ForceMode.Impulse);

            Destroy(child.gameObject, BotPlayer.lifetime + UnityEngine.Random.Range(0f, 1f));
        }
        Destroy(fragmentClone, BotPlayer.lifetime + 1);

        transform.GetComponent<Fire>().SetFireActive(true);

        StartCoroutine(BurnAway());

        Destroy(transform.gameObject, BotPlayer.lifetime);
    }

    private IEnumerator BurnAway()
    {
        float elapsed = 0f;

        Vector3 endScale = initialScale * finalScaleMultiplier;

        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;

        while (elapsed < 3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / BotPlayer.lifetime;

            transform.localScale = Vector3.Lerp(initialScale, endScale, t);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    Color col = Color.Lerp(originalColors[i], burnColor, 2f *t);
                    renderers[i].material.color = col;
                }
            }

            yield return null;
        }
    }
}
