using System;
using UnityEngine;
using UnityEngine.AI;

public class TankController : MonoBehaviour
{
    #region PROPERTIES
    [SerializeField] private NavMeshAgent _navAgent;
    [SerializeField] private Transform[] _movePositions;
    [SerializeField] private CanonController _canonController;
    [SerializeField] private int _bulletThreshold;
    
    private int _currentIndexPosition = -1;

    private float _angleYRotationNext;

    private float _angleRotationVelocity;

    private bool _canMove = true;

    private bool _isSetUpFireCanon;

    private Transform _boss;

    private int _bulletFireCount;
    
    #endregion

    #region UNITY CORE

    void Start()
    {
        Debug.Log("Hellosdjkflaskjdflka");
        if (_navAgent)
        {
            _navAgent.updateRotation = false;
            SetUpMoveNextPosition();
        }

        _boss = GameObject.FindGameObjectWithTag("Boss").transform;

        if (_canonController)
        {
            _canonController.Init(this);
        }
        
    }
    

    void Update()
    {
        if (GameManager.Instance && GameManager.Instance.IsFinishGame)
        {
            if (_navAgent)
            {
                _navAgent.ResetPath();
                _navAgent = null;
            }
            return;
        }
        if (_navAgent)
        {
            if (!_isSetUpFireCanon)
            {
                if (!_canMove)
                {
                    float angleY = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _angleYRotationNext,
                        ref _angleRotationVelocity, 0.2f);
                    if ( Mathf.Abs(Mathf.DeltaAngle(angleY, _angleYRotationNext)) <= .1f)
                    {
                        angleY = _angleYRotationNext;
                        OnMovePosition(_currentIndexPosition);
                    }
                
                    transform.rotation = Quaternion.Euler(0,angleY,0);
                }
                else if (_navAgent.remainingDistance <= _navAgent.stoppingDistance)
                {
                    _isSetUpFireCanon = true;
                    if (_canonController && _boss)
                    {
                        _canonController.SetUpFire(_boss);
                    }
                }
            }
        }
    }

    #endregion

    #region MAIN

    private void SetUpMoveNextPosition()
    {
        _currentIndexPosition++;
        if(_movePositions.Length == 0 || _navAgent == null || _currentIndexPosition < 0) return;

        if (_movePositions.Length <= _currentIndexPosition)
        {
            _currentIndexPosition = 0;
        }

        _canMove = false;

        _angleYRotationNext = Quaternion
            .LookRotation(_movePositions[_currentIndexPosition].position - transform.position).eulerAngles.y;
    }

    private void OnMovePosition(int index)
    {
        if(_movePositions.Length == 0 || _navAgent == null || index < 0) return;
        _canMove = true;
        _navAgent.SetDestination(_movePositions[index].position);
    }

    public void FinishSetUpFireCanon()
    {
        _bulletFireCount++;

        if (_bulletFireCount >= _bulletThreshold)
        {
            this.PostEvent(EventID.OnFinishGame,true);
            return;
        }
        
        _isSetUpFireCanon = false;
        SetUpMoveNextPosition();
    }
    #endregion
    
}
