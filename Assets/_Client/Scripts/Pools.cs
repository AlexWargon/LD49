using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

public class Pools : MonoBehaviour
{
    [SerializeField] private int PoolSizeDefault = 256;
    private static Pools instance;

    public static Vector3 UnActivePos = new Vector3(-100000f, -100000f, 0);
    [SerializeField] private List<PoolContainer> poolContainers = new List<PoolContainer>();
    private readonly Dictionary<int, Queue<Entity>> entityPool = new Dictionary<int, Queue<Entity>>();
    private readonly Dictionary<string, PoolContainer> poolMap = new Dictionary<string, PoolContainer>();
    
    private static Pools Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<Pools>();

            if (instance == null)
            {
                instance = new GameObject().AddComponent<Pools>();
                instance.transform.position = new Vector3(1000, 1000, 0);
                instance.name = "POOLS";
            }

            return instance;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Transform GetContainerNonStatic(int id)
    {
        return poolContainers[id].transform;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Transform GetContainer(int id)
    {
        return Instance.GetContainerNonStatic(id);
    }  
    #region ENTITY POOL
    
    private int poolIncreaseInFrame = 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CalledInFrameAlready()
    {
        return poolIncreaseInFrame > 0;
    }

    private IEnumerator CheckFrames()
    {
        while (true)
        {
            yield return null;
            poolIncreaseInFrame = 0;
        }
    }

    private void Start()
    {
        StartCoroutine(CheckFrames());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateEntityPool(MonoEntity prefab, int poolSize)
    {
        Instance.CreateEntityPoolNonStatic(prefab, poolSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CreateEntityPoolNonStatic(MonoEntity prefab, int poolSize)
    {
        var poolKey = prefab.GetInstanceID();

        if (!entityPool.ContainsKey(poolKey))
        {
            entityPool.Add(poolKey, new Queue<Entity>());

            var poolHolder = new PoolContainer(new GameObject(),
                poolSize, prefab.name, poolKey, poolContainers.Count) {transform = {parent = transform}};
            poolContainers.Add(poolHolder);
            poolMap.Add(prefab.name, poolHolder);
            for (var i = 0; i < poolSize; i++)
            {
                var newEntity = Instantiate(prefab);
                newEntity.ConvertToEntity();
                var pooled = newEntity.Entity.Get<Pooled>();
                pooled.SetActive(false);
                pooled.containerIndex = poolHolder.index;
                entityPool[poolKey].Enqueue(newEntity.Entity);
                pooled.SetParent(poolHolder.transform);
                pooled.SetName($"[PoolObject] {prefab.name} ID:{pooled.mono.id.ToString()}");
            }

            print($"<color=yellow> POOL OF {poolSize} [{poolHolder.Name}] CREATED </color>");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CreateEntityPoolNonStatic(MonoEntity prefab, int poolSize, Transform parent)
    {
        var poolKey = prefab.GetInstanceID();

        if (!entityPool.ContainsKey(poolKey))
        {
            entityPool.Add(poolKey, new Queue<Entity>());

            var poolHolder = new PoolContainer(new GameObject(), poolSize, prefab.name, poolKey, poolContainers.Count);
            poolHolder.transform.SetParent(parent);
            poolContainers.Add(poolHolder);
            poolMap.Add(prefab.name, poolHolder);
            for (var i = 0; i < poolSize; i++)
            {
                var newEntity = Instantiate(prefab);
                newEntity.ConvertToEntity();
                var pooled = newEntity.Entity.Get<Pooled>();
                pooled.SetActive(false);
                pooled.containerIndex = poolHolder.index;
                entityPool[poolKey].Enqueue(newEntity.Entity);
                pooled.SetParent(poolHolder.transform);
#if UNITY_EDITOR
                pooled.SetName($"[PoolObject] {prefab.name} ID:{pooled.mono.id.ToString()}");
#endif
            }

            print($"<color=yellow> POOL OF {poolSize} [{poolHolder.Name}] CREATED </color>");
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Entity ReuseEntity(MonoEntity prefab, Vector3 position)
    {
        return Instance.ReuseEntityNonStatic(prefab, position, Quaternion.identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Entity ReuseEntity(MonoEntity prefab, Vector3 position, Quaternion rotation)
    {
        return Instance.ReuseEntityNonStatic(prefab, position, rotation);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Entity ReuseEntityNonStatic(MonoEntity prefab, Vector3 position, Quaternion rotation)
    {
        var poolKey = prefab.GetInstanceID();

        if (entityPool.ContainsKey(poolKey))
        {
            var poolObject = entityPool[poolKey].Dequeue().Get<Pooled>();
            entityPool[poolKey].Enqueue(poolObject.mono.Entity);

            if (!poolObject.IsActive)
            {
                poolObject.Reuse(position, rotation);
                return poolObject.mono.Entity;
            }

            AddPoolSize(prefab, 16, poolObject.containerIndex);
            return ReuseEntityNonStatic(prefab, position, rotation);
        }

        CreateEntityPool(prefab, PoolSizeDefault);
        return ReuseEntityNonStatic(prefab, position, rotation);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Entity ReuseEntity(MonoEntity prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        return Instance.ReuseEntityNonStatic(prefab, position, rotation,parent);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Entity ReuseEntityNonStatic(MonoEntity prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        var poolKey = prefab.GetInstanceID();

        if (entityPool.ContainsKey(poolKey))
        {
            var poolObject = entityPool[poolKey].Dequeue().Get<Pooled>();
            entityPool[poolKey].Enqueue(poolObject.mono.Entity);

            if (!poolObject.IsActive)
            {
                poolObject.Reuse(position, rotation);
                return poolObject.mono.Entity;
            }

            AddPoolSize(prefab, 16, poolObject.containerIndex);
            return ReuseEntityNonStatic(prefab, position, rotation, parent);
        }

        CreateEntityPoolNonStatic(prefab, PoolSizeDefault, parent);
        return ReuseEntityNonStatic(prefab, position, rotation, parent);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Entity ReuseEntity(string name, Vector3 position, Quaternion rotation) {
        return Instance.ReuseEntityNonStatic(name, position, rotation);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity ReuseEntityNonStatic(string name, Vector3 position, Quaternion rotation) {
        if (poolMap.ContainsKey(name))
        {
            var poolKey = poolMap[name].poolKey;
            if (entityPool.ContainsKey(poolKey)) {
                var poolObject = entityPool[poolKey].Dequeue().Get<Pooled>();
                entityPool[poolKey].Enqueue(poolObject.mono.Entity);
                if (!poolObject.IsActive) {
                    poolObject.Reuse(position, rotation);
                    return poolObject.mono.Entity;
                }
            }
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddPoolSize(MonoEntity prefab, int addPoolSize, int containerIndex)
    {
        var poolKey = prefab.GetInstanceID();

        if (entityPool.ContainsKey(poolKey))
        {
            var poolHolder = poolContainers[containerIndex];
            for (var i = 0; i < addPoolSize; i++)
            {
                var newEntity = Instantiate(prefab);
                newEntity.ConvertToEntity();
                var newObject = newEntity.Entity.Get<Pooled>();
                newObject.containerIndex = containerIndex;
                Instance.entityPool[poolKey].Enqueue(newObject.mono.Entity);
                newObject.SetParent(poolHolder.transform);
                #if UNITY_EDITOR
                newObject.SetName($"[PoolObject] {prefab.name} ID:{newObject.mono.id.ToString()}");
                #endif
                newObject.SetActive(false);
                poolHolder.Add();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Destroy(MonoEntity prefab)
    {
        Instance.DestroyNonStatic(prefab);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyNonStatic(MonoEntity prefab)
    {
        var poolKey = prefab.GetInstanceID();
        if (Instance.entityPool.ContainsKey(poolKey))
        {
            Destroy(poolContainers[entityPool[poolKey].Dequeue().Get<Pooled>().containerIndex].transform.gameObject);
            poolContainers.RemoveAt(entityPool[poolKey].Dequeue().Get<Pooled>().containerIndex);
            entityPool[poolKey].Clear();
            entityPool.Remove(poolKey);
        }
    }

    //public EcsEntity GetPoolObject(MonoToEntity prefab) {
    //    int poolKey = prefab.GetInstanceID();
    //    if (entityPool.ContainsKey(poolKey)) {
    //        return entityPool[poolKey].Dequeue();
    //    }
    //    Debug.LogError($"NO POOL WITH {poolKey} KEY");
    //    return null;
    //}

    #endregion
}

[EcsComponent]
public sealed class Pooled
{
    public int containerIndex;
    public float LifeTime;
    public float CurrentLifeTime;
    public MonoEntity mono;
    public bool IsActive;
    private Vector2 UnActivePosition = new Vector2(-100000, -100000);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetParent(Transform parent)
    {
        mono.transform.SetParent(parent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetName(string name)
    {
        mono.transform.gameObject.name = name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetActive(bool value)
    {
        IsActive = value;
        if (IsActive)
        {
            mono.Entity.Remove<UnActive>();
            mono.Entity.Set<PooledEvent>();
            CurrentLifeTime = LifeTime;
        }
        else
        {
            mono.Entity.Set<UnActive>();
            mono.Entity.Set<BackToPoolEvent>();
        }
        mono.SetActive(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reuse(Vector3 position, Quaternion rotation)
    {
        mono.transform.position = position;
        mono.transform.rotation = rotation;
        SetActive(true);
        if (mono.Entity.Has<TransformComponent>())
        {
            ref var transformPure = ref mono.Entity.GetRef<TransformComponent>();
            transformPure.position = position;
            transformPure.rotation = rotation;
            transformPure.scale = Vector3.one;
        }
    }
}

[Serializable]
public class PoolContainer
{
    [HideInInspector] public Transform transform;
    public int size;
    public int index;
    public int poolKey;

    public PoolContainer(GameObject gameObject, int size, string prefabName, int poolKey, int index)
    {
        transform = gameObject.transform;
        this.size = size;
        this.Name = prefabName;
        this.poolKey = poolKey;
        this.index = index;
        Set();
    }

    public string Name { get; }

    private void Set()
    {
        transform.name = $"[Pool] {Name} [KeyID:{poolKey}]  Size:{size}";
    }

    public void Add()
    {
        size++;
        transform.name = $"[Pool] {Name} [KeyID:{poolKey}]  Size:{size}";
    }
}
[EcsComponent] public struct BackToPoolEvent{}
[EcsComponent] public struct PooledEvent{}

[EcsComponent] public class TransformPure
{
    public Entity parent;
    public Entity child;
    public Vector3 position;
    public Vector3 direction;
    public Quaternion rotation;
    public Vector2 size;
    public Vector3 right => rotation * Vector3.right;
}
[EcsComponent] public class PurePoolComponent
{
    public float lifeTime;
    public float currentLifeTime;
    public Entity entity;
    public bool active;

    public PurePoolComponent(float lifeTime, float currentLifeTime, int entityID, bool active)
    {
        this.lifeTime = lifeTime;
        this.currentLifeTime = currentLifeTime;
        var e = GameEcs.World.GetEntity(entityID);
        entity = e;
        this.active = active;
        e.Add(this);
    }
    public PurePoolComponent(){}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetActive(bool value)
    {
        active = value;
        if (value)
            entity.Remove<UnActive>();
        else
            entity.Add(new UnActive());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Spawn(Vector2 pos, Quaternion rotation)
    {
        var transform = entity.Get<TransformPure>();
        transform.rotation = rotation;
        transform.position = pos;
        currentLifeTime = lifeTime;
        SetActive(true);
    }

}
 public class EntityPoolFast
{
    private readonly Dictionary<int, Queue<Entity>> entityPool = new Dictionary<int, Queue<Entity>>();
    private readonly Dictionary<int, MonoEntity> monoEntities = new Dictionary<int, MonoEntity>();
    private World world => GameEcs.World;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CreatPool(MonoEntity prefab, int size)
    {
        var poolKey = prefab.GetInstanceID();
        
        if (!entityPool.ContainsKey(poolKey)) {
            Debug.Log($"POOL {prefab.name} EMPTY!");
            entityPool.Add(poolKey, new Queue<Entity>());
            var monoEntity = UnityEngine.Object.Instantiate(prefab);
            monoEntity.gameObject.SetActive(false);
            monoEntities.Add(poolKey, monoEntity);
            
            for (var i = 0; i < size; i++)
            {
                var entity = world.CreateEntity();
                foreach (var newEntityComponent in monoEntity.Components)
                    entity.AddBoxed(newEntityComponent);
                
                var newObject = new PurePoolComponent(2, 2, entity.id, false);
                entityPool[poolKey].Enqueue(newObject.entity);
                newObject.SetActive(false);
            }
            Debug.Log($"POOL {prefab.name} CREATED");
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Spawn(MonoEntity prefab, Vector2 pos, Quaternion rotation)
    {
        var poolKey = prefab.GetInstanceID();

        if (entityPool.ContainsKey(poolKey)) {
            var poolObject = entityPool[poolKey].Dequeue().Get<PurePoolComponent>();
            entityPool[poolKey].Enqueue(poolObject.entity);

            if (!poolObject.active) {
                poolObject.Spawn(pos, rotation);
            }
            else
            {
                //Log.Show(Color.magenta, $"POOL {prefab.name} NOT ENOUGH ENTITIES! TRYING CREATE NEW");
                var monoEntity = monoEntities[poolKey];
                var entity = world.CreateEntity();
                foreach (var newEntityComponent in monoEntity.Components)
                    entity.AddBoxed(newEntityComponent);
                
                var newObject = new PurePoolComponent(2, 2, entity.id, false);
                entityPool[poolKey].Enqueue(newObject.entity);
                newObject.Spawn(pos,rotation);
            }
        }
    }
}
public class PurePoolQueue<T>
    {
        public EntityType<T, PurePoolComponent, UnActive> entityType;
        private int last;
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PurePoolComponent GetNext()
        {
            last++;
            if (last > entityType.Count)
                last = 0;
            return entityType.poolB.items[entityType.entities[last]];
        }
    }
    public class EntityPoolFast<T>
    {
        private readonly Dictionary<int, MonoEntity> monoEntities = new Dictionary<int, MonoEntity>();
        private readonly Dictionary<int, PurePoolQueue<T>> entitiesQueues = new Dictionary<int, PurePoolQueue<T>>();
        private World world => GameEcs.World;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreatPool(MonoEntity prefab, int size)
        {
            var poolKey = prefab.GetInstanceID();
            
            if (!entitiesQueues.ContainsKey(poolKey)) {
                Debug.Log( $"POOL {prefab.name} EMPTY!");
                
                var entityType = world.Entities.GetEntityType<T, PurePoolComponent, UnActive>();
                var newQueue = new PurePoolQueue<T>();
                newQueue.entityType = entityType;
                entitiesQueues.Add(poolKey, newQueue);

                for (var i = 0; i < size; i++)
                {
                    //var entity = world.CreateEntity();
                    var monoEntity = UnityEngine.Object.Instantiate(prefab);
                    monoEntity.ConvertToEntity();
                    var entity = monoEntity.Entity;
                    var newObject = entity.Get<PurePoolComponent>();
                    newObject.entity = entity;
                    newObject.SetActive(false);
                    monoEntity.DestroyWithoutEntity();
                }
                Debug.Log($"POOL {prefab.name} CREATED");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Spawn(MonoEntity prefab, Vector2 pos, Quaternion rotation)
        {
            var poolKey = prefab.GetInstanceID();

            if (entitiesQueues.ContainsKey(poolKey)) {
                var pool = entitiesQueues[poolKey].GetNext();
                
                if (!pool.active)
                {
                    pool.Spawn(pos, rotation);
                }
                else
                {
                    //Log.Show(Color.magenta, $"POOL {prefab.name} NOT ENOUGH ENTITIES! TRYING CREATE NEW");
                    // var monoEntity = monoEntities[poolKey];
                    // var entity = world.CreateEntity();
                    // foreach (var newEntityComponent in monoEntity.Components)
                    //     entity.AddBoxed(newEntityComponent);
                    // var newObject = new PurePoolComponent(2, 2, entity.id, false);
                    // newObject.Spawn(pos,rotation);
                    for (var i = 0; i < 12; i++)
                    {
                        //var entity = world.CreateEntity();
                        var monoEntity = UnityEngine.Object.Instantiate(prefab);
                        monoEntity.ConvertToEntity();
                        var entity = monoEntity.Entity;
                        var newObject = entity.Get<PurePoolComponent>();
                        newObject.entity = entity;
                        newObject.SetActive(false);
                        monoEntity.DestroyWithoutEntity();
                    }
                }
            }
            else
            {
                CreatPool(prefab, 24);
            }
        }
        static object CreateDeepCopy(object obj)
        {
            using (var ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(ms);
            }
        }
    }