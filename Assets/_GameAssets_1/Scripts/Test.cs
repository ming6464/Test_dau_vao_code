using System;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int indexItem;
    public int IndexMove;
    public bool isSet;
    public bool isRemove;
    public List<Transform> Transforms;
    public List<int> lists = new List<int>() {0, 1, 2, 3, 4, 5 };
    public List<int> lists2 = new List<int>() {0, 1, 2, 3, 4, 5 };
    

    private void Update()
    {
        if (isSet)
        {
            Transforms[indexItem].SetSiblingIndex(IndexMove);
            int item = lists[indexItem];
            if (isRemove)
            {
                lists.Remove(item);
                lists.Insert(IndexMove,item);
            }
            else
            {
                lists.Insert(IndexMove,item);
                lists.Remove(item);
            }
            isSet = false;
        }
    }


    #region PROPERTIES

    #endregion

    #region UNITY CORE

    #endregion


    #region MAIN

    #region Event

    public void OnToggleValueChange(string name)
    {
        Debug.Log($"Toggle value change : {name}");
    }

    #endregion
    
    #endregion
    
    
}
