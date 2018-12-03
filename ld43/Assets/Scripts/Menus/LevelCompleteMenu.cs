using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompleteMenu : MonoBehaviour
{
    public void OnContinueClicked() {
        GameCoordinator.Instance.LoadNextLevel();
    }
}
