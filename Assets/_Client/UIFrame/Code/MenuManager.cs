using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager
{
    public Menu CurrentMenuScreen { get; private set; }
    public Popup CurrentPopup { get; private set; }

    private readonly Transform _menuScreensParent;
    private readonly Transform _popupsParent;
    private readonly CanvasGroup _canvasGroup;
    private readonly Image _fade;

    private readonly Dictionary<Type, Popup> _popups = new Dictionary<Type, Popup>();
    private readonly Dictionary<Type, Menu> _menuScreens = new Dictionary<Type, Menu>();

    private readonly UIFactory _uiFactory;
    
    public MenuManager(UIFactory uiFactory, Transform menuScreensRoot, Transform popupsRoot, CanvasGroup canvasGroup) {
        _uiFactory = uiFactory;
        _menuScreensParent = menuScreensRoot;
        _popupsParent = popupsRoot;
        _canvasGroup = canvasGroup;
    }
    
    public T ShowMenu<T>(Action onFinish = null) where T : Menu{
        if (CurrentMenuScreen != null)
            CurrentMenuScreen.SetActive(false);
        _canvasGroup.interactable = false;
        var menuScreen = _uiFactory.Create<T>();
        menuScreen.OnCreate(this);
        menuScreen.transform.SetParent(_menuScreensParent, false);
        menuScreen.transform.SetAsLastSibling();
        menuScreen.SetActive(true);

        if (!_menuScreens.ContainsKey(typeof(T)))
        {
            Debug.Log($"ADDED {typeof(T)}");
            _menuScreens.Add(typeof(T), menuScreen);
        }
        CurrentMenuScreen = menuScreen;
        CurrentMenuScreen.PlayShowAnimation(() => {
            _canvasGroup.interactable = true;
            onFinish?.Invoke();
        });
        return menuScreen;
    }

    public void HideMenu<T>(Action onFinish = null) where T: Menu {
        //if (_menuScreens.Values.Count(scr => scr.IsActive) < 1 || _menuScreens.Count < 2) return;
        var screen = GetMenuScreen<T>();
        if (screen == null) {
            Debug.LogError($"{typeof(T)} DOES NOT EXIST YET");
            return;
        }
        HideMenu(screen, onFinish);
    }

    public void HideMenu(Menu menuScreen, Action onFinish = null) {
        _canvasGroup.interactable = false;
        HideAllPopups();
        var screenToShow = _menuScreens.Values.OrderBy(e => e.RootIndex).LastOrDefault(e => !e.IsActive);
        menuScreen.Hide(() => {
            _canvasGroup.interactable = true;
            menuScreen.SetActive(false);
            menuScreen.transform.SetAsFirstSibling();           

            CurrentMenuScreen = screenToShow;
            onFinish?.Invoke();
        });
    }

    public T GetPopup<T>() where T : Popup {
        var type = typeof(T);
        return _popups.ContainsKey(type) ? (T)_popups[type] : null;
    }

    public T GetMenuScreen<T>() where T : Menu {
        var type = typeof(T);
        return (T)_menuScreens[type];
    }
    
    public T Show<T>(Action onFinish = null) where T : Popup {
        if (CurrentPopup != null)
            CurrentPopup.SetActive(false);
        _canvasGroup.interactable = false;
        var popup = _uiFactory.Create<T>();
        popup.OnCreate(this);
        popup.transform.SetParent(_popupsParent, false);
        popup.transform.SetAsLastSibling();
        popup.SetActive(true);

        if (!_popups.ContainsKey(typeof(T)))
        {
            Debug.Log($"ADDED {typeof(T)}");
            _popups.Add(typeof(T), popup);
        }
        CurrentPopup = popup;
        CurrentPopup.PlayShowAnimation(() => {
            _canvasGroup.interactable = true;
            onFinish?.Invoke();
        });
        return popup;
    }

    public void Hide<T>(Action onFinish = null) where T: Popup {
        if (_popups.Values.Count(pop => pop.IsActive) < 1) return;
        var popup = GetPopup<T>();
        if (popup == null) {
            Debug.LogError("POPUP DOES NOT EXIST YET");
            return;
        }
        Hide(popup, onFinish);
    }

    public void Hide(Popup popup, Action onFinish = null) {
        _canvasGroup.interactable = false;
        popup.PlayHideAnimation(() => {
            _canvasGroup.interactable = true;
            popup.SetActive(false);
            CurrentPopup = _popups.Values.OrderBy(pop => pop.RootIndex).LastOrDefault(pop => pop.IsActive);
            onFinish?.Invoke();
        });
    }

    public void HideAllPopups() {
        foreach (var popup in _popups.Values) {
            popup.SetActive(false);
        }
        CurrentPopup = null;
    }

    public void RestartScene()
    {
        
    }
}
