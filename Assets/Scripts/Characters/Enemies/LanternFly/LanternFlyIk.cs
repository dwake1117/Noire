using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LanternFlyIk : MonoBehaviour
{
    public LayerMask terrainLayers;
    public Transform[] footTransforms;
    public CCDIK[] ccdiks;
    public Vector3[] footVectors;
    public Transform[] FootMarkers;
    public GameObject footMarkerPrefab;
    public float MoveDistanceDelta = 0.5f;
    public int FootMarkerSet = 0;
    public float MoveTime = 0.5f;
    public float averageY;
    public float VerticalSpeed;
    public float originalOffset;
    public float alternateFootDelay = 0.5f;
    public Transform lanterflyParent;
    public int[][] setIndices = new int[][]
    {
        new int[] { 0, 3, 4 },
        new int[] { 1, 2, 5 }
    };

    public float idleScaler = 0.25f;
    public float idleSpeedScaler = 4f;

    public Transform NavmeshParent;
    public bool footMoving = false;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 vector in footVectors)
        {
            Gizmos.DrawSphere(transform.position - (vector.x * transform.right + vector.y * transform.up + vector.z * transform.forward), 0.5f);
        }
        // Draws a blue sphere with a radius of 0.5 units
    }

    private Vector3[] startInterpolationPositions;
    public float[] interpolationProgress; // For each foot

    void Start()
    {
        originalOffset = transform.position.y - CalcAverageY();
    }

    void RotateAroundAxis(Vector3 axis)
    {
        float angle = 0.1f;
        for(int i = 0; i< 6; i++)
            FootMarkers[i].RotateAround(transform.position, axis, angle);
    }
    void Update()
    {
        transform.forward = NavmeshParent.forward;
        if (footMoving) 
            RotateAroundAxis(Vector3.up);
        averageY = CalcAverageY() + originalOffset;
        transform.position = Vector3.Lerp(transform.position,
            new Vector3(NavmeshParent.position.x, averageY - Mathf.Abs(Mathf.Sin(Time.time * idleSpeedScaler)) * idleScaler, NavmeshParent.position.z), VerticalSpeed * Time.deltaTime);
        Vector3 NormalAverage = Vector3.zero;
        RaycastHit hit;
        for (int i = 0; i < 6; i++)
        {
            if (Physics.Raycast(FootMarkers[i].position + transform.up * 20, -transform.up, out hit, terrainLayers))
            {
                NormalAverage += hit.normal;
            }
        }

        NormalAverage /= 6;
        //transform.up = NormalAverage;
        if(footMoving)
            return;
        for (int i = 0; i < 6; i++)
        {
            if (Vector3.Distance(transform.position - (footVectors[i].x * transform.right + footVectors[i].y * transform.up + footVectors[i].z * transform.forward), FootMarkers[i].position) > MoveDistanceDelta)
            {
                StartCoroutine(MoveFootSet(1));
                break;
            }
        }
    }

    IEnumerator MoveFootSet(int i)
    {
        footMoving = true;
        StartCoroutine(MoveFoot(transform.position -  (footVectors[setIndices[i][0]].x * transform.right + footVectors[setIndices[i][0]].y * transform.up + footVectors[setIndices[i][0]].z * transform.forward), FootMarkers[setIndices[i][0]]));
        StartCoroutine(MoveFoot(transform.position -  (footVectors[setIndices[i][2]].x * transform.right + footVectors[setIndices[i][2]].y * transform.up + footVectors[setIndices[i][2]].z * transform.forward), FootMarkers[setIndices[i][2]]));
        yield return new WaitForSeconds(alternateFootDelay);
        StartCoroutine(MoveFoot(transform.position -  (footVectors[setIndices[i][1]].x * transform.right + footVectors[setIndices[i][1]].y * transform.up + footVectors[setIndices[i][1]].z * transform.forward), FootMarkers[setIndices[i][1]]));
        yield return new WaitForSeconds(alternateFootDelay);
        StartCoroutine(MoveFootSet(Math.Abs(i - 1), true));
    }
    IEnumerator MoveFootSet(int i, bool finalMove)
    {
        
        StartCoroutine(MoveFoot(transform.position -  (footVectors[setIndices[i][0]].x * transform.right + footVectors[setIndices[i][0]].y * transform.up + footVectors[setIndices[i][0]].z * transform.forward), FootMarkers[setIndices[i][0]]));
        StartCoroutine(MoveFoot(transform.position -  (footVectors[setIndices[i][2]].x * transform.right + footVectors[setIndices[i][2]].y * transform.up + footVectors[setIndices[i][2]].z * transform.forward), FootMarkers[setIndices[i][2]]));
        yield return new WaitForSeconds(alternateFootDelay);
        StartCoroutine(MoveFoot(transform.position -  (footVectors[setIndices[i][1]].x * transform.right + footVectors[setIndices[i][1]].y * transform.up + footVectors[setIndices[i][1]].z * transform.forward), FootMarkers[setIndices[i][1]]));
        yield return new WaitForSeconds(alternateFootDelay);
        //lanterflyParent.position = new Vector3(transform.position.x, lanterflyParent.position.y, transform.position.z);
        footMoving = false;
    }
private float CalcAverageY()
    {
        float sum = 0;
        for(int i = 0; i < 6; i++)
        {
            sum += FootMarkers[i].position.y;
        }

        return sum / 6;
    }
    private IEnumerator MoveFoot(Vector3 currentFootPos, Transform beginPos)
    {
        // raycast down and find grounded foot position
        
        RaycastHit hit;
        if (Physics.Raycast(currentFootPos + transform.up * 20, -transform.up, out hit, 100f, terrainLayers))
        { 
            currentFootPos = hit.point;
        }
        else
        {
            Debug.Log("Not grounded");
        }

        float timer = 0;
        while(timer < MoveTime)
        {
            timer += Time.deltaTime;
            beginPos.position = Vector3.Lerp(beginPos.position, currentFootPos, timer / MoveTime);
            yield return null;
        }
    }
    Vector3 CalculateAverageNormal(Transform[] points)
    {
        // Ensure we have at least 3 points
        if (points.Length < 3)
        {
            Debug.LogError("Not enough points to calculate the average normal.");
            return Vector3.zero;
        }

        // Calculate the normal vector for each set of three adjacent points
        Vector3[] normals = new Vector3[points.Length - 2];
        for (int i = 0; i < normals.Length; i++)
        {
            Vector3 v1 = points[i + 1].position - points[i].position;
            Vector3 v2 = points[i + 2].position - points[i].position;
            normals[i] = Vector3.Cross(v1, v2).normalized;
        }

        // Sum up all the normal vectors
        Vector3 sumNormals = Vector3.zero;
        for (int i = 0; i < normals.Length; i++)
        {
            sumNormals += normals[i];
        }

        // Divide the sum by the total number of normal vectors
        Vector3 averageNormal = sumNormals / normals.Length;

        return averageNormal;
    }
    // This method calculates vectors from transform.position to each footTransform's position
    public void CalculateVectors()
    {
        footVectors = new Vector3[footTransforms.Length];
        for(int i = 0; i < footTransforms.Length; i++)
        {
            if(footTransforms[i] != null)
            {
                Vector3 vectorToFoot = transform.position - footTransforms[i].position;
                footVectors[i] = vectorToFoot;
            }
        }
    }

    // This method spawns a GameObject at each footTransform's position
    public void SpawnFootMarkers()
    {
        if (footMarkerPrefab == null)
        {
            Debug.LogError("Foot marker prefab is not assigned!");
            return;
        }

        FootMarkers = new Transform[ccdiks.Length];
        for (int i = 0; i < footTransforms.Length; i++)
        {
            if (footTransforms[i] != null)
            {
                GameObject obj = Instantiate(footMarkerPrefab, footTransforms[i].position, Quaternion.identity);
                ccdiks[i].solver.target = obj.transform;
                FootMarkers[i] = obj.transform;
            }
        }
    }

#if UNITY_EDITOR
    // Draw buttons in the Unity editor
    [CustomEditor(typeof(LanternFlyIk))]
    public class LanternFlyIkEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LanternFlyIk script = (LanternFlyIk)target;

            if (GUILayout.Button("Calculate Vectors"))
            {
                script.CalculateVectors();
            }

            if (GUILayout.Button("Spawn Foot Markers"))
            {
                script.SpawnFootMarkers();
            }
        }
    }
#endif
}