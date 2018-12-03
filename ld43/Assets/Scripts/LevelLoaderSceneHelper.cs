using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class LevelLoaderSceneHelper : MonoBehaviour {
    public Slider LoadingBar;
    public TMP_Text LoadingText;
    public AudioSource MusicPlayer;
    public float MaxMusicVolume = 1.0f;

    private CanvasRenderer SceneFader {
        get { return GetComponent<CanvasRenderer>(); }
    }

    public IEnumerator FadeIn(float length) {
//        Debug.Log("*** FADE IN ***");
        yield return StartCoroutine(DoFade(length, 1.0f, 0.0f));
    }

    public IEnumerator FadeOut(float length) {
//        Debug.Log("*** FADE OUT ***");
        yield return StartCoroutine(DoFade(length, 0.0f, 1.0f));
    }

    IEnumerator DoFade(float length, float start, float end) {
        float startTime = Time.time;
        float endTime = startTime + length;

//        Debug.Log("(" + Time.time.ToString("F") + ") Fade: " + start.ToString("F"));
        SceneFader.cull = (start == 0.0f);
        SceneFader.SetAlpha(start);
        MusicPlayer.volume = (1.0f - start) * MaxMusicVolume;
        yield return null;
        
        do {
            float t = (Time.time - startTime) / (endTime - startTime);
            float fade = Mathf.Lerp(start, end, t);
 
//            Debug.Log("(" + Time.time.ToString("F") + ") Fade: " + fade.ToString("F") + " / t: " + t.ToString("F"));
            SceneFader.cull = (fade == 0.0f);
            SceneFader.SetAlpha(fade);
            MusicPlayer.volume = (1.0f - fade) * MaxMusicVolume;
            
            yield return null;
        } while (Time.time < endTime);
        
//        Debug.Log("(" + Time.time.ToString("F") + ") Fade: " + end.ToString("F"));
        SceneFader.cull = (end == 0.0f);
        SceneFader.SetAlpha(end);
        MusicPlayer.volume = (1.0f - end) * MaxMusicVolume;
        
        // One final yield here to ensure the last frame is displayed before moving on
        yield return null;
        
        // Fall-through to exit the coroutine
    }

    public void SetLoadingProgress(float progress) {
//        Debug.Log("(" + Time.time.ToString("F") + ") Progress: " + progress.ToString("P1"));
        if (LoadingBar != null) {
            LoadingBar.normalizedValue = progress;
        }

        if (LoadingText != null) {
            LoadingText.text = progress.ToString("P1");
        }
    }

    private void Awake() {
        SceneFader.cull = false;
        SceneFader.SetAlpha(1.0f);

//        Debug.Log("Awake from " + gameObject.scene.name, this);
        LevelLoader.Instance.SceneHelper = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
//    void Update()
//    {
//        
//    }
}
