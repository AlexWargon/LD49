using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public float SpawnTimer;
    public float CurrentSpawnTimer;
    public Transform[] spawnPoint;
    public MonoEntity MeleeEnemy;
    public MonoEntity RangeEnemy;
    public int PoolSize;
    public float YHeight = 2f;
    private Queue<MonoEntity> EnemyPool = new Queue<MonoEntity>();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SpawnEnemies()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        for(var i = 0; i < PoolSize; i++)
        {
            SpawnSingleEnemy();
            yield return null;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SpawnSingleEnemy()
    {
        var random = Random.Range(0, spawnPoint.Length - 1);
        var spawner = spawnPoint[random];
        if (EnemyPool.Count > 0)
        {
            var enemy = EnemyPool.Dequeue();
            enemy.Entity.Remove<Dead>();
            enemy.Entity.Remove<UnActive>();
            enemy.Entity.Remove<DeathState>();

            enemy.gameObject.SetActive(true);
            
            var x = Random.Range(-14, 14);
            var z = Random.Range(-14, 14);
            var spawnpos = new Vector3(spawner.position.x + x, 2.51f,spawner.position.z + z);
            var transform1 = enemy.transform;
            transform1.position = spawnpos;
            transform1.rotation = Quaternion.identity;
            enemy.Entity.Set<CanRotate>();
            enemy.Entity.Set<CanRun>();
            enemy.Entity.Get<ColliderRef>().Value.enabled = true;
            enemy.Entity.Get<EnemyRef>().NavMeshAgentVelue.enabled = true;
            enemy.Entity.Add(new EnemySpawnEvent());
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BackToPool(MonoEntity entity, StayDeadCountDown countDown)
    {
        entity.Entity.Set<UnActive>();
        entity.gameObject.SetActive(false);
        EnemyPool.Enqueue(entity);
        countDown.Value = countDown.Default;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Init()
    {
        StartCoroutine(InitPools());
    }

    private IEnumerator InitPools()
    {
        var thisTransform = transform;
        var chunk = 25;
        var spawnVector = new Vector3(thisTransform.position.x, YHeight, thisTransform.position.z);
        var rangeEnemyEventSpawn = 0;
        for (int i = 0; i < PoolSize; i++)
        {
            if (rangeEnemyEventSpawn == 15)
            {
                var monoEntity = Instantiate(RangeEnemy, spawnVector, Quaternion.identity, thisTransform);
                monoEntity.ConvertToEntity();
                //monoEntity.Get<EnemyRef>().NavMeshAgentVelue.enabled = false;
                monoEntity.Entity.Set<Dead>();
                monoEntity.Entity.Set<UnActive>();
                monoEntity.gameObject.SetActive(false);
                EnemyPool.Enqueue(monoEntity);
                rangeEnemyEventSpawn = 0;
            }
            else
            {
                var monoEntity = Instantiate(MeleeEnemy, spawnVector, Quaternion.identity, thisTransform);
                monoEntity.ConvertToEntity();
                //monoEntity.Get<EnemyRef>().NavMeshAgentVelue.enabled = false;
                monoEntity.Entity.Set<Dead>();
                monoEntity.Entity.Set<UnActive>();
                monoEntity.gameObject.SetActive(false);
                EnemyPool.Enqueue(monoEntity);
            }

            chunk--;
            
            if (chunk == 0)
            {
                chunk = 25;
                yield return null;
            }

            rangeEnemyEventSpawn++;
        }

    }
    private void Update()
    {
        CurrentSpawnTimer -= Time.deltaTime;
        if (CurrentSpawnTimer <= 0)
        {
            SpawnEnemies();
            CurrentSpawnTimer = SpawnTimer;
        }
    }
}
