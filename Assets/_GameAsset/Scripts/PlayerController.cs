using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private Transform _canonTf;
    [SerializeField] private LayerMask _maskMap;
    [SerializeField] private PlayerCanonController _canonPlayer;
    #endregion

    #region UNITY CORE

    private void Update()
    {
        FollowMouse();
        HandleFire();
    }

    #endregion


    #region MAIN

    private void FollowMouse()
    {
        if(_canonTf == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _maskMap))
        {
            _canonTf.rotation = Quaternion.LookRotation(hit.point - _canonTf.position);
            _canonTf.rotation = Quaternion.Euler(0,_canonTf.rotation.eulerAngles.y,0);
        }

    }

    private void HandleFire()
    {
        if(_canonPlayer == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            _canonPlayer.HandleFire();
        }
    }
    
    #region Event


    #endregion
    
    #endregion
}
