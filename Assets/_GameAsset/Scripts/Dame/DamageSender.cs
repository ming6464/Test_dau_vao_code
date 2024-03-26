using UnityEngine;

public class DamageSender : MonoBehaviour
{
    
    #region PROPERTIES
    [SerializeField] private int _minDamage;
    [SerializeField] protected int _maxdamage;
    #endregion

    #region UNITY CORE
    
    
    #endregion


    #region MAIN
    
    public virtual void SendDame(Transform objHit)
    {
        if(objHit == null) return;
        if (objHit.TryGetComponent(out DamageReceiver damageReceiver))
        {
            SendDame(damageReceiver);
        }
    }

    public virtual void SendDame(DamageReceiver damageReceiver)
    {
        if(damageReceiver == null) return;
        int damageRandom = Random.Range(_minDamage, _maxdamage + 1);
        damageReceiver.Reduct(damageRandom);
    }
    
    #endregion
    
    
}