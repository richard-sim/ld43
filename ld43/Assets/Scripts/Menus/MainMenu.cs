using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    public Slider VolumeSlider;
    
    void Start() {
        VolumeSlider.value = AudioListener.volume;
    }

    public void VolumeChange(float volume) {
        AudioListener.volume = volume;
    }

    public void PlayClicked() {
        LevelLoader.Instance.LoadLevel(GameCoordinator.Instance.LevelName);
    }
}
