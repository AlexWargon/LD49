using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : Showable
{
    
    public override void Show(Action callback = null)
    {
        SetActive(true);
        PlayShowAnimation(callback);
    }

    public override void Hide(Action callback = null)
    {
        
        PlayHideAnimation(() =>
        {
            callback+= () => SetActive(false);
            callback?.Invoke();
        });
        
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
