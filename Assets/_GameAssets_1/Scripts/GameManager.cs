using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

    #region PROPERTIES

    public bool IsFinishGame;
    public string SceneGamePlayName;
    #endregion

    #region UNITY CORE

    private void OnEnable()
    {
        this.RegisterListener(EventID.OnFinishGame,OnFinishGame);
        this.RegisterListener(EventID.ReplayGame,OnReplayGame);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnFinishGame,OnFinishGame);
        EventDispatcher.Instance.RemoveListener(EventID.ReplayGame,OnReplayGame);
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

    #endregion
    
    #endregion

}
