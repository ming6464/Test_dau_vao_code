using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private Transform _canonTf;
    
    #endregion

    #region UNITY CORE

    private void Update()
    {
        FollowMouse();
    }

    #endregion


    #region MAIN

    private void FollowMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _canonTf.rotation = Quaternion.LookRotation(mousePosition - _canonTf.position);
        _canonTf.rotation = Quaternion.Euler(0,_canonTf.rotation.eulerAngles.y,0);
    }
    
    #region Event


    #endregion
    
    #endregion
}
