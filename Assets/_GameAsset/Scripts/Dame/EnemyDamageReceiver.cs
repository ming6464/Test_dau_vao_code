using System;
using UnityEngine;

public class EnemyDamageReceiver : DamageReceiver
{
    #region PROPERTIES

    [SerializeField] private HealUIEnemy _healUI;

    private ColorCustom _colorCustom;
    
    #endregion

    #region UNITY CORE

    private void Start()
    {
        TryGetComponent(out _colorCustom);
    }

    #endregion


    #region MAIN

    [Obsolete("Obsolete")]
    protected override void OnDead()
    {
        base.OnDead();
        this.PostEvent(EventID.EnemyDead);
        if (VFX_manager.Instance && _colorCustom)
        {
            VFX_manager.Instance.PlayEffect(transform.position,_colorCustom.Color,VFXKEY.EnemyDead);
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