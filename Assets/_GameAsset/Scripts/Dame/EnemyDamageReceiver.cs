using System;
using UnityEngine;

public class EnemyDamageReceiver : DamageReceiver
{
    #region PROPERTIES

    [SerializeField] private HealUIEnemy _healUI;

    private MaterialCustom _materialCustom;
    
    #endregion

    #region UNITY CORE

    private void Start()
    {
        TryGetComponent(out _materialCustom);
    }

    #endregion


    #region MAIN

    protected override void OnDead()
    {
        base.OnDead();
        this.PostEvent(EventID.EnemyDead);
        if (VFX_manager.Instance && _materialCustom && _materialCustom.Material)
        {
            VFX_manager.Instance.PlayEffect(transform.position,_materialCustom.Material,VFXKEY.EnemyDead);
        }
        if (GameObjectPooling.Instance)
        {
            GameObjectPooling.Instance.Push(PoolKEY.Enemy, transform);
        }
    }

    public void Revival()
    {
        _hp = _maxHp;
    }

    protected override void OnHpChange()
    {
        base.OnHpChange();
        if(_healUI) _healUI.UpdateHp(_hp,_maxHp);
    }

    #endregion

}