
using System;
using UnityEngine;
using UnityEngine.UI;

public class HealUIEnemy : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private Image _hpImageSlider;
    [SerializeField] private float _timeShowHpImage;
    private Camera _camera;
    private float _timeShowDetail;
    #endregion

    #region UNITY CORE

    private void OnEnable()
    {
        if (_hpImageSlider)
        {
            _hpImageSlider.enabled = false;
        }
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_hpImageSlider != null)
        {
            Transform sliderTf = _hpImageSlider.transform;
            sliderTf.rotation = Quaternion.LookRotation(_camera.transform.position - sliderTf.position);
            sliderTf.rotation = Quaternion.Euler(sliderTf.rotation.eulerAngles.x,sliderTf.rotation.eulerAngles.y,0);
        }

        _timeShowDetail -= Time.deltaTime;
        if (_timeShowDetail < 0)
        {
            if (_hpImageSlider)
            {
                _hpImageSlider.enabled = false;
            }
            
        }
    }

    #endregion

    #region MAIN

    public void UpdateHp(int hp,int maxHp)
    {
        if(_hpImageSlider == null) return;
        _hpImageSlider.fillAmount = hp * 1.0f / maxHp;
        _hpImageSlider.enabled = true;
        _timeShowDetail = _timeShowHpImage;
    }

    #endregion
    
    
}
