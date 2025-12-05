using System.Collections.Generic;
using UnityEngine;

public static class TrackGen
{
    public static List<GameObject> trackPrefabs;
    public static readonly List<GameObject> raceTrack = new();
    public static readonly List<RoadType> roadTypes = new();
    private static VirtualSegment lastVirtual;
    private enum TrackSegmentsChoices { smallS, largeS, smallT, largeT }
    /*
        smallS: 3-4 straights,                  # -> 4-5 prefabs
        largeS: 5-7 straights,                  # -> 4-5 ...
        smallT: 2-3 turns,                      # -> 5-6
        largeT: 4-6 turns,                      # -> 1-2
        slope: deltaSlope during straights,     # -> 2-4

        short case: 4*3 + 4*5 + 5*2 + 1*4 + 2*2 = 50 ( + 16 + 2 = 68 prefabs)
        long case: 5*4 + 5*7 + 6*3 + 2*6 + 4*2 = 93 ( + 22 + 2 = 117 prefabs)
    */
    public static int numberOfRedos = 0;
    public static bool success = false;
    private static readonly Dictionary<GameObject, PrefabData> prefabData = new();
    private class PrefabData
    {
        public Vector3 colliderCenter, colliderSize;
        public Vector3 p0, p1, p2, p3;
    }
    private static readonly List<VirtualSegment> virtualTrack = new();
    private struct VirtualSegment
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 colliderCenter;
        public Vector3 colliderSize;

        public Vector3 p0, p1, p2, p3;
    }
    public static string Scene2Load;

    public static void CachePrefabData()
    {
        prefabData.Clear();

        foreach (var prefab in trackPrefabs)
        {
            BoxCollider bc = prefab.GetComponent<BoxCollider>();
            BezierCurve curve = prefab.GetComponent<BezierCurve>();

            PrefabData pd = new()
            {
                colliderCenter = bc.center,
                colliderSize = bc.size,

                p0 = curve.p0.localPosition,
                p1 = curve.p1.localPosition,
                p2 = curve.p2.localPosition,
                p3 = curve.p3.localPosition
            };

            prefabData[prefab] = pd;
        }
    }

    private static void InitializeStartVirtual(GameObject startGO)
    {
        virtualTrack.Clear();

        var data = prefabData[startGO];

        VirtualSegment start = new()
        {
            position = Vector3.zero,
            rotation = Quaternion.identity,

            p0 = data.p0,
            p1 = data.p1,
            p2 = data.p2,
            p3 = data.p3,

            colliderCenter = data.colliderCenter,
            colliderSize = data.colliderSize
        };

        virtualTrack.Add(start);
        lastVirtual = start;
    }

    public static void MakeTrack()
    {
        success = false;
        int specialCount = 0;
        RoadType roadType = RoadType.Normal;

        while (!success && numberOfRedos < 100)
        {
            raceTrack.Clear();
            roadTypes.Clear();
            GenerateTrack();

            InitializeStartVirtual(trackPrefabs[^2]);

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
                roadTypes.Add(roadType);

                SpawnNextSegmentVirtual(prefab);
            }

            if (CheckTrackVirtual()) success = true;
            numberOfRedos++;
        }
    }

    private static void GenerateTrack()
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

    private static void SpawnNextSegmentVirtual(GameObject prefab)
    {
        var data = prefabData[prefab];

        Vector3 lastP2 = lastVirtual.p2;
        Vector3 lastP3 = lastVirtual.p3;

        Vector3 tangent = (lastP3 - lastP2).normalized;

        Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up);

        Vector3 worldP0 = lastP3;

        Vector3 offset = worldP0 - (rotation * data.p0);
        offset.y -= 0.001f; // dont actually know if this fixes the random collision bug

        VirtualSegment seg = new()
        {
            position = offset,
            rotation = rotation,

            p0 = offset + rotation * data.p0,
            p1 = offset + rotation * data.p1,
            p2 = offset + rotation * data.p2,
            p3 = offset + rotation * data.p3,

            colliderCenter = data.colliderCenter,
            colliderSize = data.colliderSize
        };

        virtualTrack.Add(seg);
        lastVirtual = seg;
    }

    private static bool CheckTrackVirtual()
    {
        int count = virtualTrack.Count;

        for (int i = 3; i < count; i++)
        {
            for (int j = 0; j < i - 2; j++)
            {
                if (OBBOverlap(virtualTrack[i], virtualTrack[j]))
                    return false;
            }
        }

        return true;
    }
    
    private static bool OBBOverlap(in VirtualSegment a, in VirtualSegment b)
    {
        Vector3 aExt = a.colliderSize * 0.5f;
        Vector3 bExt = b.colliderSize * 0.5f;

        Vector3 aCenter = a.position + a.rotation * a.colliderCenter;
        Vector3 bCenter = b.position + b.rotation * b.colliderCenter;

        Vector3[] aAxes = {
            a.rotation * Vector3.right,
            a.rotation * Vector3.up,
            a.rotation * Vector3.forward
        };

        Vector3[] bAxes = {
            b.rotation * Vector3.right,
            b.rotation * Vector3.up,
            b.rotation * Vector3.forward
        };

        Vector3 T = bCenter - aCenter;

        float R, R0, R1;

        for (int i = 0; i < 3; i++)
        {
            R = Mathf.Abs(Vector3.Dot(T, aAxes[i]));
            R0 = aExt[i];
            R1 = Mathf.Abs(Vector3.Dot(bExt.x * bAxes[0], aAxes[i])) + Mathf.Abs(Vector3.Dot(bExt.y * bAxes[1], aAxes[i])) + Mathf.Abs(Vector3.Dot(bExt.z * bAxes[2], aAxes[i]));

            if (R > R0 + R1) return false;
        }

        for (int i = 0; i < 3; i++)
        {
            R = Mathf.Abs(Vector3.Dot(T, bAxes[i]));
            R0 = Mathf.Abs(Vector3.Dot(aExt.x * aAxes[0], bAxes[i])) + Mathf.Abs(Vector3.Dot(aExt.y * aAxes[1], bAxes[i])) + Mathf.Abs(Vector3.Dot(aExt.z * aAxes[2], bAxes[i]));
            R1 = bExt[i];

            if (R > R0 + R1) return false;
        }

        return true;
    }
}
