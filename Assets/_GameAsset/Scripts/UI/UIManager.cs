using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region PROPERTIES

    [SerializeField] private GameObject _uiGameOverPanel;
    [SerializeField] private GameObject _uiGameWinPanel;

    #endregion

    #region UNITY CORE

    private void OnEnable()
    {
        this.RegisterListener(EventID.OnFinishGame,OnFinishGame);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnFinishGame,OnFinishGame);
    }

    #endregion


    #region MAIN

    #region Event

    private void OnFinishGame(object obj)
    {
        if(obj == null) return;
        bool result = (bool)obj;
        if (_uiGameOverPanel)
        {
            _uiGameOverPanel.SetActive(!result);
        }

        if (_uiGameWinPanel)
        {
            _uiGameWinPanel.SetActive(result);
        }
    }

    #endregion

    #endregion

}
