using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonBottom : MonoBehaviour
{
    [Serializable]
    private struct ToggleButtonInfo
    {
        public RectTransform ToggleRectTf;
        public Toggle Toggle;
    }
    
    #region PROPERTIES

    [SerializeField] private LayoutGroup _layoutGroup;
    
    [SerializeField] private ToggleButtonInfo _toggle0;
    [SerializeField] private ToggleButtonInfo _toggle1;
    [SerializeField] private ToggleButtonInfo _toggle2;
    [SerializeField] private ToggleButtonInfo _toggle3;
    [SerializeField] private ToggleButtonInfo _toggle4;

    [SerializeField] private float _widthRatioToggleOn;
    [SerializeField] private float _widthRatioToggleOff;

    private int CurrentIndexOn = -1;
    
    private float _widthToggleOnCalculated;
    private float _widthToggleOffCalculated;

    #endregion

    #region UNITY CORE

    private void Awake()
    {
        float ratio = 1920f / Screen.height;
        float widthCalculated = Screen.width * ratio;
        float widthPerCell = (widthCalculated - 24) / (_widthRatioToggleOn + _widthRatioToggleOff * 4);
        _widthToggleOnCalculated = widthPerCell * _widthRatioToggleOn;
        _widthToggleOffCalculated = widthPerCell * _widthRatioToggleOff;
    }

    private void Start()
    {
        if (_toggle0.Toggle)
        {
            _toggle0.Toggle.isOn = true;
        }
        ToggleChangeValue(0);
        ToggleChangeValue(1);
        ToggleChangeValue(2);
        ToggleChangeValue(3);
        ToggleChangeValue(4);
    }

    #endregion

    #region MAIN

    #region Event

    public void ToggleChangeValue(int index)
    {
        switch (index)
        {
            case 0:
                ChangeWidthToggle(_toggle0,index);
                break;
            case 1:
                ChangeWidthToggle(_toggle1,index);
                break;
            case 2:
                ChangeWidthToggle(_toggle2,index);
                break;
            case 3:
                ChangeWidthToggle(_toggle3,index);
                break;
            case 4:
                ChangeWidthToggle(_toggle4,index);
                break;
        }
    }

    private void ChangeWidthToggle(ToggleButtonInfo buttonInfo,int index = -1)
    {
        if(!buttonInfo.ToggleRectTf || !buttonInfo.Toggle) return;
        if(buttonInfo.Toggle.isOn && CurrentIndexOn == index) return;
        buttonInfo.ToggleRectTf.sizeDelta =
            new Vector2(buttonInfo.Toggle.isOn ? _widthToggleOnCalculated : _widthToggleOffCalculated,
                buttonInfo.ToggleRectTf.sizeDelta.y);
        _layoutGroup.CalculateLayoutInputHorizontal();
        _layoutGroup.SetLayoutHorizontal();
        if (index >= 0 && buttonInfo.Toggle.isOn) CurrentIndexOn = index;
    }

    #endregion

    #endregion
}
