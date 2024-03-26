using System;
using UnityEngine;

[RequireComponent(typeof(DamageSender))]
public class PlayerCanonController : CanonController_2
{
    #region PROPERTIES

    [SerializeField] private float _timeReset;
    private DamageSender _damageSender;
    private float _timeResetDetail;
    #endregion

    #region UNITY CORE

    private void Awake()
    {
        _damageSender = GetComponent<DamageSender>();
    }

    protected override void Update()
    {
        if(GameManager.Instance && GameManager.Instance.IsFinishGame) return;
        base.Update();
        _timeResetDetail -= Time.deltaTime;
    }

    #endregion
    
    #region MAIN

    public override void HandleFire()
    {
        if(_timeResetDetail > 0) return;
        _timeResetDetail = _timeReset;
        OnFire();
    }

    protected override void OnFire()
    {
        base.OnFire();
        if(_pivotFire == null) return;
        if (Physics.Raycast(_pivotFire.position,_pivotFire.forward,out RaycastHit hit,_rangeFire))
        {
            _damageSender.SendDame(hit.transform);
            Debug.Log($"play fire {hit.transform.name}");
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

    #endregion
}