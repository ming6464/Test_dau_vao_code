using System;
using UnityEngine;
using UnityEngine.UI;

public class UITopPanel : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private Image _hpImageSlider;
    [SerializeField] private Text _hpText;
    [SerializeField] private Text _point;

    #endregion

    #region UNITY CORE

    private void OnEnable()
    {
        this.RegisterListener(EventID.UpdateHPPlayer,OnUpdateHpUI);
        this.RegisterListener(EventID.UpdatePoint,OnUpdatePoint);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.UpdateHPPlayer,OnUpdateHpUI);
        EventDispatcher.Instance.RemoveListener(EventID.UpdatePoint,OnUpdatePoint);
    }

    #endregion


    #region MAIN

    #region Event

    private void OnUpdateHpUI(object obj)
    {
        if(obj == null) return;
        MessageHpPlayer mess = (MessageHpPlayer)obj;
        if (_hpImageSlider != null)
        {
            _hpImageSlider.fillAmount = mess.Hp / mess.MaxHp;
        }

        if (_hpText != null)
        {
            _hpText.text = mess.Hp.ToString();
        }
    }

    private void OnUpdatePoint(object obj)
    {
        if(obj == null ) return;
        if (_point != null)
        {
            _point.text = ((int)obj).ToString();
        }
    }

    #endregion
    
    #endregion

}

[Serializable]
public class MessageHpPlayer
{
    public float Hp;
    public float MaxHp;
}
