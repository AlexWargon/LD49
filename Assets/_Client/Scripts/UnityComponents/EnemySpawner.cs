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
    public int SpawnSize;
    public float SpawnTimer;
    public float CurrentSpawnTimer;
    public Transform[] spawnPoint;
    public MonoEntity MeleeEnemy;
    public int PoolSize;
    private Queue<MonoEntity> EnemyPool = new Queue<MonoEntity>();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SpawnEnemies()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        for(var i = 0; i < SpawnSize; i++)
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
            enemy.Entity.Add(new EnemySpawnEvent());
            
            enemy.gameObject.SetActive(true);
            enemy.Get<EnemyRef>().NavMeshAgentVelue.enabled = true;
            var x = Random.Range(-20, 20);
            var z = Random.Range(-20, 20);
            var spawnpos = new Vector3(spawner.position.x +x, 4f,spawner.position.z + z);
            var transform1 = enemy.transform;
            transform1.position = spawnpos;
            transform1.rotation = Quaternion.identity;
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
        var chunk = 25;
        for (int i = 0; i < PoolSize; i++)
        {
            var monoEntity = Instantiate(MeleeEnemy, transform.position, Quaternion.identity);
            monoEntity.ConvertToEntity();
            monoEntity.Get<EnemyRef>().NavMeshAgentVelue.enabled = false;
            monoEntity.Entity.Set<Dead>();
            monoEntity.Entity.Set<UnActive>();
            monoEntity.gameObject.SetActive(false);
            EnemyPool.Enqueue(monoEntity);
            chunk--;
            if (chunk == 0)
            {
                chunk = 25;
                yield return null;
            }
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
