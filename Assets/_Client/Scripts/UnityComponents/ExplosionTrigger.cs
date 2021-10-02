using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wargon.ezs.Unity;


[EcsComponent]
public class SphereColliderRef
{
    public SphereCollider Value;
}

[EcsComponent]
public class ExplosionTriggerRef
{
    public ExplosionTrigger Value;
}
public class ExplosionTrigger : MonoBehaviour
{
    public bool delayStarted;
    public bool triggered;
    private MonoEntity entity;
    private void Start()
    {
        entity = GetComponent<MonoEntity>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(triggered) return;
        if (!delayStarted)
            StartCoroutine(TriggerDelay());
        delayStarted = true;
        var mono = other.GetComponent<MonoEntity>();
        if (!mono) return;
            
        if (mono.Entity.Has<CanTakeDamageByExplosion>())
        {
            Debug.Log(other.name);
            mono.Entity.Add(new DamagedByExplosion
            {
                Power = entity.Entity.Get<ExplosionPower>().Value,
                From = transform.position
            });
        }
    }

    private IEnumerator TriggerDelay()
    {
        yield return new WaitForSeconds(0.1f);
        triggered = true;
    }
}
