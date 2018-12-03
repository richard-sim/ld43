using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWinMenu : MonoBehaviour
{
    public void OnMainMenuClicked() {
        LevelLoader.Instance.LoadLevel("StartMenu",
            () => {
                GameCoordinator.Instance.ResetGame();
            });
    }
}
