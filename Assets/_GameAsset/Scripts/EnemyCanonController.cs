using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(DamageSender))]
public class EnemyCanonController : CanonController_2
{
    #region PROPERTIES
    
    private float _angleYRotationNext;

    private float _angleRotationVelocity;

    private bool _isRotateToTarget;

    private EnemyController _enemyController;

    private bool _isReturnOriginAngle;

    private float _originLocalAngleY;

    private DamageSender _damageSender;
    
    #endregion
    
    #region UNITY CORE

    private void Start()
    {
        _originLocalAngleY = transform.localRotation.eulerAngles.y;
        _damageSender = GetComponent<DamageSender>();
    }
    
    public void Init(EnemyController enemyController)
    {
        _enemyController = enemyController;
        if (_lineBullet)
        {
            _lineBullet.positionCount = 0;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (_isRotateToTarget)
        {
            float rotateTime = 0.5f;
            
            float angleY = Mathf.SmoothDampAngle(_isReturnOriginAngle ? transform.localRotation.eulerAngles.y : transform.rotation.eulerAngles.y, _angleYRotationNext,
                ref _angleRotationVelocity, rotateTime);

            if (Mathf.Abs(Mathf.DeltaAngle(angleY, _angleYRotationNext)) <= 2f)
            {
                angleY = _angleYRotationNext;
                _isRotateToTarget = false;
            }

            if (_isReturnOriginAngle)
            {
                transform.localRotation = Quaternion.Euler(0,angleY,0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0,angleY,0);
            }
            
            
            if (!_isRotateToTarget)
            {
                if (_isReturnOriginAngle)
                {
                    if (_enemyController)
                    {
                        _enemyController.FinishSetUpFireCanon();
                    }
                }
                else
                {
                    OnFire();
                }
            }
        }
    }


    #endregion

    #region MAIN

    #region Fire

    public void SetUpFire(Transform target)
    {
        if(!target) return;

        _isRotateToTarget = true;
        _isReturnOriginAngle = false;
        _angleYRotationNext =
            Quaternion.LookRotation(target.position - transform.position).eulerAngles.y;
    }


    [Obsolete("Obsolete")]
    protected override void OnFire()
    {
        base.OnFire();
        if(!_pivotFire) return;

        float timeDelayRotate = 0.5f;
        StartCoroutine(DelayRotateOrigin(timeDelayRotate));
        if (Physics.Raycast(_pivotFire.position,_pivotFire.forward,out RaycastHit hit,_rangeFire))
        {
            if(hit.transform.CompareTag("Enemy")) return;
            _damageSender.SendDame(hit.transform);
            if (_lineBullet)
            {
                _lineBullet.positionCount = 2;
                _lineBullet.SetPosition(0,_pivotFire.position);
                _lineBullet.SetPosition(1,hit.point);
                PlayEffect(hit.point,hit.normal,hit.transform);
            }

            _timeResetLineDetail = _timeResetLine;
        }
    }

    IEnumerator DelayRotateOrigin(float time)
    {
        yield return new WaitForSeconds(time);
        _isRotateToTarget = true;
        _angleYRotationNext = _originLocalAngleY;
        _isReturnOriginAngle = true;
    }


    #endregion
    
    #endregion
}