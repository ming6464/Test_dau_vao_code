using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class C_Inventory : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private GameObject _parentItem;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private int _itemSpawnCount;
    #endregion

    #region UNITY CORE

    private void Awake()
    {
        if (_parentItem)
        {
            float width = Screen.width * (1920f / Screen.height) - 40;
            float widthItem = (width - 15 * 4) / 5;
            if (_parentItem.TryGetComponent(out GridLayoutGroup gridLayout))
            {
                gridLayout.cellSize = new Vector2(widthItem,widthItem);
            }
        }
    }

    private void Start()
    {
        if (GameConfig.Instance && _itemPrefab && _parentItem)
        {
            InventoryItemData[] itemDatas = new InventoryItemData[_itemSpawnCount];
            for (int i = 0; i < _itemSpawnCount; i++)
            {
                itemDatas[i] = GameConfig.Instance.GetRandomInventoryItemData();
            }

            
            foreach (InventoryItemData itemData in itemDatas.OrderBy(x => (int)x.InventoryItemType))
            {
                GameObject itemNew = Instantiate(_itemPrefab);
                if (itemNew.TryGetComponent(out UIItemInventory uiItemInventory))
                {
                    uiItemInventory.Init(itemData);
                }
                itemNew.transform.SetParent(_parentItem.transform,false);
            }
        }
    }

    #endregion

    #region MAIN

    #region Event


    #endregion

    #endregion
}
