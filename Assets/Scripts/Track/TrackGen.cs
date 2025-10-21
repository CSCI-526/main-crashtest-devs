using System.Collections.Generic;
using UnityEngine;

public class TrackGen : MonoBehaviour
{
    public List<GameObject> trackPrefabs;
    public GameObject trackObject;
    private BezierCurve lastCurve;
    private int segmentCount = 1;
    private enum TrackSegmentsChoices { smallS, largeS, smallT, largeT }
    /*
        smallS: 3-4 straights,                  # -> 4-5
        largeS: 5-7 straights,                  # -> 4-5
        smallT: 2-3 turns,                      # -> 5-6
        largeT: 4-6 turns,                      # -> 1-2
        slope: deltaSlope during straights,     # -> 2-4

        short case: 4*3 + 4*5 + 5*2 + 1*4 + 2*2 = 50 ( + 16 + 2 = 68)
        long case: 5*4 + 5*7 + 6*3 + 2*6 + 4*2 = 93 ( + 22 + 2 = 117)
    */

    private readonly List<GameObject> raceTrack = new();
    private int numberOfRedos = 0;


    private void Start()
    {
        MakeTrack();
    }

    private void MakeTrack()
    {
        bool success = false;
        int specialCount = 0;
        RoadType roadType = RoadType.Normal;

        while (!success && numberOfRedos < 100)
        {
            raceTrack.Clear();
            segmentCount = 1;

            for (int i = trackObject.transform.childCount - 1; i >= 1; i--)
                DestroyImmediate(trackObject.transform.GetChild(i).gameObject);

            GenerateTrack();

            lastCurve = trackObject.transform.GetChild(0).GetComponent<BezierCurve>();
            raceTrack.Add(trackPrefabs[^1]);
            foreach (GameObject prefab in raceTrack)
            {
                if (specialCount == 0)
                {
                    float randomValue = Random.value;
                    if (randomValue <= 0.05)
                    {
                        roadType = RoadType.Dirt;
                        specialCount = Random.Range(4, 8);
                    }
                    else if (randomValue >= 0.95)
                    {
                        roadType = RoadType.Wet;
                        specialCount = Random.Range(2, 6);
                    }
                    else roadType = RoadType.Normal;
                }
                else specialCount--;
                SpawnNextSegment(prefab, roadType);
            }

            if (CheckTrack())
            {
                success = true;
                foreach (Transform child in trackObject.transform)
                    child.GetComponent<BoxCollider>().enabled = false;
            }
            numberOfRedos++;
        }

        trackObject.GetComponent<Racetrack>().AddTrackCurves();

    }

    private void GenerateTrack()
    {
        Dictionary<TrackSegmentsChoices, int> summary = new();
        List<TrackSegmentsChoices> segmentList = new();
        int numSlopes = Random.Range(2, 5);

        void AddSegments(TrackSegmentsChoices type, int minGroups, int maxGroups)
        {
            int groupCount = Random.Range(minGroups, maxGroups + 1);
            if (!summary.ContainsKey(type))
                summary[type] = 0;
            summary[type] += groupCount;
        }

        AddSegments(TrackSegmentsChoices.smallS, 4, 5);
        AddSegments(TrackSegmentsChoices.largeS, 4, 6);
        AddSegments(TrackSegmentsChoices.smallT, 5, 6);
        AddSegments(TrackSegmentsChoices.largeT, 1, 2);

        foreach (KeyValuePair<TrackSegmentsChoices, int> entry in summary) for (int i = 0; i < entry.Value; i++) segmentList.Add(entry.Key);

        for (int i = 0; i < segmentList.Count; i++)
        {
            int rand = Random.Range(i, segmentList.Count);
            (segmentList[i], segmentList[rand]) = (segmentList[rand], segmentList[i]);
        }

        List<int> straightLoc = new();
        for (int i = 0; i < segmentList.Count; i++)
        {
            TrackSegmentsChoices segment = segmentList[i];
            if (segment == TrackSegmentsChoices.smallS || segment == TrackSegmentsChoices.largeS) straightLoc.Add(i);
        }

        List<int> slopeLoc = new();
        for (int i = 0; i < numSlopes; i++)
        {
            int randomIndex = Random.Range(0, straightLoc.Count);
            slopeLoc.Add(straightLoc[randomIndex]);
            straightLoc.RemoveAt(randomIndex);
        }

        void AddPrefabs(int prefabsMin, int prefabsMax, int minPrefabs, int maxPrefabs, bool slope = false)
        {
            int numPrefabs = Random.Range(minPrefabs, maxPrefabs + 1);
            if (slope)
            {
                int index1 = Random.Range(0, numPrefabs);
                int index2 = Random.Range(2, numPrefabs + 1);
                if (index1 == index2) index2 = (Random.Range(0, 2) == 0) ? index2 + 1 : index2 - 1;

                int indexInc = index2, indexDec = index1;

                bool incFirst = true && (Random.Range(0, 2) == 0);
                if (incFirst)
                {
                    indexInc = index1;
                    indexDec = index2;
                }

                for (int i = 0; i < numPrefabs + 2; i++)
                {
                    if (i == indexInc) raceTrack.Add(trackPrefabs[1]);
                    else if (i == indexDec) raceTrack.Add(trackPrefabs[2]);
                    else raceTrack.Add(trackPrefabs[0]);
                }
            }
            else for (int i = 0; i < numPrefabs; i++) raceTrack.Add(trackPrefabs[Random.Range(prefabsMin, prefabsMax + 1)]);

        }

        for (int i = 0; i < segmentList.Count; i++)
        {
            TrackSegmentsChoices choice = segmentList[i];
            switch (choice)
            {
                case TrackSegmentsChoices.smallS:
                    AddPrefabs(0, 0, 3, 4, slopeLoc.Contains(i));
                    raceTrack.Add(trackPrefabs[Random.Range(3, 11)]);
                    break;
                case TrackSegmentsChoices.largeS:
                    AddPrefabs(0, 0, 5, 7, slopeLoc.Contains(i));
                    raceTrack.Add(trackPrefabs[Random.Range(3, 11)]);
                    break;
                case TrackSegmentsChoices.smallT:
                    AddPrefabs(3, 10, 2, 3);
                    raceTrack.Add(trackPrefabs[0]);
                    break;
                case TrackSegmentsChoices.largeT:
                    AddPrefabs(3, 10, 4, 6);
                    raceTrack.Add(trackPrefabs[0]);
                    break;
            }
        }
    }

    private void SpawnNextSegment(GameObject prefab, RoadType roadType)
    {
        GameObject newSegment = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
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

        // randomly assign road type (40% special, 60% normal)
        RoadMesh roadMesh = newSegment.GetComponentInChildren<RoadMesh>();
        roadMesh.roadType = roadType;

        lastCurve = newCurve;
    }

    private bool CheckTrack()
    {
        Physics.SyncTransforms();

        BoxCollider one;
        BoxCollider two;
        int numSegments = raceTrack.Count;

        for (int i = 3; i < numSegments; i++)
        {
            one = trackObject.transform.GetChild(i).GetComponent<BoxCollider>();

            for (int j = i - 2; j >= 0; j--)
            {
                two = trackObject.transform.GetChild(j).GetComponent<BoxCollider>();
                if (Physics.ComputePenetration(one, one.transform.position, one.transform.rotation, two, two.transform.position, two.transform.rotation, out Vector3 _, out float _))
                    return false;
            }
        }

        return true;
    }
}
