using UnityEngine;
using UnityEngine.UI;

public class UIItemInventory : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private Image _bgColorImage;
    [SerializeField] private GameObject _EquipedUIGObj;
    [SerializeField] private Image _inventoryItemImage;
    private Color _bgColor;
    private Sprite _inventoryItemSprite;
    private bool _isInit;
    private InventoryItemData _data;
    private bool _isEquipped;
    private int _id;

    public int Quality => _data.Quality;
    public InventoryItemType InventoryType => _data.InventoryItemType;
    public bool IsEquipped => _isEquipped;
    public int ID => _id;
    
    #endregion

    #region UNITY CORE

    public void Init(InventoryItemData data)
    {
        if(_isInit) return;
        _data = data;
        _isInit = true;
        if(GameConfig.Instance == null) return;
        _bgColor = GameConfig.Instance.GetColorWithQuality(data.Quality);
        _inventoryItemSprite = GameConfig.Instance.GetSpriteItem(data.Name_sprite);
        _isEquipped = false;
        _id = transform.GetInstanceID();
        if (_bgColorImage)
        {
            _bgColorImage.color = _bgColor;
        }

        if (_EquipedUIGObj)
        {
            _EquipedUIGObj.SetActive(false);
        }

        if (_inventoryItemImage)
        {
            _inventoryItemImage.sprite = _inventoryItemSprite;
        }
    }

    #endregion


    #region MAIN

    #region Event


    #endregion
    
    #region Event

    public void HandleOnClick()
    {
        if (!_isEquipped)
        {
            OnEquipped();
            this.PostEvent(EventID.EquippedWeapon , new MessUIItemEquipped{ID = _id,Type = InventoryType,Name_sprite = _data.Name_sprite, Quality = _data.Quality});
        }
        else
        {
            OnUnEquipped();
            this.PostEvent(EventID.UnEquippedWeapon , _data.InventoryItemType);
        }
    }
    
    public void OnEquipped()
    {
        _isEquipped = true;
        if (_EquipedUIGObj)
        {
            _EquipedUIGObj.SetActive(true);
        }
    }

    public void OnUnEquipped()
    {
        _isEquipped = false;
        if (_EquipedUIGObj)
        {
            _EquipedUIGObj.SetActive(false);
        }
    }
    
    
    #endregion
    
    #endregion
}
