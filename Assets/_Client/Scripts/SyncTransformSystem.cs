using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Jobs;
using Wargon.ezs;
using Wargon.ezs.Unity;

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
            ExludeCount = 1;
            ExcludeTypes = new[] {
                ComponentType<UnActive>.ID//,
                //ComponentType<NoBurst>.ID
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
            // var PoolEx2 = world.GetPool<NoBurst>();
            // PoolEx2.OnAdd += OnAddExclude;
            // PoolEx2.OnRemove += OnRemoveExclude;
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
    public override void Init(Entities entities, World world)
    {
        base.Init(entities, world);
        transforms = new Transforms(world);
        entities.Without<UnActive>().EntityTypes.Add( typeof(Transforms), transforms);
    }

    public override void Update()
    {
        transforms.Synchronize();
    }
}
[EcsComponent] public struct NoBurst{}