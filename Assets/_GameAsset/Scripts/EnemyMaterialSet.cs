using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyMaterialSet : MaterialCustom
{
    #region PROPERTIES
    [SerializeField] private MeshRenderer[] _meshRenderers;
    [SerializeField] private LineRenderer _lineRenderer;
    #endregion

    #region UNITY CORE
    

    #endregion


    #region MAIN

    public void SetMaterial(Material material)
    {
        if(_meshRenderers.Length == 0) return;
        foreach (MeshRenderer mesh in _meshRenderers)
        {
            SetMaterial(mesh, material);
        }

        if (_lineRenderer)
        {
            _lineRenderer.material = material;
        }

        Material = material;
    }
    
    private void SetMaterial(MeshRenderer mesh, Material material)
    {
        mesh.material = material;
    }
    
    #endregion
}
