using UnityEngine;
using Wargon.ezs.Unity;

public class EnemyExplosionTrigger : MonoBehaviour
{
    private MonoEntity entity;
    private void Start()
    {
        entity = GetComponent<MonoEntity>();
    }
    private void OnTriggerEnter(Collider other)
    {
        var mono = other.GetComponent<MonoEntity>();
        if (!mono) return;
        if(mono.Entity.Has<Health>())
        {
            var damaged = new Damaged();
            damaged.Damage = entity.Entity.Get<Damage>().Value;
            mono.Entity.Add(damaged);
        }
    }
}