
public class PlayerDamageReceiver : DamageReceiver
{
    #region UNITY CORE

    private void Start()
    {
        this.PostEvent(EventID.UpdateHPPlayer,new MessageHpPlayer{Hp = _hp,MaxHp = _maxHp});
    }

    #endregion
    
    #region MAIN

    public override void Reduct(int reduct)
    {
        base.Reduct(reduct);
        this.PostEvent(EventID.UpdateHPPlayer,new MessageHpPlayer{Hp = _hp,MaxHp = _maxHp});
    }

    protected override void OnDead()
    {
        base.OnDead();
        this.PostEvent(EventID.FinishGame,false);
    }

    #endregion
}