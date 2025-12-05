using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class DigitRoller : MonoBehaviour
{
    public TMP_Text text;
    public float rollDuration = 0.75f;
    public AnimationCurve rollCurve;
    public bool isPercent = false;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();

        if (rollCurve == null || rollCurve.keys.Length == 0) rollCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    public IEnumerator RollToNumber(int target)
    {
        int start = 0;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / rollDuration;
            float ease = rollCurve.Evaluate(t);

            float val = Mathf.Lerp(start, target, ease);
            int count = Mathf.RoundToInt(val);

            if (isPercent) text.text = $"{count} %";
            else text.text = $"x {count}";

            yield return null;
        }

        if (isPercent) text.text = $"{target} %";
        else text.text = $"x {target}";
    }
}
