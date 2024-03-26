using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] protected int _hp;
    [SerializeField] protected int _maxHp;

    #endregion

    #region UNITY CORE

    protected virtual void Awake()
    {
        if (_hp > _maxHp) _hp = _maxHp;
    }

    public virtual void SetHp(int hp, int maxHp)
    {
        if(hp < 0 || maxHp < 0) return;
        _maxHp = maxHp;
        _hp = maxHp >= hp ? hp : maxHp;
        OnHpChange();
    }
    
    #endregion


    #region MAIN

    public virtual void Add(int add)
    {
        if(add < 0) return;
        _hp += add;
        if (_hp > _maxHp) _hp = _maxHp;
        OnHpChange();
    }

    public virtual void Reduct(int reduct)
    {
        if(reduct < 0) return;
        _hp -= reduct;
        if (_hp <= 0)
        {
            _hp = 0;
            OnDead();
        }

        OnHpChange();
    }

    protected virtual void OnDead()
    {
        
    }

    protected virtual bool CheckDead()
    {
        return _hp <= 0;
    }

    protected virtual void OnHpChange()
    {
        
    }

    #endregion
}