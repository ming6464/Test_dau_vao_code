
using System;
using UnityEngine;
[CreateAssetMenu()]
public class DataGame : ScriptableObject
{
    public InventoryItemData[] InventoryItemDatas;
    public SpriteItemData[] SpriteItemDatas;
    public InventoryItemTypeData[] InventoryItemTypeDatas;
}

[Serializable]
public class InventoryItemData
{
    public int Id;
    public string Name;
    public string Description;
    public string Name_sprite;
    public int Quality;
    public InventoryItemType InventoryItemType;
}



[Serializable]
public class SpriteItemData
{
    public string Name_sprite;
    public Sprite SpriteItem;
}

[Serializable]
public class InventoryItemTypeData
{
    public InventoryItemType InventoryItemType;
    public Color Color;
}

[Serializable]
public enum InventoryItemType
{
    Legend = 3,Epic = 2,Rare = 1,Normal = 0
}