using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadNextScene : MonoBehaviour
{
    [SerializeField] private bool _needStartOnScene;
    [SerializeField] private Slider _enemySettingSlider;
    [SerializeField] private TextMeshProUGUI textEnemyCounr;
    // Start is called before the first frame update
    void Start()
    {
        if (_enemySettingSlider != null)
        {
            _enemySettingSlider.onValueChanged.AddListener(delegate { EnemyPoolValueSetting(); });
            var savedValue = EnemyStaticPoolValue.EnemyPoolValue;
            if (savedValue != 0)
                _enemySettingSlider.value = savedValue;

            var value = _enemySettingSlider.value;
            int vIntValue = (int)value;
            textEnemyCounr.text = vIntValue.ToString();
            EnemyStaticPoolValue.EnemyPoolValue = vIntValue;
        }

        if (_needStartOnScene)
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }

    public void PlayAgen()
    {
        Cursor.lockState = CursorLockMode.Locked;
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

    public void EnemyPoolValueSetting()
    {
        var value= _enemySettingSlider.value;

        int vIntValue = (int)value;
        textEnemyCounr.text = vIntValue.ToString();
        EnemyStaticPoolValue.EnemyPoolValue = vIntValue;
    }

}
