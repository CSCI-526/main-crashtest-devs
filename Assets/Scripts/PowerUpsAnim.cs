using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PowerUpsAnim : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI mainText; // Assign in inspector (the main text)
    public TextMeshProUGUI subText;  // Optional child text

    [Header("Settings")]
    public float pulseInterval = 1.0f;
    public float pulseDuration = 0.5f;
    public float startScale = 0.8f;
    public float endScale = 1.0f;
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.white;

    private bool isActive;
    private Coroutine pulseRoutine;
    private GameObject flashObj;

    void Awake()
    {
        if (!mainText)
            mainText = GetComponent<TextMeshProUGUI>();

        transform.localScale = Vector3.one * startScale;
        mainText.color = inactiveColor;
        if (subText) subText.color = inactiveColor;
    }

    public void UpdateAnim(bool start)
    {
        if (isActive == start) return;

        isActive = start;
        if (isActive) StartFlashing();
        else StopFlashing();
    }

    private void StartFlashing()
    {
        transform.localScale = Vector3.one * endScale;
        mainText.color = activeColor;
        if (subText) subText.color = activeColor;

        pulseRoutine = StartCoroutine(PulseEffect());
    }

    private void StopFlashing()
    {
        if (flashObj != null) Destroy(flashObj);
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        transform.localScale = Vector3.one * startScale;
        mainText.color = inactiveColor;
        if (subText) subText.color = inactiveColor;
    }

    IEnumerator PulseEffect()
    {
        while (isActive)
        {
            flashObj = Instantiate(mainText.gameObject, mainText.transform.parent);
            TextMeshProUGUI flashText = flashObj.GetComponent<TextMeshProUGUI>();
            flashText.text = mainText.text;
            flashText.color = activeColor;
            flashObj.transform.SetSiblingIndex(mainText.transform.GetSiblingIndex() - 1);

            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;

                float scale = Mathf.Lerp(endScale, endScale * 1.5f, t);
                flashObj.transform.localScale = Vector3.one * scale;

                Color c = flashText.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                flashText.color = c;

                yield return null;
            }

            Destroy(flashObj);

            yield return new WaitForSeconds(pulseInterval);
        }
    }
}
