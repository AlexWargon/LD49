using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

public static class EcsExtensions
{
    public static MonoEntity GetMonoEntity(this GameObject gameObject)
    {
        return gameObject.GetComponent<MonoEntity>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A>(this Entities ezs, ref AExecutor jobExecute)
        where A : unmanaged where AExecutor : unmanaged, IJobExecute<A>
    {
        var entityType = ezs.GetEntityType<A>();
        var entities = entityType.entities;
        EachWithJob<A, AExecutor> job = default;
        job.Set(entities, ref entityType.poolA.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B>(this Entities ezs, ref AExecutor jobExecute) where A : unmanaged
        where B : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B>
    {
        var entityType = ezs.GetEntityType<A, B>();
        var entities = entityType.entities;
        EachWithJob<A, B, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B, NA>(this Entities.EntitiesWithout<NA> ezs, ref AExecutor jobExecute) where A : unmanaged
        where B : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B>
    {
        var entityType = ezs.GetEntityType<A, B>();
        var entities = entityType.entities;
        EachWithJob<A, B, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B, NA, NB>(this Entities.EntitiesWithout<NA,NB> ezs, ref AExecutor jobExecute) where A : unmanaged
        where B : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B>
    {
        var entityType = ezs.GetEntityTypeWithout<A, B>();
        var entities = entityType.entities;
        EachWithJob<A, B, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B, C>(this Entities ezs, ref AExecutor jobExecute) where A : unmanaged
        where B : unmanaged
        where C : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B, C>
    {
        var entityType = ezs.GetEntityType<A, B, C>();
        var entities = entityType.entities;
        EachWithJob<A, B, C, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, entityType.poolС.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B,C, NA, NB>(this Entities.EntitiesWithout<NA,NB> ezs, ref AExecutor jobExecute) where A : unmanaged
        where B : unmanaged
        where C : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B, C>
    {
        var entityType = ezs.GetEntityTypeWithout<A, B, C>();
        var entities = entityType.entities;
        EachWithJob<A, B, C, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, entityType.poolС.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B, C, D>(this Entities ezs, ref AExecutor jobExecute)
        where A : unmanaged
        where B : unmanaged
        where C : unmanaged
        where D : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B, C, D>
    {
        var entityType = ezs.GetEntityType<A, B, C, D>();
        var entities = entityType.entities;
        EachWithJob<A, B, C, D, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, entityType.poolС.items,
            entityType.poolD.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
    }

    public static void Each<A>(this Entities ezs, LambdaRef<A> lambda)
    {
        var entityType = ezs.GetEntityType<A>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]]);
    }

    public static void Each<A,B,C,NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRRC<A,B,C> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A,B,C>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                c[entities[index]]);
    }
    public static void Each<A,B,NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRef<A,B> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A,B>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]]);
    }
    public static void Each<A,B,C,NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRef<A,B,C> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A,B,C>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]]);
    }
    public static void Each<A,B,C,D,NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRef<A,B,C,D> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A,B,C,D>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        var d = entityType.poolD.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]],
                ref d[entities[index]]);
    }
    public static void Each<A,B,C,D,E,NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRef<A,B,C,D,E> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A,B,C,D,E>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        var d = entityType.poolD.items;
        var e = entityType.poolE.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]],
                ref d[entities[index]],
                ref e[entities[index]]);
    }
        public static void Each<A,B,NA,NB>(this Entities.EntitiesWithout<NA,NB> ezs, LambdaRef<A,B> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A,B>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]]);
    }
    public static void Each<A,B,C,NA,NB>(this Entities.EntitiesWithout<NA,NB> ezs, LambdaRef<A,B,C> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A,B,C>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]]);
    }
    public static void Each<A,B,C,D,NA,NB>(this Entities.EntitiesWithout<NA,NB> ezs, LambdaRef<A,B,C,D> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A,B,C,D>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        var d = entityType.poolD.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]],
                ref d[entities[index]]);
    }
    public static void Each<A,B,C,D,E,NA,NB>(this Entities.EntitiesWithout<NA,NB> ezs, LambdaRef<A,B,C,D,E> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A,B,C,D,E>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        var d = entityType.poolD.items;
        var e = entityType.poolE.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]],
                ref d[entities[index]],
                ref e[entities[index]]);
    }
    public static void EachWithJobs<A, NA>(this Entities.EntitiesWithout<NA> ezs, Lambda<A> lambda)
    {
        var entityType = ezs.GetEntityTypeWithout<A>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(a[entities[index]]);
    }
}

[EcsComponent]
public struct TransformComponent
{
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;
    public Vector3 right;
    public Vector3 forward;
}

public interface IJobExecute<T>
    where T : unmanaged
{
    void Each(ref T t);
}

public interface IJobExecute<T, T2>
    where T : unmanaged where T2 : unmanaged
{
    void ForEach(ref T t, ref T2 t2);
}

public interface IJobExecute<T, T2, T3>
    where T : unmanaged
    where T2 : unmanaged
    where T3 : unmanaged
{
    void ForEach(ref T t, ref T2 t2, ref T3 t3);
}

public interface IJobExecute<T, T2, T3, T4>
    where T : unmanaged
    where T2 : unmanaged
    where T3 : unmanaged
    where T4 : unmanaged
{
    void Execute(ref T t, ref T2 t2, ref T3 t3, ref T4 t4);
}

public interface IJobExecute<T, T2, T3, T4, T5>
    where T : unmanaged
    where T2 : unmanaged
    where T3 : unmanaged
    where T4 : unmanaged
    where T5 : unmanaged
{
    void Execute(ref T t, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5);
}

[BurstCompile]
public struct EachWithJob<A, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A>
    where A : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int[] entites, ref A[] items, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(items);
        executionFunc = action;
    }


    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var item = ItemsA.Array[entity];
        executionFunc.Each(ref item);
        ItemsA.Array[entity] = item;
    }
}

[BurstCompile]
public struct EachWithJob<A, B, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A, B>
    where A : unmanaged
    where B : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;
    private NativeWrappedData<B> ItemsB;

    public void Set(int[] entites, A[] itemsA, B[] itemsB, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(itemsA);
        ItemsB = NativeMagic.WrapToNative(itemsB);
        executionFunc = action;
    }

    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];
        executionFunc.ForEach(ref itemA, ref itemB);
        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        #if UNITY_EDITOR
        NativeMagic.UnwrapFromNative(Entites);
        NativeMagic.UnwrapFromNative(ItemsA);
        NativeMagic.UnwrapFromNative(ItemsB);
        #endif
    }
}

[BurstCompile]
public struct EachWithJob<A, B, C, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A, B, C>
    where A : unmanaged
    where B : unmanaged
    where C : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;
    private NativeWrappedData<B> ItemsB;
    private NativeWrappedData<C> ItemsC;

    public void Set(int[] entites, A[] itemsA, B[] itemsB, C[] itemsC, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(itemsA);
        ItemsB = NativeMagic.WrapToNative(itemsB);
        ItemsC = NativeMagic.WrapToNative(itemsC);
        executionFunc = action;
    }

    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];
        var itemC = ItemsC.Array[entity];
        executionFunc.ForEach(ref itemA, ref itemB, ref itemC);
        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;
        ItemsC.Array[entity] = itemC;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
#if UNITY_EDITOR
        NativeMagic.UnwrapFromNative(Entites);
        NativeMagic.UnwrapFromNative(ItemsA);
        NativeMagic.UnwrapFromNative(ItemsB);
        NativeMagic.UnwrapFromNative(ItemsC);
#endif
    }
}

[BurstCompile]
public struct EachWithJob<A, B, C, D, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A, B, C, D>
    where A : unmanaged
    where B : unmanaged
    where C : unmanaged
    where D : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;
    private NativeWrappedData<B> ItemsB;
    private NativeWrappedData<C> ItemsC;
    private NativeWrappedData<D> ItemsD;

    public void Set(int[] entites, A[] itemsA, B[] itemsB, C[] itemsC, D[] itemsD, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(itemsA);
        ItemsB = NativeMagic.WrapToNative(itemsB);
        ItemsC = NativeMagic.WrapToNative(itemsC);
        ItemsD = NativeMagic.WrapToNative(itemsD);
        executionFunc = action;
    }

    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];
        var itemC = ItemsC.Array[entity];
        var itemD = ItemsD.Array[entity];

        executionFunc.Execute(ref itemA, ref itemB, ref itemC, ref itemD);

        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;
        ItemsC.Array[entity] = itemC;
        ItemsD.Array[entity] = itemD;
    }
}

[BurstCompile]
public struct EachWithJob<A, B, C, D, E, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A, B, C, D, E>
    where A : unmanaged
    where B : unmanaged
    where C : unmanaged
    where D : unmanaged
    where E : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;
    private NativeWrappedData<B> ItemsB;
    private NativeWrappedData<C> ItemsC;
    private NativeWrappedData<D> ItemsD;
    private NativeWrappedData<E> ItemsE;

    public void Set(int[] entites, A[] itemsA, B[] itemsB, C[] itemsC, D[] itemsD, E[] itemsE, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(itemsA);
        ItemsB = NativeMagic.WrapToNative(itemsB);
        ItemsC = NativeMagic.WrapToNative(itemsC);
        ItemsD = NativeMagic.WrapToNative(itemsD);
        ItemsE = NativeMagic.WrapToNative(itemsE);
        executionFunc = action;
    }

    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];
        var itemC = ItemsC.Array[entity];
        var itemD = ItemsD.Array[entity];
        var itemE = ItemsE.Array[entity];

        executionFunc.Execute(ref itemA, ref itemB, ref itemC, ref itemD, ref itemE);

        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;
        ItemsC.Array[entity] = itemC;
        ItemsD.Array[entity] = itemD;
        ItemsE.Array[entity] = itemE;
    }
}


/// <summary>
///     BIG THANKS FOR LEOPOTAM "https://github.com/Leopotam/ecslite-threads-unity"
/// </summary>
internal static class NativeMagic
{
    public static unsafe NativeWrappedData<T> WrapToNative<T>(T[] managedData) where T : unmanaged
    {
        fixed (void* ptr = managedData)
        {
#if UNITY_EDITOR
            var nativeData =
                NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, managedData.Length,
                    Allocator.TempJob);
            var sh = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeData, sh);
            return new NativeWrappedData<T> {Array = nativeData, SafetyHandle = sh};
#else
            return new NativeWrappedData<T> { Array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T> (ptr, managedData.Length, Allocator.None) };
#endif
        }
    }
#if UNITY_EDITOR
    public static void UnwrapFromNative<T>(NativeWrappedData<T> sh) where T : unmanaged
    {
        AtomicSafetyHandle.CheckDeallocateAndThrow(sh.SafetyHandle);
        AtomicSafetyHandle.Release(sh.SafetyHandle);
    }
#endif
}


public struct NativeWrappedData<TT> where TT : unmanaged
{
        [NativeDisableParallelForRestriction] public NativeArray<TT> Array;
#if UNITY_EDITOR
    public AtomicSafetyHandle SafetyHandle;
#endif
}


    
