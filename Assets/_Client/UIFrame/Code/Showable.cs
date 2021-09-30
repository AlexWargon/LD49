using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Showable : MonoBehaviour
{
    protected MenuManager menuManager;

    public virtual void OnCreate(MenuManager menuManager)
    {
        this.menuManager = menuManager;
    }
    public bool IsActive => gameObject.activeSelf;

    public int RootIndex => transform.GetSiblingIndex();
    public void SetActive(bool isActive) {
        gameObject.SetActive(isActive);
    }
    public abstract void Show(Action callback = null);
    public abstract void Hide(Action callback = null);
    public abstract void PlayShowAnimation(Action callback = null);
    public abstract void PlayHideAnimation(Action callback = null);
}
