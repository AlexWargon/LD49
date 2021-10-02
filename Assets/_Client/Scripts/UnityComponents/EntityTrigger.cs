using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

public class EntityTrigger : MonoBehaviour
{
    private MonoEntity Entity;
    void Start()
    {
        Entity = GetComponent<MonoEntity>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // if(!other.CompareTag(tag)) return;
        // var mono = other.GetComponent<MonoEntity>();
        Entity.Entity.Set<Collided>();

    }
}
