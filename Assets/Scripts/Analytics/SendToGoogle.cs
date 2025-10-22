using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class SendToGoogle : MonoBehaviour
{
    [SerializeField] private string URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSde_bEOu758vRX9KBMkOcseaMh4n4YhW6E843XedV0GWnnHXw/formResponse";

    [Header("Form Entry IDs")]
    [SerializeField] private string sessionIDEntry = "entry.180420280";
    [SerializeField] private string segmentTypeEntry = "entry.1294476547";
    [SerializeField] private string surfaceTypeEntry = "entry.1404718073";
    [SerializeField] private string eventTypeEntry = "entry.457228854";
    [SerializeField] private string playerSpeedEntry = "entry.1688723416";

    private long _sessionID;

    private void Awake()
    {
        _sessionID = DateTime.Now.Ticks;
    }
    public void Send(string segmentType, string surfaceType, string eventType, float playerSpeed)
    {
        StartCoroutine(Post(segmentType, surfaceType, eventType, playerSpeed));
    }

    private IEnumerator Post(string segmentType, string surfaceType,
        string eventType, float playerSpeed)
    {
        WWWForm form = new WWWForm();
        form.AddField(sessionIDEntry, _sessionID.ToString());
        form.AddField(segmentTypeEntry, segmentType);
        form.AddField(surfaceTypeEntry, surfaceType);
        form.AddField(eventTypeEntry, eventType);
        form.AddField(playerSpeedEntry, playerSpeed.ToString("F2"));

        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
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
    }

    public string GetSessionID()
    {
        return _sessionID.ToString();
    }
}