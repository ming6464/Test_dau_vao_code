using System;
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
    #region PROPERTIES

    [SerializeField] private TabPanelInfo _machineGunTab;
    [SerializeField] private TabPanelInfo _TicketTab;
    [SerializeField] private TabPanelInfo _SupportedTab;
    [SerializeField] private int _itemMachineSpawnCount;
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
        if (GameConfig.Instance && _machineGunTab.ItemPrefab  && _machineGunTab.Content)
        {
            InventoryItemData[] itemDatas = new InventoryItemData[_itemMachineSpawnCount];
            for (int i = 0; i < _itemMachineSpawnCount; i++)
            {
                itemDatas[i] = GameConfig.Instance.GetRandomInventoryItemData();
            }

            foreach (InventoryItemData itemData in itemDatas.OrderBy(x => (int)x.InventoryItemType))
            {
                GameObject itemNew = Instantiate(_machineGunTab.ItemPrefab, _machineGunTab.Content, false);
                if (itemNew.TryGetComponent(out UIItemInventory uiItemInventory))
                {
                    uiItemInventory.Init(itemData);
                }
            }
        }

        MachineGunTab_Button_on_click();
    }

    #endregion

    #region MAIN

    #region Event


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
