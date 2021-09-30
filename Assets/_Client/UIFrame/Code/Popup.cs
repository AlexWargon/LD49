using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : Showable
{
    [SerializeField] protected Image Background;
    public override void Show(Action callback = null)
    {
        gameObject.SetActive(true);
        callback?.Invoke();
    }

    public override void Hide(Action callback = null)
    {
        gameObject.SetActive(false);
        callback?.Invoke();
    }

    public override void PlayShowAnimation(Action callback = null)
    {
        callback?.Invoke();
    }

    public override void PlayHideAnimation(Action callback = null)
    {
        callback?.Invoke();
    }
}
