using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyDamageReceiver))]
public class EnemyController : MonoBehaviour
{
    #region PROPERTIES
    [SerializeField] private EnemyCanonController _enemyCanon;
    
    [Min(0f)]
    [SerializeField] private float _minDistanceRandomThreshold;
    [Min(1f)]
    [SerializeField] private float _maxDistanceRandomThreshold;

    [SerializeField] private float _distanceBetweenNextPosition;
    
    private NavMeshAgent _navAgent;
    
    private float _angleYRotationNext;

    private float _angleRotationVelocity;

    private bool _canMove = true;

    private bool _isSetUpFireCanon;

    private Transform _player;

    private EnemyDamageReceiver _enemyDamageReceiver;

    #endregion

    #region UNITY CORE

    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _enemyDamageReceiver = GetComponent<EnemyDamageReceiver>();
    }

    private void OnEnable()
    {
        if (_player == null)
        {
            _player = GameObject.FindWithTag("Player").transform;
        }
        
        _navAgent.ResetPath();

        SetUpMoveNextPosition();
        
        _enemyDamageReceiver.Revival();
    }

    private void OnDisable()
    {

    }

    void Start()
    {
        if (_enemyCanon)
        {
            _enemyCanon.Init(this);
        }
        
    }
    

    void Update()
    {
        if (GameManager.Instance && GameManager.Instance.IsFinishGame)
        {
            if (_player)
            {
                _navAgent.ResetPath();
                _player = null;
            }
            return;
        }
        if (_navAgent)
        {
            if (!_isSetUpFireCanon)
            {
                if (_navAgent.remainingDistance <= _navAgent.stoppingDistance)
                {
                    _navAgent.ResetPath();
                    _isSetUpFireCanon = true;
                    if (_enemyCanon)
                    {
                        _enemyCanon.SetUpFire(_player);
                    }
                }
            }
        }
    }

    #endregion

    #region MAIN

    private void SetUpMoveNextPosition()
    {
        if(_player == null) return;

        Vector3 nextPosition;

        do
        {
            float distanceXRandom = Random.Range(_minDistanceRandomThreshold, _maxDistanceRandomThreshold);
            float distanceZRandom = Random.Range(_minDistanceRandomThreshold, _maxDistanceRandomThreshold);
        
            int ChangeDirectX = Random.Range(0, 2);
            int ChangeDirectZ = Random.Range(0, 2);
        
            distanceXRandom *= ChangeDirectX > 0 ? -1 : 1;
            distanceZRandom *= ChangeDirectZ > 0 ? -1 : 1;
        
            nextPosition = _player.position + new Vector3(distanceXRandom, 0, distanceZRandom);
        } while (Vector3.Distance(nextPosition, transform.position) <= _distanceBetweenNextPosition);

        
        _navAgent.SetDestination(nextPosition);
    }


    public void FinishSetUpFireCanon()
    {
        SetUpMoveNextPosition();
        _isSetUpFireCanon = false;
    }
    #endregion
}