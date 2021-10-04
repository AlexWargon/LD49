using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wargon.ezs;

public class UIController : MonoBehaviour
{
    [SerializeField] private Image _newGameButton;

    [SerializeField] private GameObject _gameOver;
    [SerializeField] private TextMeshProUGUI KillsCount;
    private Animator comboAnimator;
    [SerializeField] private TextMeshCool Combo;

    private Entity playerEntity;

    private GameService gameService;
    private bool inited;
    IEnumerator Start()
    {
        Time.timeScale = 1;
        comboAnimator = Combo.GetComponent<Animator>();
        KillsCount.text = 0.ToString();
        Servise<UIController>.Set(this);
        yield return new WaitForSeconds(1f);
        inited = true;
    }
    // Update is called once per frame
    void Update()
    { 
        if(!inited) return;
        gameService = Servise<GameService>.Get();
        playerEntity = Servise<GameService>.Get().PlayerEntity;
        if(playerEntity.IsDead()) return;
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

    private int killCount;
    public void AddKills()
    {
        killCount++;
        KillsCount.text = killCount.ToString();
    }
    public void ShowCombo(int size)
    {
        if(comboAnimPlaying) return;
            StartCoroutine(ComboAnimDelay(0.5f));
        Combo.text = size.ToString();
        comboAnimator.Play("ComboShow");
    }

    private bool comboAnimPlaying;
    private IEnumerator ComboAnimDelay(float deley)
    {
        comboAnimPlaying = true;
        yield return new WaitForSeconds(deley);
        comboAnimPlaying = false;
    }
    public void GameOver()
    {
        _gameOver.SetActive(true);
    }
}
