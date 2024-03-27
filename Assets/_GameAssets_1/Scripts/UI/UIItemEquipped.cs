using System;
using UnityEngine;
using UnityEngine.UI;

public class UIItemEquipped : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private InventoryItemType _ItemType;
    [SerializeField] private Image _bgImage;
    [SerializeField] private Image _iconItemEquiped;
    
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
        InventoryItemData inventoryItemData = (InventoryItemData)obj;
        if(inventoryItemData.InventoryItemType != _ItemType) return;
        if (GameConfig.Instance)
        {
            if (_bgImage && GameConfig.Instance)
            {
                _bgImage.color = GameConfig.Instance.GetColorWithQuality(inventoryItemData.Quality);
            }

            if (_iconItemEquiped)
            {
                _iconItemEquiped.sprite = GameConfig.Instance.GetSpriteItem(inventoryItemData.Name_sprite);
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
    }


    #endregion

    #endregion
    
}

[Serializable]
public class MessUIItemEquippedFeedback
{
    
}