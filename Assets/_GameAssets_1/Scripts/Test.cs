using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform Target;

    public Vector3 Angle;

    // Update is called once per frame
    // void Update()
    // {
    //     if (Target)
    //     {
    //         Angle = Quaternion.LookRotation((Target.position - transform.position).normalized).eulerAngles;
    //     }
    // }
    
    
    
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
