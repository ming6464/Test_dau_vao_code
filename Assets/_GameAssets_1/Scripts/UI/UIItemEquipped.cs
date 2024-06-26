﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class UIItemEquipped : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private InventoryItemType _ItemType;
    [SerializeField] private Image _bgImage;
    [SerializeField] private Image _iconItemEquiped;

    private int _currentWeaponId;
    private bool _isHasWeapon;
    
    #endregion

    #region UNITY CORE

    private void OnEnable()
    {
        this.RegisterListener(EventID.EquippedWeapon,OnCheckEquippedWeaponUI);
        this.RegisterListener(EventID.UnEquippedWeapon,OnCheckUnEquippedWeaponUI);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.EquippedWeapon,OnCheckEquippedWeaponUI);
        EventDispatcher.Instance.RemoveListener(EventID.UnEquippedWeapon,OnCheckUnEquippedWeaponUI);
    }

    #endregion

    #region MAIN

    #region EVENT

    private void OnCheckEquippedWeaponUI(object obj)
    {
        if(obj == null) return;
        MessUIItemEquipped mess = (MessUIItemEquipped)obj;
        if(mess.Type != _ItemType) return;
        _currentWeaponId = mess.ID;
        _isHasWeapon = true;
        if (GameConfig.Instance)
        {
            if (_bgImage && GameConfig.Instance)
            {
                _bgImage.color = GameConfig.Instance.GetColorWithQuality(mess.Quality);
            }

            if (_iconItemEquiped)
            {
                _iconItemEquiped.sprite = GameConfig.Instance.GetSpriteItem(mess.Name_sprite);
            } 
        }
        
    }
    
    private void OnCheckUnEquippedWeaponUI(object obj)
    {
        if(obj == null) return;
        InventoryItemType type = (InventoryItemType)obj;
        if (type != _ItemType) return;
        if (_bgImage)
        {
            _bgImage.color = Color.white;
        }

        if (_iconItemEquiped)
        {
            _iconItemEquiped.sprite = null;
        }
        _isHasWeapon = false;
    }

    public void WeaponEquippedUIOnClick()
    {
        if(!_isHasWeapon ) return;
        this.PostEvent(EventID.ClickWeaponEquippedUI, new MessUIItemEquipped{ID = _currentWeaponId,Type = _ItemType});
    }

    #endregion

    #endregion
    
}

[Serializable]
public class MessUIItemEquipped
{
    public int ID;
    public InventoryItemType Type;
    public int Quality;
    public string Name_sprite;
}