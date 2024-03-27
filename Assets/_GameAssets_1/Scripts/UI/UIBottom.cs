using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBottom : MonoBehaviour
{
    [Serializable]
    private struct TabUIBottomInfo
    {
        public GameObject Tab;
        public Toggle Toggle;
        public LayoutElement LayoutElement;
        public GameObject NameText;
    }
    #region PROPERTIES
    [SerializeField] private TabUIBottomInfo _gunTabInfo;
    [SerializeField] private TabUIBottomInfo _inventoryTabInfo;
    [SerializeField] private TabUIBottomInfo _chestTabInfo;
    [SerializeField] private TabUIBottomInfo _mapTabInfo;
    [SerializeField] private TabUIBottomInfo _shopTabInfo;

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
        if (_inventoryTabInfo.Toggle)
        {
            _inventoryTabInfo.Toggle.isOn = true;
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
                ChangeWidthToggle(_gunTabInfo,index);
                break;
            case 1:
                ChangeWidthToggle(_inventoryTabInfo,index);
                break;
            case 2:
                ChangeWidthToggle(_chestTabInfo,index);
                break;
            case 3:
                ChangeWidthToggle(_mapTabInfo,index);
                break;
            case 4:
                ChangeWidthToggle(_shopTabInfo,index);
                break;
        }
    }

    private void ChangeWidthToggle(TabUIBottomInfo tabInfo,int index = -1)
    {
        if(!tabInfo.LayoutElement || !tabInfo.Toggle) return;
        if(tabInfo.Toggle.isOn && CurrentIndexOn == index) return;
        tabInfo.LayoutElement.minWidth = _widthToggleOffCalculated;
        tabInfo.LayoutElement.preferredWidth = tabInfo.Toggle.isOn ? _widthToggleOnCalculated : _widthToggleOffCalculated;
        if (tabInfo.NameText)
        {
            tabInfo.NameText.SetActive(tabInfo.Toggle.isOn);
        }

        if (tabInfo.Tab)
        {
            tabInfo.Tab.SetActive(tabInfo.Toggle.isOn);
        }
        if (index >= 0 && tabInfo.Toggle.isOn) CurrentIndexOn = index;
    }

    #endregion

    #endregion
}
