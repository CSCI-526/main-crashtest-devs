using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class SendToGoogle : MonoBehaviour
{
    [SerializeField] private string baseURL = "https://docs.google.com/forms/d/e/1FAIpQLSde_bEOu758vRX9KBMkOcseaMh4n4YhW6E843XedV0GWnnHXw/formResponse";
    

    [Header("Form Entry IDs")]
    [SerializeField] private string sessionIDEntry = "entry.180420280";
    [SerializeField] private string segmentTypeEntry = "entry.1294476547";
    [SerializeField] private string surfaceTypeEntry = "entry.1404718073";
    [SerializeField] private string eventTypeEntry = "entry.457228854";
    [SerializeField] private string playerSpeedEntry = "entry.1688723416";
    [SerializeField] private string headlightIntensityEntry = "entry.315348212";
    [SerializeField] private string headlightRangeEntry = "entry.505370990";
    [SerializeField] private string driftUsedEntry = "entry.392201532";

    [Header("Race Completion Entry IDs")]
    [SerializeField] private string URLForProgressTrack = "https://docs.google.com/forms/d/e/1FAIpQLSciAWME8o19Qn18YzXNT3eQE8k_2x2DEMlrG6-vF52yO5bm_w/formResponse";

    [SerializeField] private string sessionIDEntry2 = "entry.521520923";
    [SerializeField] private string eventTypeEntry2 = "entry.1568276833";
    [SerializeField] private string completionTimeEntry = "entry.559683222";
    [SerializeField] private string progressPercentageEntry = "entry.1047091070";
    [SerializeField] private string crashCountEntry = "entry.1420641828";

    [Header("Turn Analytics Entry IDs")]
    [SerializeField] private string URLForTurnAnalytics = "https://docs.google.com/forms/d/e/1FAIpQLScFq9vSKpf6Vmq7ML4fQtH61C0iH5Z6A6JDK7Skign65krtqQ/formResponse";
    [SerializeField] private string sessionIDEntry3 = "entry.1506571139";
    [SerializeField] private string turnSegmentNameEntry = "entry.833411458";
    [SerializeField] private string driftUsedInTurnEntry = "entry.92877305";

    private long _sessionID;

    private void Awake()
    {
        _sessionID = DateTime.Now.Ticks;
    }
    public void Send(string segmentType, string surfaceType, string eventType, float playerSpeed, float headlightIntensity = -1f, float headlightRange = -1f, bool driftUsed = false)
    {
        StartCoroutine(Post(segmentType, surfaceType, eventType, playerSpeed, headlightIntensity, headlightRange, driftUsed));
    }

    private IEnumerator Post(string segmentType, string surfaceType,
        string eventType, float playerSpeed, float headlightIntensity, float headlightRange, bool driftUsed)
    {
        // Build URL with query parameters (Google Forms prefers GET with query string)
        string url = baseURL +
            "?" + sessionIDEntry + "=" + UnityWebRequest.EscapeURL(_sessionID.ToString()) +
            "&" + segmentTypeEntry + "=" + UnityWebRequest.EscapeURL(segmentType) +
            "&" + surfaceTypeEntry + "=" + UnityWebRequest.EscapeURL(surfaceType) +
            "&" + eventTypeEntry + "=" + UnityWebRequest.EscapeURL(eventType) +
            "&" + playerSpeedEntry + "=" + UnityWebRequest.EscapeURL(playerSpeed.ToString("F2")) +
            "&" + headlightIntensityEntry + "=" + UnityWebRequest.EscapeURL(headlightIntensity.ToString("F2")) +
            "&" + headlightRangeEntry + "=" + UnityWebRequest.EscapeURL(headlightRange.ToString("F2")) +
            "&" + driftUsedEntry + "=" + UnityWebRequest.EscapeURL(driftUsed.ToString());

        using UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Analytics submission failed: " + www.error);
        }
        else
        {
            Debug.Log("Crash analytics sent successfully");
        }
    }

    public void SendRaceCompletion(string eventType, float completionTime, float progressPercentage, int crashCount)
    {
        StartCoroutine(PostRaceCompletion(eventType, completionTime, progressPercentage, crashCount));
    }

    private IEnumerator PostRaceCompletion(string eventType, float completionTime, float progressPercentage, int crashCount)
    {
        // Build URL with query parameters for race completion
        string url = URLForProgressTrack +
            "?" + sessionIDEntry2 + "=" + UnityWebRequest.EscapeURL(_sessionID.ToString()) +
            "&" + eventTypeEntry2 + "=" + UnityWebRequest.EscapeURL(eventType) +
            "&" + completionTimeEntry + "=" + UnityWebRequest.EscapeURL(completionTime.ToString("F2")) +
            "&" + progressPercentageEntry + "=" + UnityWebRequest.EscapeURL(progressPercentage.ToString("F2")) +
            "&" + crashCountEntry + "=" + UnityWebRequest.EscapeURL(crashCount.ToString());

        using UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Race completion analytics submission failed: " + www.error);
        }
        else
        {
            Debug.Log($"Race completion analytics sent: {eventType}, Time: {completionTime:F2}s, Progress: {progressPercentage:F1}%, Crashes: {crashCount}");
        }
    }

    public void SendTurnAnalytics(string turnSegmentName, bool driftUsed)
    {
        StartCoroutine(PostTurnAnalytics(turnSegmentName, driftUsed));
    }

    private IEnumerator PostTurnAnalytics(string turnSegmentName, bool driftUsed)
    {
        // Build URL with query parameters for turn analytics (only successful turns)
        string url = URLForTurnAnalytics +
            "?" + sessionIDEntry3 + "=" + UnityWebRequest.EscapeURL(_sessionID.ToString()) +
            "&" + turnSegmentNameEntry + "=" + UnityWebRequest.EscapeURL(turnSegmentName) +
            "&" + driftUsedInTurnEntry + "=" + UnityWebRequest.EscapeURL(driftUsed.ToString());

        using UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Turn analytics submission failed: " + www.error);
        }
        else
        {
            Debug.Log($"Turn analytics sent: {turnSegmentName}, Drift: {driftUsed}");
        }
    }

    public string GetSessionID()
    {
        return _sessionID.ToString();
    }
}