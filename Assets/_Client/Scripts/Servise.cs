using System.Runtime.CompilerServices;

public static class Servise<T> where T : class
{
    private static T instance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(T t)
    {
        instance = t;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Get() => instance;
}