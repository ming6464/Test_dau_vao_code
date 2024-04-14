using System;
using Unity.Mathematics;
using UnityEngine;

public class TestNAN : MonoBehaviour
{
    public float3 VT;
    public float DistanceSet;
    public bool LoadObstacle;
    public GameObject[] ObstacleGobj;
    public float3 MyPosition;
    public bool CheckSquare;
    public float DistanceCheck;

    private void OnValidate()
    {
        DistanceCheck = math.pow(DistanceSet, CheckSquare ? 2 : 1);
        MyPosition = transform.position;
        if (!LoadObstacle) return;
        LoadObstacle = false;
        ObstacleGobj = GameObject.FindGameObjectsWithTag("Obstacle");
    }

    private void Update()
    {
        VT = float3.zero;
        foreach (var obs in ObstacleGobj)
        {
            float3 obsPos = obs.transform.position;
            Debug.DrawLine(MyPosition, obsPos);
            var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 10000));
            try
            {
                float length;
                float3 vtNormal = MyPosition - obsPos;
                if (vtNormal.Equals(float3.zero))
                {
                    vtNormal += new float3(random.NextFloat(-.3f, .3f), 0, random.NextFloat(-.3f, .3f));
                }

                if (CheckSquare)
                {
                    length = math.lengthsq(vtNormal);
                }
                else
                {
                    length = math.length(vtNormal);
                }

                if (math.isnan(length))
                {
                    Debug.Log($"Index {obs.transform.GetSiblingIndex()} is NAN0");
                }

                if (length > DistanceCheck) continue;

                if (math.any(math.isnan(vtNormal)))
                {
                    Debug.Log($"Index {obs.transform.GetSiblingIndex()} is NAN2");
                }

                vtNormal = math.normalize(vtNormal);

                if (math.any(math.isnan(vtNormal)))
                {
                    Debug.Log($"Index {obs.transform.GetSiblingIndex()} is NAN3");
                }

                VT += vtNormal * (DistanceCheck - length);
            }
            catch
            {
                Debug.Log("catch");
            }
        }

        try
        {
            Debug.DrawRay(MyPosition, math.normalize(VT) * 5f, Color.red);
        }
        catch
        {
            Debug.Log("catch2");
        }
    }
}