using UnityEngine;

public class GameConfig_ : Singleton<GameConfig_>
{
    #region PROPERTIES
    [SerializeField] private Color[] _colorsTank;
    #endregion

    #region MAIN

    public Color GetColor(int index)
    {
        if (_colorsTank.Length == 0) return new Color();
        if (index >= _colorsTank.Length)
        {
            index = _colorsTank.Length % index;
        }

        return _colorsTank[index];
    }

    #endregion
    
}