using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wargon.ezs;

public class UIController : MonoBehaviour
{
    [SerializeField] private Image _newGameButton;

    [SerializeField] private GameObject _gameOver;

    private Entity playerEntity;

    private GameService gameService;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
    }
    // Update is called once per frame
    void Update()
    { 
        gameService = Servise<GameService>.Get();
        if (gameService != null)
        {
            playerEntity = Servise<GameService>.Get().PlayerEntity;
            if (playerEntity != null)
            {
                float hp = playerEntity.Get<Health>().Value;
                _newGameButton.fillAmount = hp / 1000;
                if (hp <= 2)
                {
                    GameOver();
                }
            }
        }
        
    }

    public void GameOver()
    {
        _gameOver.SetActive(true);
    }
}
