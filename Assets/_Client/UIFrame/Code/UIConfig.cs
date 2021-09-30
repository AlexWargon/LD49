using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig", menuName = "SO/UIConfig", order = 1)]
public class UIConfig : ScriptableObject
{
    public List<Showable> uiElements;
    
}
