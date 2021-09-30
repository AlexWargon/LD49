using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneService : MonoBehaviour
{
    public Image loadScreen;
    private const float FADE_SPEED = 0.2f;
    private event Action onLoadCall;
    private void Awake()
    {
        if(FindObjectsOfType<SceneService>().Length > 1) Destroy(gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    public void Load(string name, bool fromGameScene = false, Action call = null)
    {
        SceneManager.sceneLoaded += OnLoadScene;
        onLoadCall += call;
        loadScreen.raycastTarget = true;

        TransitionAnimation(() =>
        {
            SceneManager.LoadScene(name);
        });
    }
    
    public void LoadNextLevel()
    {
        SceneManager.sceneLoaded += OnLoadScene;
        loadScreen.raycastTarget = true;
        var sceneName = SceneManager.GetActiveScene().name;
        TransitionAnimation(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    private void TransitionAnimation(Action onEnd)
    {
        loadScreen.DOColor(Color.black, FADE_SPEED).OnComplete(()=> onEnd?.Invoke());
    }

    private void OnLoadScene(Scene scene, LoadSceneMode mode)
    {
        loadScreen.DOColor(Color.clear, FADE_SPEED);
        loadScreen.raycastTarget = false;
        SceneManager.sceneLoaded -= OnLoadScene;
        onLoadCall?.Invoke();
        onLoadCall = null;
    }
}
