using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Jobs;
using Wargon.ezs;
using Wargon.ezs.Unity;


public class UnityTransforms : ICustomPool
{
    private TransformAccessArray Array;
    public int PoolType { get; set; }

    public UnityTransforms(World world)
    {
        Array = new TransformAccessArray(world.EntityCacheSize);
        PoolType = ComponentType<UnityTransforms>.ID;
    }
    public ref TransformAccessArray GetArray()
    {
        return ref Array;
    }

    public void Clear()
    {
        Array.Dispose();
    }
}

public class Transforms<T> : EntityType where T : unmanaged
{
    private readonly Pool<T> structComponents;
    private readonly Pool<TransformRef> unityTransforms;
    private TransformAccessArray transformAccessArray;

    public Transforms(World world) : base(world)
    {
        IncludCount = 2;
        IncludTypes = new[] {
            ComponentType<T>.ID,
            ComponentType<TransformRef>.ID
        };
        
        structComponents = world.GetPool<T>();
        unityTransforms = world.GetPool<TransformRef>();
        transformAccessArray = new TransformAccessArray(world.EntityCacheSize);
        structComponents.OnAdd += OnAddInclude;
        structComponents.OnRemove += OnRemoveInclude;
        unityTransforms.OnAdd += OnAddInclude;
        unityTransforms.OnRemove += OnRemoveInclude;
        world.OnCreateEntityType(this);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal new void OnAddInclude(Entity entity)
    {
        if (HasEntity(entity)) return;
        ref var data = ref entity.GetEntityData();

        for (var i = 0; i < ExludeCount; i++)
            if (data.componentTypes.Contains(ExcludeTypes[i]))
                return;

        for (var i = 0; i < IncludCount; i++)
            if(!data.componentTypes.Contains(IncludTypes[i]))
                return;

        if (entities.Length == Count)
        {
            Array.Resize(ref entities, entities.Length << 1);
        }
        entities[Count] = entity.id;
        entitiesMap.Add(entity.id, Count);
        transformAccessArray.Add(unityTransforms.items[entity.id].Value);
        Count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private new void OnRemoveExclude(Entity entity)
    {
        if (HasEntity(entity)) return;
        ref var data = ref entity.GetEntityData();

        for (var i = 0; i < ExludeCount; i++)
            if(data.componentTypes.Contains(ExcludeTypes[i]))
                return;
        for (var i = 0; i < IncludCount; i++)
            if(!data.componentTypes.Contains(IncludTypes[i]))
                return;

        if (entities.Length == Count)
        {
            Array.Resize(ref entities, entities.Length << 1);
        }
        entities[Count] = entity.id;
        entitiesMap.Add(entity.id, Count);
        transformAccessArray.Add(unityTransforms.items[entity.id].Value);
        Count++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private new void OnRemoveInclude(Entity entity)
    {
        if (!HasEntity(entity)) return;
        var lastEntityId = entities[Count - 1];
        var indexOfEntityId = entitiesMap[entity.id];
        entitiesMap.Remove(entity.id);
        transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
        Count--;
        if (Count > indexOfEntityId)
        {
            entities[indexOfEntityId] = lastEntityId;
            entitiesMap[lastEntityId] = indexOfEntityId;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private new void OnAddExclude(Entity entity)
    {
        if (!HasEntity(entity)) return;
        var lastEntityId = entities[Count - 1];
        var indexOfEntityId = entitiesMap[entity.id];
        entitiesMap.Remove(entity.id);
        transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
        Count--;
        if (Count > indexOfEntityId)
        {
            entities[indexOfEntityId] = lastEntityId;
            entitiesMap[lastEntityId] = indexOfEntityId;
        }
    }   
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void Remove(Entity entity)
    {
        if (!entitiesMap.ContainsKey(entity.id)) return;
        var lastEntityId = entities[Count - 1];
        var indexOfEntityId = entitiesMap[entity.id];
        entitiesMap.Remove(entity.id);
        transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
        Count--;
        if (Count > indexOfEntityId)
        {
            entities[indexOfEntityId] = lastEntityId;
            entitiesMap[lastEntityId] = indexOfEntityId;
        }
    }
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnUpdate<Executor>(ref Executor jobExecutor) where Executor : unmanaged, ITransformJobExecute<T>
    {

        var data = NativeMagic.WrapToNative(structComponents.items);
        TransformSynchronizeJob<T,Executor> job;
        job.structComponents1 = data;
        job.entities = NativeMagic.WrapToNative(entities);
        job.structComponents1 = NativeMagic.WrapToNative(structComponents.items);
        job.executor = jobExecutor;
        job.Schedule(transformAccessArray).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }

    internal override void Clear()
    {
#if UNITY_EDITOR
        transformAccessArray.Dispose();
#endif
        
    }
    public class WithOut<NA> : Transforms<T>
    {
        public WithOut(World world) : base(world)
        {
            ExludeCount = 1;
            ExcludeTypes = new[]
            {
                ComponentType<NA>.ID
            };
            var pool1 = world.GetPool<NA>();
            pool1.OnAdd += OnAddExclude;
            pool1.OnRemove += OnRemoveExclude;
            
        }
    }
    public class WithOut<NA, NB> : Transforms<T>
    {
        public WithOut(World world) : base(world)
        {
            ExludeCount = 2;
            ExcludeTypes = new[] {
                ComponentType<NA>.ID,
                ComponentType<NB>.ID
            };
            var pool1 = world.GetPool<NA>();
            pool1.OnAdd += OnAddExclude;
            pool1.OnRemove += OnRemoveExclude;
            var pool2 = world.GetPool<NB>();
            pool2.OnAdd += OnAddExclude;
            pool2.OnRemove += OnRemoveExclude;
        }
    }
}
public class Transforms<T1,T2> : EntityType where T1 : unmanaged where T2 : unmanaged
{
    private readonly Pool<T1> structComponents1;
    private readonly Pool<T2> structComponents2;
    private readonly Pool<TransformRef> unityTransforms;
    private TransformAccessArray transformAccessArray;

    public Transforms(World world) : base(world)
    {
        IncludCount = 3;
        IncludTypes = new[] {
            ComponentType<T1>.ID,
            ComponentType<T2>.ID,
            ComponentType<TransformRef>.ID
        };

        structComponents1 = world.GetPool<T1>();
        structComponents2 = world.GetPool<T2>();
        unityTransforms = world.GetPool<TransformRef>();
        transformAccessArray = new TransformAccessArray(world.EntityCacheSize);

        structComponents1.OnAdd += OnAddInclude;
        structComponents1.OnRemove += OnRemoveInclude;
        structComponents2.OnAdd += OnAddInclude;
        structComponents2.OnRemove += OnRemoveInclude;
        unityTransforms.OnAdd += OnAddInclude;
        unityTransforms.OnRemove += OnRemoveInclude;
        world.OnCreateEntityType(this);
        Debug.Log($"trafnsforms with enenty {entities.Length}");
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal new void OnAddInclude(Entity entity)
    {
        if (HasEntity(entity)) return;
        ref var data = ref entity.GetEntityData();

        for (var i = 0; i < ExludeCount; i++)
            if (data.componentTypes.Contains(ExcludeTypes[i]))
                return;

        for (var i = 0; i < IncludCount; i++)
            if(!data.componentTypes.Contains(IncludTypes[i]))
                return;

        if (entities.Length == Count)
        {
            Array.Resize(ref entities, entities.Length << 1);
        }
        entities[Count] = entity.id;
        entitiesMap.Add(entity.id, Count);
        transformAccessArray.Add(unityTransforms.items[entity.id].Value);
        Count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private new void OnRemoveExclude(Entity entity)
    {
        if (HasEntity(entity)) return;
        ref var data = ref entity.GetEntityData();

        for (var i = 0; i < ExludeCount; i++)
            if(data.componentTypes.Contains(ExcludeTypes[i]))
                return;
        for (var i = 0; i < IncludCount; i++)
            if(!data.componentTypes.Contains(IncludTypes[i]))
                return;

        if (entities.Length == Count)
        {
            Array.Resize(ref entities, entities.Length << 1);
        }
        entities[Count] = entity.id;
        entitiesMap.Add(entity.id, Count);
        transformAccessArray.Add(unityTransforms.items[entity.id].Value);
        Count++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private new void OnRemoveInclude(Entity entity)
    {
        if (!HasEntity(entity)) return;
        var lastEntityId = entities[Count - 1];
        var indexOfEntityId = entitiesMap[entity.id];
        entitiesMap.Remove(entity.id);
        transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
        Count--;
        if (Count > indexOfEntityId)
        {
            entities[indexOfEntityId] = lastEntityId;
            entitiesMap[lastEntityId] = indexOfEntityId;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private new void OnAddExclude(Entity entity)
    {
        if (!HasEntity(entity)) return;
        var lastEntityId = entities[Count - 1];
        var indexOfEntityId = entitiesMap[entity.id];
        entitiesMap.Remove(entity.id);
        transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
        Count--;
        if (Count > indexOfEntityId)
        {
            entities[indexOfEntityId] = lastEntityId;
            entitiesMap[lastEntityId] = indexOfEntityId;
        }
    }   
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void Remove(Entity entity)
    {
        if (!entitiesMap.ContainsKey(entity.id)) return;
        var lastEntityId = entities[Count - 1];
        var indexOfEntityId = entitiesMap[entity.id];
        entitiesMap.Remove(entity.id);
        transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
        Count--;
        if (Count > indexOfEntityId)
        {
            entities[indexOfEntityId] = lastEntityId;
            entitiesMap[lastEntityId] = indexOfEntityId;
        }
    }
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnUpdate<Executor>(ref Executor jobExecutor) where Executor : unmanaged, ITransformJobExecute<T1,T2>
    {

        var data = NativeMagic.WrapToNative(structComponents1.items);
        TransformSynchronizeJob<T1,T2,Executor> job;
        job.structComponents1 = data;
        job.entities = NativeMagic.WrapToNative(entities);
        job.structComponents1 = NativeMagic.WrapToNative(structComponents1.items);
        job.structComponents2 = NativeMagic.WrapToNative(structComponents2.items);
        job.executor = jobExecutor;
        job.Schedule(transformAccessArray).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }

    internal override void Clear()
    {
#if UNITY_EDITOR
        transformAccessArray.Dispose();
#endif
    }
    public class WithOut<NA> : Transforms<T1,T2>
    {
        public WithOut(World world) : base(world)
        {
            ExludeCount = 1;
            ExcludeTypes = new[]
            {
                ComponentType<NA>.ID
            };
            var pool1 = world.GetPool<NA>();
            pool1.OnAdd += OnAddExclude;
            pool1.OnRemove += OnRemoveExclude;
        }
    }
    public class WithOut<NA, NB> : Transforms<T1,T2>
    {
        public WithOut(World world) : base(world)
        {
            ExludeCount = 2;
            ExcludeTypes = new[] {
                ComponentType<NA>.ID,
                ComponentType<NB>.ID
            };
            var pool1 = world.GetPool<NA>();
            pool1.OnAdd += OnAddExclude;
            pool1.OnRemove += OnRemoveExclude;
            var pool2 = world.GetPool<NB>();
            pool2.OnAdd += OnAddExclude;
            pool2.OnRemove += OnRemoveExclude;
        }
    }
}
[BurstCompile(CompileSynchronously = true)]
public struct TransformSynchronizeJob<T1,Executor> : IJobParallelForTransform 
    where T1 : unmanaged

    where Executor : unmanaged, ITransformJobExecute<T1>
{
    public NativeWrappedData<T1> structComponents1;

    public NativeWrappedData<int> entities;
    public Executor executor;
    public void Execute(int index, TransformAccess transform)
    {
        var entity = entities.Array[index];
        var component = structComponents1.Array[entity];
        executor.ForEach(ref component, ref transform);
        structComponents1.Array[entity] = component;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
#if UNITY_EDITOR
        NativeMagic.UnwrapFromNative(structComponents1);
        NativeMagic.UnwrapFromNative(entities);
#endif
    }
}
[BurstCompile(CompileSynchronously = true)]
public struct TransformSynchronizeJob<T1,T2,Executor> : IJobParallelForTransform 
    where T1 : unmanaged
    where T2 : unmanaged
    where Executor : unmanaged, ITransformJobExecute<T1,T2>
{
    public NativeWrappedData<T1> structComponents1;
    public NativeWrappedData<T2> structComponents2;
    public NativeWrappedData<int> entities;
    public Executor executor;
    public void Execute(int index, TransformAccess transform)
    {
        var entity = entities.Array[index];
        var component = structComponents1.Array[entity];
        var component2 = structComponents2.Array[entity];
        executor.ForEach(ref component,ref component2, ref transform);
        structComponents1.Array[entity] = component;
        structComponents2.Array[entity] = component2;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
#if UNITY_EDITOR
        NativeMagic.UnwrapFromNative(structComponents1);
        NativeMagic.UnwrapFromNative(structComponents2);
        NativeMagic.UnwrapFromNative(entities);
#endif
    }
}
public interface ITransformJobExecute<T>
    where T : unmanaged
{
    void ForEach(ref T component, ref TransformAccess transform);
}
public interface ITransformJobExecute<T1,T2>
    where T1 : unmanaged
    where T2 : unmanaged
{
    void ForEach(ref T1 component1, ref T2 component2, ref TransformAccess transform);
}
public class SyncTransformSystem : UpdateSystem, IJobSystemTag
{
    private class Transforms : EntityType
    {
        private readonly Pool<TransformComponent> structComponents;
        private readonly Pool<TransformRef> classComponents;
        private TransformAccessArray transformAccessArray;
        private bool disposed;
        private TransformSynchronizeJob job;
        public Transforms(World world) : base(world)
        {
            IncludCount = 2;
            IncludTypes = new[] {
                ComponentType<TransformComponent>.ID,
                ComponentType<TransformRef>.ID
            };
            ExludeCount = 2;
            ExcludeTypes = new[] {
                ComponentType<UnActive>.ID,
                ComponentType<NoBurst>.ID
            };
            structComponents = world.GetPool<TransformComponent>();
            classComponents = world.GetPool<TransformRef>();
            transformAccessArray = new TransformAccessArray(world.EntityCacheSize);
            Count = 0;
            structComponents.OnAdd += OnAddInclude;
            structComponents.OnRemove += OnRemoveInclude;
            classComponents.OnAdd += OnAddInclude;
            classComponents.OnRemove += OnRemoveInclude;
            var PoolEx = world.GetPool<UnActive>();
            PoolEx.OnAdd += OnAddExclude;
            PoolEx.OnRemove += OnRemoveExclude;
            var PoolEx2 = world.GetPool<NoBurst>();
            PoolEx2.OnAdd += OnAddExclude;
            PoolEx2.OnRemove += OnRemoveExclude;
            world.OnCreateEntityType(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal new void OnAddInclude(Entity entity)
        {
            if (HasEntity(entity)) return;
            ref var data = ref entity.GetEntityData();

            for (var i = 0; i < ExludeCount; i++)
                if (data.componentTypes.Contains(ExcludeTypes[i]))
                    return;

            for (var i = 0; i < IncludCount; i++)
                if(!data.componentTypes.Contains(IncludTypes[i]))
                    return;

            if (entities.Length == Count)
            {
                Array.Resize(ref entities, entities.Length << 1);
            }
            entities[Count] = entity.id;
            entitiesMap.Add(entity.id, Count);
            transformAccessArray.Add(classComponents.items[entity.id].Value);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private new void OnRemoveExclude(Entity entity)
        {
            if (HasEntity(entity)) return;
            ref var data = ref entity.GetEntityData();

            for (var i = 0; i < ExludeCount; i++)
                if(data.componentTypes.Contains(ExcludeTypes[i]))
                    return;
            for (var i = 0; i < IncludCount; i++)
                if(!data.componentTypes.Contains(IncludTypes[i]))
                    return;

            if (entities.Length == Count)
            {
                Array.Resize(ref entities, entities.Length << 1);
            }
            entities[Count] = entity.id;
            entitiesMap.Add(entity.id, Count);
            transformAccessArray.Add(classComponents.items[entity.id].Value);
            Count++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private new void OnRemoveInclude(Entity entity)
        {
            if (!HasEntity(entity)) return;
            var lastEntityId = entities[Count - 1];
            var indexOfEntityId = entitiesMap[entity.id];
            entitiesMap.Remove(entity.id);
            transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
            Count--;
            if (Count > indexOfEntityId)
            {
                entities[indexOfEntityId] = lastEntityId;
                entitiesMap[lastEntityId] = indexOfEntityId;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private new void OnAddExclude(Entity entity)
        {
            if (!HasEntity(entity)) return;
            var lastEntityId = entities[Count - 1];
            var indexOfEntityId = entitiesMap[entity.id];
            entitiesMap.Remove(entity.id);
            transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
            Count--;
            if (Count > indexOfEntityId)
            {
                entities[indexOfEntityId] = lastEntityId;
                entitiesMap[lastEntityId] = indexOfEntityId;
            }
        }   
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Remove(Entity entity)
        {
            if (!entitiesMap.ContainsKey(entity.id)) return;
            var lastEntityId = entities[Count - 1];
            var indexOfEntityId = entitiesMap[entity.id];
            entitiesMap.Remove(entity.id);
            transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
            Count--;
            if (Count > indexOfEntityId)
            {
                entities[indexOfEntityId] = lastEntityId;
                entitiesMap[lastEntityId] = indexOfEntityId;
            }
        }
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Synchronize()
        {
            if(disposed) return;
            var data = NativeMagic.WrapToNative(structComponents.items);
            TransformSynchronizeJob job = default;
            job.transformComponents = data;
            job.entities = NativeMagic.WrapToNative(entities);
            job.Schedule(transformAccessArray).Complete();
#if UNITY_EDITOR
            job.Clear();
#endif
        }

        internal override void Clear()
        {
            
            #if UNITY_EDITOR
            Debug.Log("TRANSFORMS DISPOSED");
            transformAccessArray.Dispose();
            disposed = true;
            #endif
            
        }

        [BurstCompile(CompileSynchronously = true)]
        struct TransformSynchronizeJob : IJobParallelForTransform
        {
            public NativeWrappedData<TransformComponent> transformComponents;
            public NativeWrappedData<int> entities;
            public void Execute(int index, TransformAccess transform)
            {
                var entity = entities.Array[index];
                
                var transformComponent = transformComponents.Array[entity];
                transformComponent.right = transformComponent.rotation * Vector3.right;
                transformComponent.forward = transformComponent.rotation * Vector3.forward;
                transform.position = transformComponent.position;
                transform.rotation = transformComponent.rotation;
                transform.localScale = transformComponent.scale;
                transformComponents.Array[entity] = transformComponent;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
#if UNITY_EDITOR
                NativeMagic.UnwrapFromNative(transformComponents);
                NativeMagic.UnwrapFromNative(entities);
#endif
            }
        }
    }
    private Transforms transforms;
    private Transforms<TransformComponent> transform;
    private JobTransform jobTransform;
    public override void Init(Entities entities, World world)
    {
        base.Init(entities, world);
        transforms = new Transforms(world);
        entities.Without<UnActive,NoBurst>().EntityTypes.Add( typeof(Transforms), transforms);
        // transform = new Transforms<TransformComponent>.WithOut<NoBurst>(world);
        // entities.Without<NoBurst>().EntityTypes.Add(type<Transforms<TransformComponent>.WithOut<NoBurst>>.Value, transform);
    }

    public override void Update()
    {
        transforms.Synchronize();
        //transform.OnUpdate(ref jobTransform);
    }
    private struct JobTransform : ITransformJobExecute<TransformComponent>
    {
        public void ForEach(ref TransformComponent component, ref TransformAccess transform)
        {
            component.right = component.rotation * Vector3.right;
            component.forward = component.rotation * Vector3.forward;
            transform.position = component.position;
            transform.rotation = component.rotation;
            transform.localScale = component.scale;
        }
    }
}
[EcsComponent] public struct NoBurst{}