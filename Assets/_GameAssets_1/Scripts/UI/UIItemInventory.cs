using UnityEngine;
using UnityEngine.UI;

public class UIItemInventory : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private Image _bgColorImage;
    [SerializeField] private GameObject _EquipedUIGObj;
    [SerializeField] private Image _inventoryItemImage;
    private Color _bgColor;
    private bool _isEquiped;
    private Sprite _inventoryItemSprite;

    #endregion

    #region UNITY CORE

    public void Init(InventoryItemData data)
    {
        if(data == null) return;
        if(GameConfig.Instance == null) return;
        _bgColor = GameConfig.Instance.GetColorTypeItem(data.InventoryItemType);
        _isEquiped = false;
        _inventoryItemSprite = GameConfig.Instance.GetSpriteItem(data.Name_sprite);
        LoadUI();
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
    
    #endregion
}
