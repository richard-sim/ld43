using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

public class LevelLoader : MonoBehaviour {
    public static LevelLoader Instance;

    public float FadeOutTime = 1.0f;
    public float FadeInTime = 1.0f;
    
    [NonSerialized]
    public LevelLoaderSceneHelper SceneHelper;

    private String _nextLevelToLoad = null;
    private Action _onFadeOutComplete = null;

    private void Awake() {
        if (Instance != null) {
            FadeOutTime = Instance.FadeOutTime;
            FadeInTime = Instance.FadeInTime;
            SceneHelper = Instance.SceneHelper;
            _nextLevelToLoad = Instance._nextLevelToLoad;
            _onFadeOutComplete = Instance._onFadeOutComplete;
            
            Destroy(Instance.gameObject);
            Instance = null;
        }
        
        DontDestroyOnLoad(this.gameObject);

        Instance = this;
    }

    void Start() {
        StartCoroutine(SceneHelper.FadeIn(FadeInTime));
    }

    void Update() {
        if (!String.IsNullOrEmpty(_nextLevelToLoad)) {
            StartCoroutine(DoLoadLevel(_nextLevelToLoad, _onFadeOutComplete));
            _nextLevelToLoad = null;
            _onFadeOutComplete = null;
        }
    }

    public void LoadLevel(String levelName, Action onFadeOutComplete = null) {
        _nextLevelToLoad = levelName;
    }

    // StartCoroutine(LevelLoader.Instance.LoadLevel("level-01"))
    IEnumerator DoLoadLevel(String levelName, Action onFadeOutComplete) {
        // Start fade-out animation
        yield return StartCoroutine(SceneHelper.FadeOut(FadeOutTime));
        onFadeOutComplete?.Invoke();

//        yield return new WaitForSeconds(1.0f);
//        Debug.Log("*** Loading LoadingScreen ***");
        
        // Load the loading scene
        AsyncOperation loadingOp = SceneManager.LoadSceneAsync("LoadingScreen");
        do {
            yield return null;
        } while (!loadingOp.isDone);

//        Debug.Log("*** Done loading LoadingScreen ***");

        // Start fade-in animation
        yield return StartCoroutine(SceneHelper.FadeIn(FadeInTime));

        // Load the actual scene
//        Debug.Log("*** Loading " + levelName + " ***");
        loadingOp = SceneManager.LoadSceneAsync(levelName);
        loadingOp.allowSceneActivation = false;
        do {
            float progress = Mathf.Clamp01(loadingOp.progress / 0.9f);
            SceneHelper.SetLoadingProgress(progress);

            yield return null;
        } while (loadingOp.progress < 0.9f);

        // Update to show 100% before continuing
        SceneHelper.SetLoadingProgress(1.0f);
        yield return null;

//        Debug.Log("*** Done loading " + levelName + " ***");

        // Start fade-out animation
        yield return StartCoroutine(SceneHelper.FadeOut(FadeOutTime));

//        Debug.Log("*** Activating level ***");

        loadingOp.allowSceneActivation = true;
        do {
            yield return null;
        } while (!loadingOp.isDone);

//        Debug.Log("*** Level activated ***");

        // Start fade-in animation
        yield return StartCoroutine(SceneHelper.FadeIn(FadeInTime));
    }
}
