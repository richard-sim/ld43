using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    public static GameCoordinator Instance;
    
    private int _level = 0;
    private int _score = 0;

    public int Level {
        get { return _level; }
    }

    public string LevelName {
        get { return $"level-{_level:D2}"; }
    }

    public int Score {
        get { return _score; }
    }

    public void ResetGame() {
        _level = 0;
        _score = 0;
    }

    public void LoadNextLevel() {
        _level = _level + 1;
        
        LevelLoader.Instance.LoadLevel(LevelName);
    }

    public void LoadLevel(int level) {
        _level = level;
        
        LevelLoader.Instance.LoadLevel(LevelName);
    }

    private void Awake() {
        if (Instance != null) {
            _level = Instance._level;
            _score = Instance._score;
            
            Destroy(Instance.gameObject);
            Instance = null;
        }
        
        DontDestroyOnLoad(this.gameObject);

        Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        ResetGame();
    }

    // Update is called once per frame
    void Update() {
    }
}
