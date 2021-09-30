using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wargon.ezs;

public class GameEcs : MonoBehaviour
{
    public static World World;
    private Systems updateSystems;
    private void Awake()
    {
        World = new World();
        updateSystems = new Systems(World)



            ;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (World != null)
        {
            updateSystems.OnUpdate();
        }
    }
}
