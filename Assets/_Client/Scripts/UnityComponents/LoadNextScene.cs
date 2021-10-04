using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour
{
    [SerializeField] private bool _needStartOnScene;
    // Start is called before the first frame update
    void Start()
    {
        if (_needStartOnScene)
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }

    public void PlayAgen()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
