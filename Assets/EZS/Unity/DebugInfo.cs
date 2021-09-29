﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wargon.ezs;

public class DebugInfo
{
    private List<ISystemListener> systemListeners;
    
    public DebugInfo(Wargon.ezs.World world)
    {
        systemListeners = new List<ISystemListener>();
        var systemsPool = world.GetAllSystems();
        var worldDebug = new GameObject("ECS World Debug").AddComponent<WorldDebug>();
        SceneManager.sceneUnloaded += scnene =>
        {
            if (!worldDebug) return;
            if(worldDebug.gameObject!=null)
                Object.Destroy(worldDebug.gameObject);
        };
        Object.DontDestroyOnLoad(worldDebug);
        worldDebug.world = world;
        worldDebug.transform.SetSiblingIndex(0);
        Debug.Log($"systems count {systemsPool.Count}");
        for (var i = 0; i < systemsPool.Count; i++)
        {
            var systems = systemsPool[i];
            var newListener = new SystemsDebug(systems, world);
            systems.SetListener(newListener);
            systemListeners.Add(newListener);
        }
    }

}

