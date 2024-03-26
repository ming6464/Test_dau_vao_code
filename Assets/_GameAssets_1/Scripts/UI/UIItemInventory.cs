using System;
using UnityEngine;
using UnityEngine.UI;

public class UIItemInventory : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private Image _bgColorImage;
    [SerializeField] private GameObject _EquipedUIGObj;
    [SerializeField] private Image _inventoryItemImage;
    [SerializeField] private Button _button;
    private Color _bgColor;
    private bool _isEquiped;
    private Sprite _inventoryItemSprite;
    private bool _isInit;

    #endregion

    #region UNITY CORE

    public void Init(InventoryItemData data)
    {
        if(_isInit || data == null) return;
        _isInit = true;
        if(GameConfig.Instance == null) return;
        _bgColor = GameConfig.Instance.GetColorTypeItem(data.InventoryItemType);
        _isEquiped = false;
        _inventoryItemSprite = GameConfig.Instance.GetSpriteItem(data.Name_sprite);
        LoadUI();
        if (_button)
        {
            _button.onClick.AddListener(Button_on_click);
        }
    }

    private void OnEnable()
    {
        if (_isInit)
        {
            if (_button)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(Button_on_click);
            }
        }
    }

    private void OnDisable()
    {
        if (_button)
        {
            _button.onClick.RemoveAllListeners();
        }
    }

    #endregion


    #region MAIN

    #region Event


    #endregion

    private void LoadUI()
    {
        if (_bgColorImage)
        {
            _bgColorImage.color = _bgColor;
        }

        if (_EquipedUIGObj)
        {
            _EquipedUIGObj.SetActive(_isEquiped);
        }

        if (_inventoryItemImage)
        {
            _inventoryItemImage.sprite = _inventoryItemSprite;
        }
    }

    #region Event

    private void Button_on_click()
    {
        _isEquiped = !_isEquiped;
        if (_EquipedUIGObj)
        {
            _EquipedUIGObj.SetActive(_isEquiped);
        }
    }

    #endregion
    
    #endregion
}
