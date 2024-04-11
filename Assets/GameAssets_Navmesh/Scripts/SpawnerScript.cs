using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    public Transform[] SpawmPoints;
    public GameObject PrefabSpawm;
    public int SpawmCount;
    public bool Run;

    private void Update()
    {
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