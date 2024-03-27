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

    public Color GetColorWithQuality(int quality)
    {
        if (!_dataGame || _dataGame.QualityDatas.Length == 0) return new Color();
        int indexFind = Array.FindIndex(_dataGame.QualityDatas, x => x.Quality == quality);
        if(indexFind < 0) return new Color();
        return _dataGame.QualityDatas[indexFind].Color;
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
