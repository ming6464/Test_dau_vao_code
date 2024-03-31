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
    
    
    private List<ItemUIInfo> _itemUIInfos;

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

        Init();
    }

    private void OnEnable()
    {
        this.RegisterListener(EventID.ClickWeaponEquippedUI,OnUnEquippedWeapon);
        this.RegisterListener(EventID.OpenTabInventory,OnOpenTabInventory);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.ClickWeaponEquippedUI,OnUnEquippedWeapon);
        EventDispatcher.Instance.RemoveListener(EventID.OpenTabInventory,OnOpenTabInventory);
    }

    private void Start()
    {
        MachineGunTab_Button_on_click();
    }

    private void Init()
    {
        if (GameConfig.Instance && _itemMachineSpawnCount > 0 && _machineGunTab.ItemPrefab  && _machineGunTab.Content)
        {
            _itemUIInfos = new List<ItemUIInfo>();
            List<InventoryItemData> itemDatas = new List<InventoryItemData>();
            for (int i = 0; i < _itemMachineSpawnCount; i++)
            {
                InventoryItemData itemUIInfo = GameConfig.Instance.GetInventoryItemData(i);
                if(itemUIInfo == null) return;
                itemDatas.Add(itemUIInfo);
            }

            itemDatas = itemDatas.OrderBy(x => -x.Quality).ToList();

            if (_machineGunTab.ItemPrefab.TryGetComponent(out UIItemInventory _))
            {
                for (int i = 0; i < itemDatas.Count; i ++)
                {
                    GameObject itemNew = Instantiate(_machineGunTab.ItemPrefab, _machineGunTab.Content, false);
                    itemNew.name = i.ToString();
                    UIItemInventory uiItemInventory = itemNew.GetComponent<UIItemInventory>();
                    uiItemInventory.Init(itemDatas[i]);
                    if (itemNew.TryGetComponent(out Button button))
                    {
                        button.onClick.AddListener(delegate { ButtonInventoryClick(uiItemInventory.ID,uiItemInventory.InventoryType,!uiItemInventory.IsEquipped); });
                    }
                    itemNew.SetActive(false);
                    _itemUIInfos.Add(new ItemUIInfo{IndexDefault = i,UIItemInventory = uiItemInventory,ItemTf = itemNew.transform});
                }
            }
            
        }
    }
    

    #endregion

    #region MAIN

    #region Event

    private void OnUnEquippedWeapon(object obj)
    {
        if(obj == null) return;
        MessUIItemEquipped mess = (MessUIItemEquipped)obj;
        ButtonInventoryClick(mess.ID,mess.Type,false);
    }

    private void OnOpenTabInventory(object obj)
    {
        if(obj == null) return;
        TabInventoryKey tabKey = (TabInventoryKey)obj;
        InventoryItemType typeInven = InventoryItemType.Pistol;
        if (tabKey != TabInventoryKey.All)
        {
            switch (tabKey)
            {
                case TabInventoryKey.Pistol:
                    typeInven = InventoryItemType.Pistol;
                    break;
                case TabInventoryKey.Rifle:
                    typeInven = InventoryItemType.Rifle;
                    break;
                case TabInventoryKey.Shotgun:
                    typeInven = InventoryItemType.Shotgun;
                    break;
                case TabInventoryKey.Microgun:
                    typeInven = InventoryItemType.Microgun;
                    break;
                case TabInventoryKey.MachineGun:
                    typeInven = InventoryItemType.MachineGun;
                    break;
                
            }
        }

        foreach (ItemUIInfo itemUIInfo in _itemUIInfos)
        {
            bool check = true;
            if (tabKey != TabInventoryKey.All)
            {
                check = itemUIInfo.UIItemInventory.InventoryType == typeInven;
            }

            if (itemUIInfo.ItemTf)
            {
                itemUIInfo.ItemTf.gameObject.SetActive(check);
            }
        }
    }
    
    private void ButtonInventoryClick(int id,InventoryItemType type,bool isEquipped)
    {
        if(_itemUIInfos.Count == 0) return;
        int findIndexItemHandle = _itemUIInfos.FindIndex(x => x.UIItemInventory.ID == id);
        if (findIndexItemHandle < 0) return;
        ItemUIInfo itemUIInfoHandle = _itemUIInfos[findIndexItemHandle];
        int indexItemEquipped = 0;
        if (isEquipped)
        {
            List<ItemUIInfo> listItemEquipped = _itemUIInfos.FindAll(x => x.UIItemInventory.IsEquipped);
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

                    if (itemUIInfoHandle.UIItemInventory.Quality <= itemList.UIItemInventory.Quality)
                    {
                        indexItemEquipped = i;
                    }
                }
                if (findIndexUnEquipped >= 0)
                {
                    ItemUIInfo itemUIInfoUnEquipped = listItemEquipped[findIndexUnEquipped];
                    int indexNew =
                        GetIndexInventoryCalculator(itemUIInfoUnEquipped.IndexDefault);
                    itemUIInfoUnEquipped.ItemTf.SetSiblingIndex(indexNew);

                    if (indexNew > findIndexUnEquipped)
                    {
                        if (indexNew == _itemUIInfos.Count - 1)
                        {
                            _itemUIInfos.Add(itemUIInfoUnEquipped);
                        }
                        else
                        {
                            _itemUIInfos.Insert(indexNew + 1,itemUIInfoUnEquipped);
                        }
                        _itemUIInfos.RemoveAt(findIndexUnEquipped);                          
                    }
                    itemUIInfoUnEquipped.UIItemInventory.OnUnEquipped();
                    if (itemUIInfoUnEquipped.UIItemInventory.Quality >= itemUIInfoHandle.UIItemInventory.Quality)
                    {
                        indexItemEquipped--;
                    }
                }

                indexItemEquipped++;
            }
            itemUIInfoHandle.ItemTf.SetSiblingIndex(indexItemEquipped);
        }
        else
        {
            indexItemEquipped = GetIndexInventoryCalculator(itemUIInfoHandle.IndexDefault);
            itemUIInfoHandle.ItemTf.SetSiblingIndex(indexItemEquipped);
        }

        itemUIInfoHandle.UIItemInventory.HandleOnClick();
        
        _itemUIInfos.Remove(itemUIInfoHandle);
        _itemUIInfos.Insert(indexItemEquipped,itemUIInfoHandle);
        
    }

    private int GetIndexInventoryCalculator(int indexDefault)
    {
        if (_itemUIInfos.Count > 0)
        {
            int startIndex = indexDefault + 5;
            if (startIndex >= _itemUIInfos.Count)
            {
                startIndex = _itemUIInfos.Count - 1;
            }
            for (int i = startIndex; i >= 0; i--)
            {
                if (_itemUIInfos[i].IndexDefault < indexDefault || _itemUIInfos[i].UIItemInventory.IsEquipped)
                {
                    return i;
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
