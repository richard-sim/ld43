using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    public GameLevelManager LevelManager;
    public PlayerController Player;
    
    void Update() {
        if (LevelManager.State == GameLevelManager.GameState.Playing) {
            if (Input.GetButtonDown("Cancel")) {
                LevelManager.ChangeState(GameLevelManager.GameState.PlayPaused);
            }

//            if (Input.GetButtonDown("Fire3")) {
//                LevelManager.ChangeState(GameLevelManager.GameState.LevelComplete);
//            }

//            if (Input.GetButtonDown("Fire2")) {
//                LevelManager.ChangeState(GameLevelManager.GameState.Won);
//            }

//            if (Input.GetButtonDown("Fire3")) {
//                LevelManager.ChangeState(GameLevelManager.GameState.Lost);
//            }
        } else if (LevelManager.State == GameLevelManager.GameState.Attacking) {
            if (Input.GetButtonDown("Cancel")) {
                Player.CancelAttack();
            }
        }
    }
}
