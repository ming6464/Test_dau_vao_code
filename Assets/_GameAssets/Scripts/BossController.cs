using System;
using UnityEngine;

public class BossController : MonoBehaviour
{
    #region PROPERTIES
    [SerializeField] private float _timeResetFire;
    [SerializeField] private int _maxHP; 
    [SerializeField] private int _bulletCount;
    private Transform _player;
    private int _currentHP;
    private int _currentBulletCount;
    #endregion

    #region UNITY CORE

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        Invoke(nameof(DelayFire),_timeResetFire);
        _currentBulletCount = _bulletCount;
        _currentHP = _maxHP;
        this.PostEvent(EventID.UpdateBulletCountBoss,new MessageBossInfo{Value1 = _currentBulletCount, Value2 = _bulletCount});
        this.PostEvent(EventID.UpdateHPBoss,new MessageBossInfo{Value1 = _currentHP, Value2 = _maxHP});
    }

    private void OnEnable()
    {
        this.RegisterListener(EventID.PlayerFire,OnHit);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.PlayerFire,OnHit);
    }

    #endregion

    #region MAIN

    private void DelayFire()
    {
        if(!_player || (GameManager.Instance && GameManager.Instance.IsFinishGame)) return;
        _currentBulletCount--;
        transform.LookAt(_player);
        transform.rotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y,0);
        
        Debug.DrawLine(transform.position,_player.position,Color.black,_timeResetFire);

        
        if (_currentBulletCount < 0)
        {
            _currentBulletCount = 0;
        }
        
        this.PostEvent(EventID.UpdateBulletCountBoss,new MessageBossInfo{Value1 = _currentBulletCount, Value2 = _bulletCount});


        if (_currentBulletCount == 0)
        {
            this.PostEvent(EventID.OnFinishGame,false);
            return;
        }
        
        Invoke(nameof(DelayFire),_timeResetFire);
    }

    #region Event

    private void OnHit(object obj)
    {
        _currentHP--;
        if (_currentHP <= 0)
        {
            _currentHP = 0;
        }
        this.PostEvent(EventID.UpdateHPBoss,new MessageBossInfo{Value1 = _currentHP, Value2 = _maxHP});
    }

    #endregion

    #endregion
}

public class MessageBossInfo
{
    public int Value1;
    public int Value2;
}
