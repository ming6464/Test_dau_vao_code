using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    public Transform[] SpawmPoints;
    public GameObject PrefabSpawm;
    public int SpawmCount;
    public bool Run;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, math.INFINITY))
            {
                EventDispatcher.Instance.PostEvent(EventID.LeftClick, hit.point);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            EventDispatcher.Instance.PostEvent(EventID.RightClick);
        }

        if (Run)
        {
            Run = false;
            int spawnPointCount = SpawmPoints.Length;
            for (int i = 0; i < SpawmCount; i++)
            {
                int index = i % spawnPointCount;
                Instantiate(PrefabSpawm, SpawmPoints[index].position, Quaternion.identity);
            }
        }
    }
}