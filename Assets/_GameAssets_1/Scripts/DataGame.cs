
using System;
using UnityEngine;
[CreateAssetMenu()]
public class DataGame : ScriptableObject
{
    public InventoryItemData[] InventoryItemDatas;
    public SpriteItemData[] SpriteItemDatas;
    public QualityData[] QualityDatas;
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
public enum InventoryItemType
{
    Pistol,Rifle,Shotgun,Microgun,MachineGun
}

[Serializable]
public enum TabInventoryKey
{
    All = 0,Pistol = 1,Rifle = 2,Shotgun = 3,Microgun = 4,MachineGun = 5
}

[Serializable]
public class QualityData
{
    public int Quality;
    public Color Color;
}