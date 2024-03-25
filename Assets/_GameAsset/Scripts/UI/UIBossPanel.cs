using UnityEngine;
using UnityEngine.UI;

public class UIBossPanel : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private Text _hpTextCount;
    [SerializeField] private Image _hpImageSlider;
    [SerializeField] private Text _bulletTextCount;
    [SerializeField] private Image _bulletImageSlider;

    #endregion

    #region UNITY CORE

    private void OnEnable()
    {
        this.RegisterListener(EventID.UpdateBulletCountBoss,OnUpdateBulletCountBoss);
        this.RegisterListener(EventID.UpdateHPBoss,OnUpdateHPBoss);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.UpdateBulletCountBoss,OnUpdateBulletCountBoss);
        EventDispatcher.Instance.RemoveListener(EventID.UpdateHPBoss,OnUpdateHPBoss);
    }

    #endregion


    #region MAIN

    #region Event

    private void OnUpdateHPBoss(object obj)
    {
        if(obj == null) return;
        MessageBossInfo messHp = (MessageBossInfo)obj;
        if (_hpImageSlider)
        {
            _hpImageSlider.fillAmount = messHp.Value1 * 1.0f / messHp.Value2;
        }

        if (_hpTextCount)
        {
            _hpTextCount.text = $"{messHp.Value1}/{messHp.Value2}";
        }
    }

    private void OnUpdateBulletCountBoss(object obj)
    {
        if(obj == null) return;
        MessageBossInfo messBullet = (MessageBossInfo)obj;
        if (_bulletImageSlider)
        {
            _bulletImageSlider.fillAmount = messBullet.Value1 * 1.0f / messBullet.Value2;
        }

        if (_bulletTextCount)
        {
            _bulletTextCount.text = $"{messBullet.Value1}/{messBullet.Value2}";
        }
    }
    #endregion

    
    
    #endregion

}
