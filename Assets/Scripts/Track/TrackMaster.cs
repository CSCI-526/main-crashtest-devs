using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

public class TrackMaster : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds2 = new WaitForSeconds(2f);
    public GameObject CanvasGO;
    public List<GameObject> trackPrefabs;
    public RectTransform titleRT;
    public GameObject trackObject;
    private BezierCurve lastCurve;


    private void Start()
    {
        TrackGen.trackPrefabs = trackPrefabs;

        StartCoroutine(TitleAnimation());

        TrackGen.CachePrefabData();
        TrackGen.MakeTrack();

        lastCurve = trackObject.transform.GetChild(0).GetComponent<BezierCurve>();
        for (int i = 0; i < TrackGen.raceTrack.Count; i++) SpawnNextSegment(TrackGen.raceTrack[i]);
    }

    private void SpawnNextSegment(GameObject prefab)
    {
        GameObject newSegment = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        for (int i = newSegment.transform.childCount - 1; i >= 0; i--)
            if (!newSegment.transform.GetChild(i).name.Contains('P')) DestroyImmediate(newSegment.transform.GetChild(i).gameObject);

        newSegment.GetComponent<BoxCollider>().enabled = false;
        newSegment.GetComponent<MeshRenderer>().enabled = false;
        newSegment.GetComponent<MeshCollider>().enabled = false;
        newSegment.GetComponent<RoadMesh>().enabled = false;
        BezierCurve newCurve = newSegment.GetComponent<BezierCurve>();

        Vector3 lastP2 = lastCurve.p2.position;
        Vector3 lastP3 = lastCurve.p3.position;

        Vector3 tangent = (lastP3 - lastP2).normalized;

        Vector3 offset = lastP3 - newCurve.p0.position;
        offset.y -= 0.001f;
        newSegment.transform.position += offset;

        Quaternion rotation = Quaternion.LookRotation(tangent, lastCurve.transform.up);
        newSegment.transform.rotation = rotation;

        lastCurve = newCurve;
    }

    private IEnumerator TitleAnimation()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;

        float duration = 1f;

        Vector3 startScale = titleRT.localScale;
        Vector3 endScale = new(0.7f, 0.7f, 1f);

        Vector2 startPos = titleRT.anchoredPosition;
        Vector2 endPos = new(0, 200f);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            titleRT.localScale = Vector3.Lerp(startScale, endScale, t);
            titleRT.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        StartCoroutine(AnimateCounts());
    }

    private IEnumerator AnimateCounts()
    {
        int[] counts = ComputeCounts();
        int s = counts[0];
        int t = counts[1];
        int h = counts[2] / 2;

        float total = counts[0] + counts[1] + counts[2];
        int wet = (int)(counts[3] / total * 100f);
        int dirt = (int)(counts[4] / total * 100f);

        // Start all rolls in parallel
        StartCoroutine(Roll("TextGO/S", s));
        StartCoroutine(Roll("TextGO/T", t));
        StartCoroutine(Roll("TextGO/H", h));
        StartCoroutine(Roll("TextGO/W", wet));
        StartCoroutine(Roll("TextGO/D", dirt));

        // Wait for the roll duration (they all run at the same time now)
        yield return new WaitForSeconds(0.75f);

        CenterAndScaleTrack();
    }

    private IEnumerator Roll(string path, int target)
    {
        DigitRoller roller = CanvasGO.transform.Find(path).GetComponent<DigitRoller>();
        yield return StartCoroutine(roller.RollToNumber(target));
    }

    private int[] ComputeCounts()
    {
        int[] counts = new int[] { 1, 0, 0, 0, 0 };

        for (int i = 0; i < TrackGen.raceTrack.Count; i++)
        {
            if (TrackGen.raceTrack[i].name.Contains("St")) counts[0]++;
            else if (TrackGen.raceTrack[i].name.Contains("L") || TrackGen.raceTrack[i].name.Contains("R")) counts[1]++;
            else if (TrackGen.raceTrack[i].name.Contains("u") || TrackGen.raceTrack[i].name.Contains("d")) counts[2]++;

            switch (TrackGen.roadTypes[i])
            {
                case RoadType.Wet: counts[3]++; break;
                case RoadType.Dirt: counts[4]++; break;
            }
        }

        return counts;
    }

    private void CenterAndScaleTrack()
    {
        float minX = 99999f, minZ = 99999f;
        float maxX = -99999f, maxZ = -99999f;

        for (int i = 0; i < trackObject.transform.childCount; i++)
        {
            Transform t = trackObject.transform.GetChild(i);

            if (t.position.x < minX) minX = t.position.x;
            if (t.position.z < minZ) minZ = t.position.z;
            if (t.position.x > maxX) maxX = t.position.x;
            if (t.position.z > maxZ) maxZ = t.position.z;
        }

        float trackWidth = maxX - minX;
        float trackHeight = maxZ - minZ;

        Vector3 center = new((minX + maxX) * 0.5f, trackObject.transform.position.y, (minZ + maxZ) * 0.5f);

        trackObject.transform.position -= center;

        Vector3[] worldCorners = new Vector3[4];
        CanvasGO.GetComponent<RectTransform>().GetWorldCorners(worldCorners);

        float targetWidth = Vector3.Distance(worldCorners[0] * .9f, worldCorners[3] * .9f);
        float targetHeight = Vector3.Distance(worldCorners[0] * .9f, worldCorners[1] * .9f);

        float bestScale = 0f;
        float bestAngle = 0f;

        for (int angleDeg = 0; angleDeg < 180; angleDeg++)
        {
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Abs(Mathf.Cos(angleRad));
            float sin = Mathf.Abs(Mathf.Sin(angleRad));

            float rotatedWidth = trackWidth * cos + trackHeight * sin;
            float rotatedHeight = trackWidth * sin + trackHeight * cos;

            float scaleW = targetWidth / rotatedWidth;
            float scaleH = targetHeight / rotatedHeight;

            float scale = Mathf.Min(scaleW, scaleH);

            if (scale > bestScale)
            {
                bestScale = scale;
                bestAngle = angleDeg;
            }
        }

        trackObject.transform.parent.localRotation = Quaternion.Euler(0, bestAngle, 0);
        trackObject.transform.parent.localScale = new Vector3(bestScale, bestScale, bestScale);

        StartCoroutine(AnimateAll(bestScale));

    }

    public float spawnDelay = .03f; // Faster track piece spawning
    public IEnumerator AnimateAll(float bestScale)
    {
        for (int i = -1; i < TrackGen.raceTrack.Count; i++)
        {
            StartCoroutine(AnimateNextTrackPiece(bestScale, i));
            yield return new WaitForSeconds(spawnDelay);
        }

        yield return new WaitForSeconds(0.75f); // Shorter wait before loading
        SceneManager.LoadScene(TrackGen.Scene2Load);
    }


    public float moveDuration = .05f;
    public IEnumerator AnimateNextTrackPiece(float bestScale, int index)
    {
        if (index + 1 >= trackObject.transform.childCount)
            yield break;

        int startPosIndex = 0;
        string startPosString = "Straight UI";
        int prefabIndex = 0;

        if (index != -1)
        {
            if (TrackGen.raceTrack[index].name.Contains("St")) startPosIndex = 0;
            else if (TrackGen.raceTrack[index].name.Contains("L") || TrackGen.raceTrack[index].name.Contains("R")) startPosIndex = 1;
            else if (TrackGen.raceTrack[index].name.Contains("u") || TrackGen.raceTrack[index].name.Contains("d")) startPosIndex = 2;

            switch (TrackGen.roadTypes[index])
            {
                case RoadType.Wet: startPosIndex = 3; break;
                case RoadType.Dirt: startPosIndex = 4; break;
            }

            switch (startPosIndex)
            {
                case 0: startPosString = "Straight UI"; break;
                case 1: startPosString = "Turn UI"; break;
                case 2: startPosString = "Hill UI"; break;
                case 3: startPosString = "Wet UI"; break;
                case 4: startPosString = "Dirt UI"; break;
            }

            if (TrackGen.raceTrack[index].name.Contains("St")) prefabIndex = 0;
            else if (TrackGen.raceTrack[index].name.Contains("L") || TrackGen.raceTrack[index].name.Contains("R"))
            {
                if (TrackGen.raceTrack[index].name.Contains("30")) prefabIndex = 3;
                else if (TrackGen.raceTrack[index].name.Contains("45")) prefabIndex = 5;
                else if (TrackGen.raceTrack[index].name.Contains("60")) prefabIndex = 7;
                else if (TrackGen.raceTrack[index].name.Contains("90")) prefabIndex = 9;

                if (TrackGen.raceTrack[index].name.Contains("R")) prefabIndex++;
            }
            else if (TrackGen.raceTrack[index].name.Contains("u")) prefabIndex = 1;
            else if (TrackGen.raceTrack[index].name.Contains("d")) prefabIndex = 2;
        }

        GameObject prefabToUse = trackPrefabs[prefabIndex];
        Vector3 startPos = CanvasGO.transform.Find(startPosString).position;

        GameObject piece = Instantiate(prefabToUse, startPos, Quaternion.identity);
        RoadMesh roadMesh = piece.GetComponentInChildren<RoadMesh>();
        roadMesh.roadType = TrackGen.roadTypes[index == -1 ? 0 : index];

        for (int i = piece.transform.childCount - 1; i >= 0; i--)
            if (!piece.transform.GetChild(i).name.Contains('P')) DestroyImmediate(piece.transform.GetChild(i).gameObject);

        foreach (var treeRB in piece.GetComponentsInChildren<TreeRB>()) treeRB.enabled = false;
        foreach (var rb in piece.GetComponentsInChildren<Rigidbody>()) rb.isKinematic = true;
        foreach (var rb2d in piece.GetComponentsInChildren<Rigidbody2D>()) rb2d.simulated = false;
        foreach (var col in piece.GetComponentsInChildren<Collider>()) col.enabled = false;
        foreach (var col2d in piece.GetComponentsInChildren<Collider2D>()) col2d.enabled = false;
        foreach (var thing in piece.GetComponentsInChildren<CheckPointTrigger>()) thing.enabled = false;
        foreach (var thing in piece.GetComponentsInChildren<MeshCollider>()) thing.enabled = false;

        Vector3 endPos;
        Quaternion endRot;
        if (index == -1)
        {
            endPos = new(trackObject.transform.GetChild(index + 1).position.x, trackObject.transform.GetChild(index + 1).position.y - 1000f, trackObject.transform.GetChild(index + 1).position.z);
            endRot = trackObject.transform.GetChild(index + 1).rotation;
        }
        else
        {
            BezierCurve lastCurve = trackObject.transform.GetChild(index).GetComponent<BezierCurve>();

            Vector3 P2 = lastCurve.p2.position;
            Vector3 P3 = lastCurve.p3.position;

            Vector3 tangent = (P3 - P2).normalized;
            Quaternion curveRot = Quaternion.LookRotation(tangent, lastCurve.transform.up);

            endPos = new(P3.x, trackObject.transform.GetChild(index + 1).position.y - 1000f, P3.z);
            endRot = curveRot;
        }

        float t = 0f;
        while (t < moveDuration)
        {
            t += 0.01f;
            float lerp = Mathf.Clamp01(t / moveDuration);

            piece.transform.SetPositionAndRotation(Vector3.Lerp(startPos, endPos, lerp), Quaternion.Slerp(Quaternion.identity, endRot, lerp));
            piece.transform.localScale = Vector3.Lerp(Vector3.one, new(bestScale, bestScale, bestScale), lerp);

            yield return null;
        }
        piece.transform.SetPositionAndRotation(endPos, endRot);
    }
}