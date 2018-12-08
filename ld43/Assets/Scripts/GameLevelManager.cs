using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLevelManager : MonoBehaviour {
    public GameObject IntroUI;
    
    public GameObject GameplayUI;

    public GameObject LevelCompleteUI;

    public GameObject GameWinUI;

    public GameObject GameLostUI;

    public GameObject PauseMenu;
    
    public enum GameState {
        Intro,
        Playing,
        SelectingForAttack,
        Attacking,
        PlayPaused,
        LevelComplete,
        Won,
        Lost
    }

    private GameState _state = GameState.Intro;

    public GameState State {
        get { return _state; }
    }

    public void ChangeState(GameState newState) {
        Debug.Log($"ChangeState({_state} -> {newState})");

        switch (_state) {
            case GameState.Intro:
                Debug.Assert(newState == GameState.Playing);
                
                IntroUI.SetActive(false);
                GameplayUI.SetActive(true);
                
                break;
            
            case GameState.Playing:
                if (newState == GameState.SelectingForAttack) {
                    // Nothing to do
                } else if (newState == GameState.PlayPaused) {
                    PauseMenu.SetActive(true);
                } else if (newState == GameState.LevelComplete) {
                    LevelCompleteUI.SetActive(true);
                } else if (newState == GameState.Won) {
                    GameWinUI.SetActive(true);
                } else if (newState == GameState.Lost) {
                    GameLostUI.SetActive(true);
                }
                else {
                    Debug.LogError("Unhandled state: " + newState);
                }

                break;
            
            case GameState.SelectingForAttack:
                Debug.Assert((newState == GameState.Attacking) || (newState == GameState.Playing));

                // Nothing to do

                break;
            
            case GameState.Attacking:
                Debug.Assert(newState == GameState.Playing);

                // Nothing to do

                break;
            
            case GameState.PlayPaused:
                Debug.Assert(newState == GameState.Playing);

                PauseMenu.SetActive(false);

                break;
            
            case GameState.LevelComplete:
//                Debug.Break();
                Debug.Log("Level complete!");
                break;
            
            case GameState.Won:
//                Debug.Break();
                Debug.Log("Game won!");
                break;
            
            case GameState.Lost:
//                Debug.Break();
                Debug.Log("Game lost!");
                break;
        }

        _state = newState;
    }

    // Awake is called when the script instance is being loaded
    private void Awake() {
        IntroUI.SetActive(true);
        GameplayUI.SetActive(false);
        LevelCompleteUI.SetActive(false);
        GameWinUI.SetActive(false);
        GameLostUI.SetActive(false);
        PauseMenu.SetActive(false);
    }

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }
}
