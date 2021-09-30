using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    [SerializeField] private Transform _menuScreensParent;
    [SerializeField] private Transform _popupsParent;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private UIConfig _uiConfig;

    private void Awake()
    {
        if (!UIRoot.RunTime)
        {
            UIRoot.Init(_menuScreensParent, _popupsParent, _canvasGroup, _uiConfig);
            
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Destroy(this);
    }
}

public static class UIRoot
{
    private static MenuManager menuManager;
    public static MenuManager MenuManager;
    public static bool RunTime;
    public static void Init(Transform menuRoot, Transform popupRoot, CanvasGroup canvasGroup, UIConfig uiConfig)
    {
        var uiFactory = new UIFactory(uiConfig);
        MenuManager = new MenuManager(uiFactory, menuRoot, popupRoot, canvasGroup);
        RunTime = true;
    }
}
