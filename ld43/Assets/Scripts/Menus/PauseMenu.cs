using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameLevelManager LevelManager;
    
    public void OnResumeClicked() {
        LevelManager.ChangeState(GameLevelManager.GameState.Playing);
    }
    
    public void OnMainMenuClicked() {
        LevelLoader.Instance.LoadLevel("StartMenu",
            () => {
                GameCoordinator.Instance.ResetGame();
            });
    }
}
