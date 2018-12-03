using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroMenu : MonoBehaviour {
    public GameLevelManager LevelManager;
    
    public void OnGoClicked() {
        LevelManager.ChangeState(GameLevelManager.GameState.Playing);
    }
}
