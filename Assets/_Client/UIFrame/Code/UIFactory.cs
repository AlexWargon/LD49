using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class UIFactory
{
    private readonly Dictionary<Type, Showable> _showables = new Dictionary<Type, Showable>();
    private readonly Dictionary<Type, Showable> _instantiatedShowables = new Dictionary<Type, Showable>();
    private readonly UIConfig _uiConfig;
    
    public UIFactory(UIConfig uiConfig) {
        _uiConfig = uiConfig;
        foreach (var uiElement in _uiConfig.uiElements) {
            _showables.Add(uiElement.GetType(), uiElement);
        }
    }

    public T Create<T>() where T : Showable {
        var type = typeof(T);
        if (_instantiatedShowables.ContainsKey(type)) return (T) _instantiatedShowables[type];
        var showable = Object.Instantiate(_showables[type]);
        _instantiatedShowables.Add(type, showable);
        return (T)showable;
    }
}
