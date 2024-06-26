﻿using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    #region PROPERTIES
    [SerializeField]
    private float _thresholdDecreasesTime;
    [SerializeField] private float _timeMinSpawn;
    [SerializeField] private float _timeSpawn;
    [SerializeField] private Transform[] _spawnPoints;
    private int priorityCount;
    #endregion

    #region UNITY CORE

    private void Start()
    {
        Invoke(nameof(SpawnEnemy),_timeSpawn);
    }

    #endregion

    #region MAIN

    private void SpawnEnemy()
    {
        priorityCount++;
        if(GameManager.Instance && GameManager.Instance.IsFinishGame) return;
        if(!GameObjectPooling.Instance  || _spawnPoints.Length == 0) return;
        _timeSpawn -= _thresholdDecreasesTime;
        if (_timeSpawn < _timeMinSpawn) _timeSpawn = _timeMinSpawn;
        GameObject enemyNew = GameObjectPooling.Instance.Pull(PoolKEY.Enemy);
        int randomIndex = Random.Range(0, _spawnPoints.Length);
        enemyNew.transform.parent = transform;
        enemyNew.transform.position = _spawnPoints[randomIndex].position;
        enemyNew.transform.GetComponent<NavMeshAgent>().avoidancePriority = priorityCount;
        if (GameConfig_.Instance)
        {
            enemyNew.transform.GetComponent<EnemyColorSet>().SetColor(GameConfig_.Instance.GetColor(priorityCount));
        }
        
        enemyNew.SetActive(true);
        Invoke(nameof(SpawnEnemy),_timeSpawn);
    }

    #endregion
}