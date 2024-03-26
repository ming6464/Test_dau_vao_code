
using System;
using UnityEngine;

public class CanonController_2 : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] protected Transform _pivotFire;
    [SerializeField] protected LineRenderer _lineBullet;
    [SerializeField] protected float _rangeFire;
    [SerializeField] protected float _timeResetLine;
    protected float _timeResetLineDetail;
    
    #endregion

    #region UNITY CORE

    protected virtual void Update()
    {
        if (_timeResetLineDetail > 0 && _lineBullet)
        {
            _timeResetLineDetail -= Time.deltaTime;
            if (_timeResetLineDetail < 0)
            {
                _lineBullet.positionCount = 0;
            }
        }
    }

    #endregion


    #region MAIN

    public virtual void HandleFire()
    {
        
    }
    
    protected virtual void OnFire()
    {
        
    }

    protected virtual void PlayEffect(Vector3 position, Vector3 direction, Transform target)
    {
        if(!VFX_manager.Instance || !target) return;
        if (target.TryGetComponent(out ColorCustom colorCustom))
        {
            VFX_manager.Instance.PlayEffect(position, direction,colorCustom.Color,VFXKEY.HitEffect);
        }
        
    }

    #endregion
}
