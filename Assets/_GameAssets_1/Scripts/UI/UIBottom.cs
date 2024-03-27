using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBottom : MonoBehaviour
{
    [Serializable]
    private struct TabUIBottomInfo
    {
        public Toggle Toggle;
        public LayoutElement LayoutElement;
        public GameObject NameText;
        public TabInventoryKey Key;
    }
    #region PROPERTIES
    [SerializeField] private TabUIBottomInfo[] _tabInfos;

    [SerializeField] private float _widthRatioToggleOn;
    [SerializeField] private float _widthRatioToggleOff;

    private int _currentIndexOn = -1;
    
    private float _widthToggleOnCalculated;
    private float _widthToggleOffCalculated;

    #endregion

    #region UNITY CORE

    private void Awake()
    {
        float ratio = 1920f / Screen.height;
        float widthCalculated = Screen.width * ratio;
        float widthPerCell = (widthCalculated - 24) / (_widthRatioToggleOn + _widthRatioToggleOff * 5);
        _widthToggleOnCalculated = widthPerCell * _widthRatioToggleOn;
        _widthToggleOffCalculated = widthPerCell * _widthRatioToggleOff;
    }

    private void Start()
    {
        int indexTabAll = Array.FindIndex(_tabInfos, x => x.Key == TabInventoryKey.All);
        if (indexTabAll >= 0 && _tabInfos[indexTabAll].Toggle)
        {
            _tabInfos[indexTabAll].Toggle.isOn = true;
        }
        ToggleChangeValue(TabInventoryKey.All);
        ToggleChangeValue(TabInventoryKey.Microgun);
        ToggleChangeValue(TabInventoryKey.Pistol);
        ToggleChangeValue(TabInventoryKey.Rifle);
        ToggleChangeValue(TabInventoryKey.Shotgun);
        ToggleChangeValue(TabInventoryKey.MachineGun);
    }

    #endregion

    #region MAIN

    #region Event

    public void ToggleChangeValue(int index)
    {
        ToggleChangeValue((TabInventoryKey)index);
    }

    public void ToggleChangeValue(TabInventoryKey key)
    {
        int tabIndex = Array.FindIndex(_tabInfos, x => x.Key == key);
        if(tabIndex < 0) return;
        TabUIBottomInfo tabInfo = _tabInfos[tabIndex];
        if(!tabInfo.LayoutElement || !tabInfo.Toggle) return;
        int index = (int)tabInfo.Key;
        if(tabInfo.Toggle.isOn && _currentIndexOn == index) return;
        tabInfo.LayoutElement.minWidth = _widthToggleOffCalculated;
        tabInfo.LayoutElement.preferredWidth = tabInfo.Toggle.isOn ? _widthToggleOnCalculated : _widthToggleOffCalculated;
        if (tabInfo.NameText)
        {
            tabInfo.NameText.SetActive(tabInfo.Toggle.isOn);
        }

        if (tabInfo.Toggle.isOn)
        {
            _currentIndexOn = index;
            this.PostEvent(EventID.OpenTabInventory,key);
        }
    }

    #endregion

    #endregion
}

