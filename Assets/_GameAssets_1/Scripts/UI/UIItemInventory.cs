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
    private bool _isEquipped;
    private Sprite _inventoryItemSprite;
    private bool _isInit;
    private InventoryItemType _type;

    #endregion

    #region UNITY CORE

    public void Init(InventoryItemData data)
    {
        if(_isInit) return;
        _isInit = true;
        if(GameConfig.Instance == null) return;
        _bgColor = GameConfig.Instance.GetColorWithQuality(data.Quality);
        _isEquipped = false;
        _inventoryItemSprite = GameConfig.Instance.GetSpriteItem(data.Name_sprite);
        _type = data.InventoryItemType;
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
            _EquipedUIGObj.SetActive(_isEquipped);
        }

        if (_inventoryItemImage)
        {
            _inventoryItemImage.sprite = _inventoryItemSprite;
        }
    }

    #region Event

    private void Button_on_click()
    {
        _isEquipped = !_isEquipped;
        if (_EquipedUIGObj)
        {
            _EquipedUIGObj.SetActive(_isEquipped);
        }

        PostEvent();
    }

    private void LoadRegisterEvent()
    {
        if (_isEquipped)
        {
            // this.RegisterListener();
        }
        else
        {
            
        }
    }

    private void PostEvent()
    {
        this.PostEvent(_isEquipped ? EventID.EquippedWeapon : EventID.UnEquippedWeapon, _type);
    }

    #endregion
    
    #endregion
}
