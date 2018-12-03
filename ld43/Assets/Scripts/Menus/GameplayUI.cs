using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour {
    public GameLevelManager LevelManager;
    
    public Button AttackButton;

    public TMP_Text CurrentLevelText;
    public TMP_Text CurrentScoreText;

    private IEnumerator WaitUntilPlayResumes() {
        while (LevelManager.State == GameLevelManager.GameState.Attacking) {
//            Debug.Log(LevelManager.State.ToString());
            yield return null;
        }

        AttackButton.interactable = true;
        Debug.Log("Play has resumed.");
    }

    public void OnAttackClicked() {
        AttackButton.interactable = false;
        
        LevelManager.ChangeState(GameLevelManager.GameState.Attacking);
        StartCoroutine(WaitUntilPlayResumes());
    }
    
    // Update is called once per frame
    void Update() {
        CurrentLevelText.text = $"Level {GameCoordinator.Instance.Level + 1}";
        CurrentScoreText.text = $"Score: {GameCoordinator.Instance.Score:D8}";
    }
}
