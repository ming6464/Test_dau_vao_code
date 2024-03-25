
using UnityEngine;

public class UIFinishGamePanel : MonoBehaviour
{
    #region PROPERTIES

    #endregion

    #region UNITY CORE

    #endregion


    #region MAIN

    #region Event
    
    public void ReplayButtonOnClick()
    {
        this.PostEvent(EventID.ReplayGame);
    }

    #endregion
    
    #endregion
}
