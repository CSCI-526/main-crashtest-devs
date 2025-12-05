using UnityEngine;

public class TrackSpawn : MonoBehaviour
{
    public GameObject trackObject;
    public bool isTutorial = false;
    private int segmentCount = 1;
    private BezierCurve lastCurve;


    private void Start()
    {
        if (!isTutorial)
        {
            lastCurve = trackObject.transform.GetChild(0).GetComponent<BezierCurve>();
            for (int i = 0; i < TrackGen.raceTrack.Count; i++) SpawnNextSegment(TrackGen.raceTrack[i], TrackGen.roadTypes[i]);
        }

        trackObject.GetComponent<Racetrack>().AddTrackCurves();
    }


    private void SpawnNextSegment(GameObject prefab, RoadType roadType)
    {
        GameObject newSegment = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        newSegment.GetComponent<BoxCollider>().enabled = false;
        newSegment.name += $" {segmentCount}";
        segmentCount++;
        BezierCurve newCurve = newSegment.GetComponent<BezierCurve>();

        Vector3 lastP2 = lastCurve.p2.position;
        Vector3 lastP3 = lastCurve.p3.position;

        Vector3 tangent = (lastP3 - lastP2).normalized;

        Vector3 offset = lastP3 - newCurve.p0.position;
        offset.y -= 0.001f;
        newSegment.transform.position += offset;

        Quaternion rotation = Quaternion.LookRotation(tangent, lastCurve.transform.up);
        newSegment.transform.rotation = rotation;

        // Set road type and segment name for analytics
        RoadMesh roadMesh = newSegment.GetComponentInChildren<RoadMesh>();
        roadMesh.roadType = roadType;

        // Extract segment name from prefab (remove suffix if present)
        string segmentName = prefab.name.Replace(" Variant", "").Replace("(Clone)", "").Trim();
        roadMesh.segmentName = segmentName;

        lastCurve = newCurve;
    }

}
