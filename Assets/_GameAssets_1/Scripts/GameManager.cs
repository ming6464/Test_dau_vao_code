using System;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

    #region PROPERTIES

    public bool IsFinishGame;
    public string SceneGamePlayName;
    public int Point;
    #endregion

    #region UNITY CORE

    private void OnEnable()
    {
        this.RegisterListener(EventID.FinishGame,OnFinishGame);
        this.RegisterListener(EventID.ReplayGame,OnReplayGame);
        this.RegisterListener(EventID.EnemyDead,OnAddPoint);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.FinishGame,OnFinishGame);
        EventDispatcher.Instance.RemoveListener(EventID.ReplayGame,OnReplayGame);
        EventDispatcher.Instance.RemoveListener(EventID.EnemyDead,OnAddPoint);
    }

    private void Start()
    {
        this.PostEvent(EventID.UpdatePoint,Point);
    }

    #endregion


    #region MAIN

    #region Event

    private void OnFinishGame(object obj)
    {
        IsFinishGame = true;
    }
    private void OnReplayGame(object obj)
    {
        SceneManager.LoadScene(SceneGamePlayName);
        IsFinishGame = false;
    }

    private void OnAddPoint(object obj)
    {
        Point++;
        this.PostEvent(EventID.UpdatePoint,Point);
    }

    #endregion
    
    #endregion

}
