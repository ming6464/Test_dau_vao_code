using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class C_Inventory : MonoBehaviour
{
    [Serializable]
    public struct TabPanelInfo
    {
        public Transform Content;
        public GameObject ItemPrefab;
        public GameObject TabOn;
        public GameObject TabOff;
    }
    
    [Serializable]
    public struct ItemUIInfo
    {
        public int IndexDefault;
        public UIItemInventory UIItemInventory;
        public Transform ItemTf;
    }
    
    #region PROPERTIES

    [SerializeField] private TabPanelInfo _machineGunTab;
    [SerializeField] private TabPanelInfo _TicketTab;
    [SerializeField] private TabPanelInfo _SupportedTab;
    [SerializeField] private int _itemMachineSpawnCount;
    
    
    private List<ItemUIInfo> ItemUIInfos;

    #endregion

    #region UNITY CORE

    private void Awake()
    {
        if (_machineGunTab.Content)
        {
            float width = Screen.width * (1920f / Screen.height) - 40;
            float widthItem = (width - 15 * 4) / 5;
            if (_machineGunTab.Content.TryGetComponent(out GridLayoutGroup gridLayout))
            {
                gridLayout.cellSize = new Vector2(widthItem,widthItem);
            }
        }
    }

    private void Start()
    {
        Init();
        MachineGunTab_Button_on_click();
    }

    private void Init()
    {
        if (GameConfig.Instance && _itemMachineSpawnCount > 0 && _machineGunTab.ItemPrefab  && _machineGunTab.Content)
        {
            ItemUIInfos = new List<ItemUIInfo>();
            InventoryItemData[] itemDatas = new InventoryItemData[_itemMachineSpawnCount];
            for (int i = 0; i < _itemMachineSpawnCount; i++)
            {
                itemDatas[i] = GameConfig.Instance.GetRandomInventoryItemData();
            }

            itemDatas = itemDatas.OrderBy(x => -x.Quality).ToArray();

            if (_machineGunTab.ItemPrefab.TryGetComponent(out UIItemInventory _))
            {
                for (int i = 0; i < itemDatas.Length; i ++)
                {
                    GameObject itemNew = Instantiate(_machineGunTab.ItemPrefab, _machineGunTab.Content, false);
                    UIItemInventory uiItemInventory = itemNew.GetComponent<UIItemInventory>();
                    uiItemInventory.Init(itemDatas[i]);
                    if (itemNew.TryGetComponent(out Button button))
                    {
                        button.onClick.AddListener(delegate { ButtonInventoryClick(uiItemInventory.ID,uiItemInventory.InventoryType,!uiItemInventory.IsEquipped); });
                    }
                    ItemUIInfos.Add(new ItemUIInfo{IndexDefault = i,UIItemInventory = uiItemInventory,ItemTf = itemNew.transform});
                }
            }
            
        }
    }
    

    #endregion

    #region MAIN

    #region Event

    private void ButtonInventoryClick(int id,InventoryItemType type,bool isEquipped)
    {
        if(ItemUIInfos.Count == 0) return;
        int findIndexItemHandle = ItemUIInfos.FindIndex(x => x.UIItemInventory.ID == id);
        if (findIndexItemHandle < 0) return;
        ItemUIInfo itemUIInfoHandle = ItemUIInfos[findIndexItemHandle];
        
        if (isEquipped)
        {
            List<ItemUIInfo> listItemEquipped = ItemUIInfos.FindAll(x => x.UIItemInventory.IsEquipped);
            int indexItemEquipped = 0;
            if (listItemEquipped.Count > 0)
            {
                indexItemEquipped = -1;
                int findIndexUnEquipped = -1;
                for (int i = 0; i < listItemEquipped.Count; i++)
                {
                    ItemUIInfo itemList = listItemEquipped[i];
                    if(!itemList.UIItemInventory.IsEquipped) break;
                    if (itemList.UIItemInventory.InventoryType == type)
                    {
                        findIndexUnEquipped = i;
                    }

                    if (itemUIInfoHandle.UIItemInventory.Quality < itemList.UIItemInventory.Quality)
                    {
                        indexItemEquipped = i;
                    }
                }
                if (findIndexUnEquipped >= 0)
                {
                    ItemUIInfo itemUIInfoUnEquipped = listItemEquipped[findIndexUnEquipped];
                    itemUIInfoUnEquipped.UIItemInventory.OnUnEquipped();
                    itemUIInfoUnEquipped.ItemTf.SetSiblingIndex(GetIndexInventoryCalculator(itemUIInfoUnEquipped.IndexDefault, listItemEquipped.Count));
                    if (itemUIInfoUnEquipped.UIItemInventory.Quality > itemUIInfoHandle.UIItemInventory.Quality)
                    {
                        indexItemEquipped--;
                    }
                }

                indexItemEquipped++;
            }
            itemUIInfoHandle.ItemTf.SetSiblingIndex(indexItemEquipped);
            itemUIInfoHandle.UIItemInventory.HandleOnClick();
        }
        else
        {
            List<ItemUIInfo> listItemEquipped = ItemUIInfos.FindAll(x => x.UIItemInventory.IsEquipped);
            itemUIInfoHandle.UIItemInventory.HandleOnClick();
            itemUIInfoHandle.ItemTf.SetSiblingIndex(GetIndexInventoryCalculator(itemUIInfoHandle.IndexDefault,listItemEquipped.Count));
        }
        
    }

    private int GetIndexInventoryCalculator(int indexDefault, int weaponEquippedCount)
    {
        if (ItemUIInfos.Count > 0)
        {
            int startIndex = indexDefault + weaponEquippedCount;
            if (startIndex >= ItemUIInfos.Count)
            {
                startIndex = ItemUIInfos.Count - 1;
            }
            
            for (int i = startIndex; i >= 0; i--)
            {
                if (indexDefault > ItemUIInfos[i].IndexDefault)
                {
                    return i + 1;
                }
                if (ItemUIInfos[i].UIItemInventory.IsEquipped)
                {
                    return i + 1;
                }
            } 
        }
        
        return 0;
    }

    #endregion

    public void MachineGunTab_Button_on_click()
    {
        HandleTab(_TicketTab, false);
        HandleTab(_SupportedTab, false);
        HandleTab(_machineGunTab, true);
    }
    public void TicketTab_Button_on_click()
    {
        HandleTab(_TicketTab, true);
        HandleTab(_SupportedTab, false);
        HandleTab(_machineGunTab, false);
    }
    public void SupportedTab_Button_on_click()
    {
        HandleTab(_TicketTab, false);
        HandleTab(_SupportedTab, true);
        HandleTab(_machineGunTab, false);
    }

    private void HandleTab(TabPanelInfo tab,bool isOnTab)
    {
        if(tab.Content) tab.Content.gameObject.SetActive(isOnTab);
        if(tab.TabOn) tab.TabOn.SetActive(isOnTab);
        if(tab.TabOff) tab.TabOff.SetActive(!isOnTab);
    }
    
    #endregion
}
