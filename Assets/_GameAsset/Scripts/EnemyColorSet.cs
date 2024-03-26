using UnityEngine;

public class EnemyColorSet : ColorCustom
{
    #region PROPERTIES
    [SerializeField] private MeshRenderer[] _meshRenderers;
    [SerializeField] private LineRenderer _lineRenderer;
    #endregion

    #region UNITY CORE
    

    #endregion


    #region MAIN

    public void SetColor(Color color)
    {
        if(_meshRenderers.Length == 0) return;
        foreach (MeshRenderer mesh in _meshRenderers)
        {
            SetColor(mesh, color);
        }

        if (_lineRenderer)
        {
            _lineRenderer.material.color = color;
        }

        Color = color;
    }
    
    private void SetColor(MeshRenderer mesh, Color color)
    {
        if(!mesh) return;
        mesh.material.color = color;
    }
    
    #endregion
}
