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
        Debug.Log($"id : {id} / type : {type} / isEquipped : {isEquipped}");
        if(ItemUIInfos.Count == 0) return;
        int findIndexItemHandle = ItemUIInfos.FindIndex(x => x.UIItemInventory.ID == id);
        if (findIndexItemHandle < 0) return;
        ItemUIInfo itemUIInfoHandle = ItemUIInfos[findIndexItemHandle];
        itemUIInfoHandle.UIItemInventory.HandleOnClick();
        if (isEquipped)
        {
            List<ItemUIInfo> listItemEquipped = ItemUIInfos.FindAll(x => x.UIItemInventory.IsEquipped);
            int findIndexUnEquipped = listItemEquipped.FindIndex(x => x.UIItemInventory.InventoryType == type);
            if (findIndexUnEquipped >= 0)
            {
                ItemUIInfo itemUIInfoUnEquipped = listItemEquipped[findIndexUnEquipped];
                itemUIInfoUnEquipped.UIItemInventory.OnUnEquipped();
                // itemUIInfoUnEquipped.ItemTf.set
                // if (_machineGunTab.Content)
                // {
                //     _machineGunTab.Content.set
                // }
            }
        }
        else
        {
            
        }
    }

    private int GetIndexInventoryCalculator(int indexDefault)
    {
        if (ItemUIInfos.Count > 0)
        {
            for (int i = ItemUIInfos.Count - 1; i >= 0; i--)
            {
                if (indexDefault > ItemUIInfos[i].IndexDefault) return i;
            } 
        }
        
        return - 1;
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
