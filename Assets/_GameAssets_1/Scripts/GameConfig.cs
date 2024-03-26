using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameConfig : Singleton<GameConfig>
{
    [SerializeField] private DataGame _dataGame;

    public Sprite GetSpriteItem(string sprite_name)
    {
        if (!_dataGame || _dataGame.SpriteItemDatas == null) return null;
        return Array.Find(_dataGame.SpriteItemDatas,x => x.Name_sprite == sprite_name).SpriteItem;
    }

    public Color GetColorTypeItem(InventoryItemType type)
    {
        if (!_dataGame || _dataGame.InventoryItemTypeDatas == null) return new Color();
        return Array.Find(_dataGame.InventoryItemTypeDatas, x => x.InventoryItemType == type).Color;
    }

    public InventoryItemData GetRandomInventoryItemData()
    {
        if (_dataGame)
        {
            if (_dataGame.InventoryItemDatas.Length > 0)
            {
                int randomIndex = Random.Range(0, _dataGame.InventoryItemDatas.Length);
                return _dataGame.InventoryItemDatas[randomIndex];
            }
        }

        return null;
    }
    
}
